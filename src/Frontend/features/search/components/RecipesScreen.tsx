import { ApiError } from "@/lib/api/client";
import { getServerAccessToken } from "@/features/auth/server";
import { getCategories, getRecipes } from "@/features/search/api/recipes";
import type { RecipeCategory, RecipeFilters as RecipeFilterValues } from "@/features/search/api/recipes";
import { RecipeFilters } from "./RecipeFilters";
import { RecipeGrid } from "./RecipeGrid";
import { RecipePagination } from "./RecipePagination";
import styles from "./SearchSurfaces.module.css";

type SearchParams = Record<string, string | string[] | undefined>;

function value(params: SearchParams, key: string, fallback = ""): string {
  const entry = params[key];
  return (Array.isArray(entry) ? entry[0] : entry) ?? fallback;
}

function errorMessage(error: unknown): string {
  if (error instanceof ApiError && error.code === "VALIDATION_ERROR") {
    return "Bộ lọc hoặc số trang không hợp lệ. Hãy kiểm tra lại các giá trị.";
  }
  return "Không tải được công thức. Vui lòng thử lại sau.";
}

function queryFrom(params: SearchParams): URLSearchParams {
  const query = new URLSearchParams();
  for (const [key, entry] of Object.entries(params)) {
    if (typeof entry === "string") query.set(key, entry);
    else entry?.forEach((item) => query.append(key, item));
  }
  return query;
}

export async function RecipesScreen({ searchParams }: { searchParams: SearchParams }) {
  const filters: RecipeFilterValues = {
    categoryId: value(searchParams, "categoryId"),
    difficulty: value(searchParams, "difficulty"),
    maxCookTime: value(searchParams, "maxCookTime"),
    minServings: value(searchParams, "minServings"),
    sort: value(searchParams, "sort", "-createdAt"),
    page: value(searchParams, "page", "1"),
    pageSize: value(searchParams, "pageSize", "12"),
  };
  const accessToken = await getServerAccessToken();
  const [recipeResult, categoryResult] = await Promise.allSettled([
    getRecipes(filters, { accessToken, cache: "no-store" }),
    getCategories({ accessToken, cache: "no-store" }),
  ]);
  const categories: RecipeCategory[] = categoryResult.status === "fulfilled" ? categoryResult.value : [];
  const currentPage = Number(filters.page) || 1;

  return (
    <div className={`${styles.screen} space-y-8`}>
      <header className="border-b border-black/10 pb-6">
        <p className="text-xs font-bold uppercase text-[var(--color-tomato)]">Khám phá</p>
        <h1 className="mt-2 font-serif text-4xl font-semibold">Công thức</h1>
        <p className="mt-2 text-sm text-[var(--muted)]">Lọc theo nguyên liệu thời gian và khẩu phần phù hợp.</p>
      </header>
      <RecipeFilters
        action="/recipes"
        values={filters}
        categories={categories}
        categoryError={categoryResult.status === "rejected" ? "Danh mục chưa tải được." : undefined}
      />
      {recipeResult.status === "rejected" ? (
        <p className="border-l-4 border-[var(--color-tomato)] bg-[var(--surface-red)] px-4 py-3 text-sm" role="alert">
          {errorMessage(recipeResult.reason)}
        </p>
      ) : recipeResult.status === "fulfilled" ? (
        <>
          <p className="text-sm text-[var(--muted)]" aria-live="polite">
            {recipeResult.value.totalCount} công thức · Trang {recipeResult.value.page}/{Math.max(recipeResult.value.totalPages, 1)}
          </p>
          <RecipeGrid recipes={[...recipeResult.value.items]} />
          <RecipePagination page={currentPage} totalPages={recipeResult.value.totalPages} query={queryFrom(searchParams)} />
        </>
      ) : null}
    </div>
  );
}