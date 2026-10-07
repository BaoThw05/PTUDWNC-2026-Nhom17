import Link from "next/link";
import { ApiError } from "@/lib/api/client";
import { getServerAccessToken } from "@/features/auth/server";
import { getCategories, searchRecipes } from "@/features/search/api/recipes";
import type { RecipeCategory, RecipeFilters as RecipeFilterValues } from "@/features/search/api/recipes";
import { RecipeFilters } from "./RecipeFilters";
import { RecipeGrid } from "./RecipeGrid";
import { RecipePagination } from "./RecipePagination";
import { RecipeSearchForm } from "./RecipeSearchForm";
import styles from "./SearchSurfaces.module.css";

type SearchParams = Record<string, string | string[] | undefined>;

function value(params: SearchParams, key: string, fallback = ""): string {
  const entry = params[key];
  return (Array.isArray(entry) ? entry[0] : entry) ?? fallback;
}

function queryFrom(params: SearchParams): URLSearchParams {
  const query = new URLSearchParams();
  for (const [key, entry] of Object.entries(params)) {
    if (typeof entry === "string") query.set(key, entry);
    else entry?.forEach((item) => query.append(key, item));
  }
  return query;
}

function errorMessage(error: unknown): string {
  if (error instanceof ApiError && error.code === "SEARCH_QUERY_TOO_SHORT") {
    return "Nhập ít nhất 2 ký tự để tìm công thức.";
  }
  if (error instanceof ApiError && error.code === "VALIDATION_ERROR") {
    return "Bộ lọc hoặc số trang không hợp lệ. Hãy kiểm tra lại các giá trị.";
  }
  return "Không thể tìm công thức lúc này. Vui lòng thử lại sau.";
}

const suggestions = ["pho bo", "bun cha", "banh xeo"];

export async function SearchScreen({ searchParams }: { searchParams: SearchParams }) {
  const q = value(searchParams, "q");
  const filters: RecipeFilterValues = {
    categoryId: value(searchParams, "categoryId"),
    difficulty: value(searchParams, "difficulty"),
    maxCookTime: value(searchParams, "maxCookTime"),
    minServings: value(searchParams, "minServings"),
    page: value(searchParams, "page", "1"),
    pageSize: value(searchParams, "pageSize", "12"),
  };
  const accessToken = await getServerAccessToken();
  const [recipeResult, categoryResult] = q.trim()
    ? await Promise.allSettled([
        searchRecipes(q, filters, { accessToken, cache: "no-store" }),
        getCategories({ accessToken, cache: "no-store" }),
      ])
    : [null, await Promise.resolve({ status: "fulfilled" as const, value: [] as RecipeCategory[] })];

  const results = recipeResult?.status === "fulfilled" ? recipeResult.value : null;
  const categories = categoryResult.status === "fulfilled" ? categoryResult.value : [];
  const currentPage = Number(filters.page) || 1;

  return (
    <div className={`${styles.screen} space-y-8`}>
      <header className="border-b border-black/10 pb-6">
        <p className="text-xs font-bold uppercase text-[var(--color-tomato)]">Tìm theo tên món</p>
        <h1 className="mt-2 font-serif text-4xl font-semibold">Tìm công thức</h1>
        <p className="mt-2 text-sm text-[var(--muted)]">Tìm kiếm tiếng Việt có dấu hoặc không dấu.</p>
      </header>
      <RecipeSearchForm defaultValue={q} />
      {q.trim() ? (
        <>
          <RecipeFilters
            action="/search"
            values={filters}
            categories={categories}
            categoryError={categoryResult.status === "rejected" ? "Danh mục chưa tải được." : undefined}
            searchTerm={q}
            showSort={false}
          />
          {recipeResult?.status === "rejected" ? (
            <p className="border-l-4 border-[var(--color-tomato)] bg-[var(--surface-red)] px-4 py-3 text-sm" role="alert">
              {errorMessage(recipeResult.reason)}
            </p>
          ) : results ? (
            results.items.length ? (
              <>
                <p className="text-sm text-[var(--muted)]" aria-live="polite">
                  {results.totalCount} kết quả cho <strong className="text-[var(--foreground)]">“{q}”</strong>
                </p>
                <RecipeGrid recipes={[...results.items]} />
                <RecipePagination page={currentPage} totalPages={results.totalPages} query={queryFrom(searchParams)} />
              </>
            ) : (
              <section className="border-y border-black/10 py-8" aria-live="polite">
                <h2 className="font-serif text-2xl font-semibold">Chưa tìm thấy công thức phù hợp</h2>
                <p className="mt-2 text-sm text-[var(--muted)]">Thử một món quen thuộc:</p>
                <ul className="mt-4 flex flex-wrap gap-x-5 gap-y-2">
                  {suggestions.map((suggestion) => (
                    <li key={suggestion}>
                      <Link className="font-semibold text-[var(--color-leaf)] underline-offset-4 hover:underline" href={`/search?q=${encodeURIComponent(suggestion)}`}>
                        {suggestion}
                      </Link>
                    </li>
                  ))}
                </ul>
              </section>
            )
          ) : null}
        </>
      ) : (
        <section className="border-y border-black/10 py-8">
          <h2 className="font-serif text-2xl font-semibold">Bắt đầu với một món ăn</h2>
          <p className="mt-2 text-sm text-[var(--muted)]">Nhập ít nhất 2 ký tự; có thể dùng tiếng Việt không dấu.</p>
          <ul className="mt-4 flex flex-wrap gap-x-5 gap-y-2">
            {suggestions.map((suggestion) => (
              <li key={suggestion}>
                <Link className="font-semibold text-[var(--color-leaf)] underline-offset-4 hover:underline" href={`/search?q=${encodeURIComponent(suggestion)}`}>
                  {suggestion}
                </Link>
              </li>
            ))}
          </ul>
        </section>
      )}
    </div>
  );
}