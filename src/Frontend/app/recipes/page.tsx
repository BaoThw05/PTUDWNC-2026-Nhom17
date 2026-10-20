import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { ApiError, apiClient } from "@/lib/api/client";
import type { PagedResult } from "@/lib/api/types";
import { getCategories } from "@/features/categories/api/public-client";
import type { Category } from "@/features/categories/public-types";

export const metadata: Metadata = { title: "Công thức" };

type Recipe = {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: string | number;
  thumbnailUrl: string | null;
  categoryId: string | null;
};
type SearchParams = Record<string, string | string[] | undefined>;

const publicRequest = { accessToken: null, cache: "no-store" as const };
const valueOf = (value: string | string[] | undefined) => Array.isArray(value) ? value[0] : value ?? "";

function makeHref(params: URLSearchParams, page: number) {
  const next = new URLSearchParams(params);
  next.set("page", String(page));
  return `/recipes?${next.toString()}`;
}

function difficultyName(value: string | number) {
  if (typeof value === "number") return ["Dễ", "Trung bình", "Khó"][value] ?? "Không rõ";
  return ({ Easy: "Dễ", Medium: "Trung bình", Hard: "Khó" } as Record<string, string>)[value] ?? value;
}

function RecipeCard({ recipe }: { recipe: Recipe }) {
  return (
    <article className="overflow-hidden border border-white/10 bg-zinc-900/70">
      <Link className="group block" href={`/recipes/${encodeURIComponent(recipe.slug)}`}>
        <div className="relative aspect-[4/3] bg-zinc-900">
          {recipe.thumbnailUrl ? <Image src={recipe.thumbnailUrl} alt={`Ảnh món ${recipe.title}`} fill unoptimized sizes="(max-width: 767px) 100vw, (max-width: 1199px) 50vw, 33vw" className="object-cover transition-transform duration-300 group-hover:scale-[1.03]" /> : (
            <div className="flex h-full items-center justify-center bg-[repeating-linear-gradient(135deg,#18181b_0px,#18181b_16px,#27272a_16px,#27272a_17px)] text-sm text-zinc-400">Chưa có ảnh</div>
          )}
        </div>
        <div className="p-4">
          <div className="flex items-start justify-between gap-3">
            <h2 className="font-serif text-xl font-semibold leading-snug group-hover:text-emerald-300">{recipe.title}</h2>
            <span className="shrink-0 text-sm text-rose-300">{recipe.cookTimeMinutes} phút</span>
          </div>
          <p className="mt-2 line-clamp-2 min-h-12 text-sm leading-6 text-zinc-400">{recipe.description}</p>
          <p className="mt-3 border-t border-white/10 pt-3 text-xs text-zinc-400">{difficultyName(recipe.difficulty)} <span className="px-2 text-zinc-600">·</span>{recipe.servings} khẩu phần</p>
        </div>
      </Link>
    </article>
  );
}

export default async function RecipesPage({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const params = await searchParams;
  const categoryId = valueOf(params.categoryId);
  const difficulty = valueOf(params.difficulty);
  const maxCookTime = valueOf(params.maxCookTime);
  const minServings = valueOf(params.minServings);
  const sort = valueOf(params.sort) || "-createdAt";
  const page = Math.max(1, Number.parseInt(valueOf(params.page) || "1", 10) || 1);
  const query = new URLSearchParams({ page: String(page), pageSize: "12", sort });
  if (categoryId) query.set("categoryId", categoryId);
  if (difficulty) query.set("difficulty", difficulty);
  if (maxCookTime) query.set("maxCookTime", maxCookTime);
  if (minServings) query.set("minServings", minServings);

  const [recipesResult, categoriesResult] = await Promise.allSettled([
    apiClient.get<PagedResult<Recipe>>(`/api/v1/recipes?${query}`, publicRequest),
    getCategories(publicRequest),
  ]);
  const recipes = recipesResult.status === "fulfilled" ? recipesResult.value : null;
  const categories: Category[] = categoriesResult.status === "fulfilled" ? categoriesResult.value : [];
  const currentParams = new URLSearchParams(query);

  return (
    <main className="min-h-[70vh] rounded-2xl bg-zinc-950 p-5 text-zinc-100 sm:p-8">
      <header className="mb-8 border-b border-white/10 pb-6">
        <p className="text-xs font-bold uppercase tracking-widest text-emerald-300">Khám phá món ngon</p>
        <h1 className="mt-2 font-serif text-4xl font-semibold">Công thức</h1>
        <p className="mt-3 max-w-2xl text-zinc-400">Tìm công thức phù hợp với nguyên liệu, thời gian và khẩu vị của bạn.</p>
      </header>

      <form action="/recipes" className="mb-8 grid gap-4 border border-white/10 bg-zinc-900/60 p-4 sm:grid-cols-2 lg:grid-cols-5">
        <label className="grid gap-1 text-sm text-zinc-300">Danh mục
          <select name="categoryId" defaultValue={categoryId} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100">
            <option value="">Tất cả danh mục</option>
            {categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
        </label>
        <label className="grid gap-1 text-sm text-zinc-300">Độ khó
          <select name="difficulty" defaultValue={difficulty} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100">
            <option value="">Mọi mức độ</option><option value="Easy">Dễ</option><option value="Medium">Trung bình</option><option value="Hard">Khó</option>
          </select>
        </label>
        <label className="grid gap-1 text-sm text-zinc-300">Nấu tối đa (phút)
          <input name="maxCookTime" type="number" min="0" defaultValue={maxCookTime} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100" />
        </label>
        <label className="grid gap-1 text-sm text-zinc-300">Khẩu phần tối thiểu
          <input name="minServings" type="number" min="1" defaultValue={minServings} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100" />
        </label>
        <label className="grid gap-1 text-sm text-zinc-300">Sắp xếp
          <select name="sort" defaultValue={sort} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100">
            <option value="-createdAt">Mới nhất</option><option value="createdAt">Cũ nhất</option><option value="title">Tên A–Z</option><option value="-title">Tên Z–A</option><option value="cookTimeMinutes">Nấu nhanh trước</option><option value="-cookTimeMinutes">Nấu lâu trước</option>
          </select>
        </label>
        <div className="flex flex-wrap gap-3 sm:col-span-2 lg:col-span-5">
          <button className="h-10 bg-emerald-800 px-5 text-sm font-semibold hover:bg-emerald-700" type="submit">Áp dụng bộ lọc</button>
          <Link className="inline-flex h-10 items-center border border-white/20 px-5 text-sm hover:border-emerald-300 hover:text-emerald-300" href="/search">Tìm theo từ khóa</Link>
        </div>
      </form>

      {recipesResult.status === "rejected" ? (
        <p role="alert" className="border-l-4 border-rose-500 bg-rose-950/40 px-4 py-3 text-sm text-rose-200">{recipesResult.reason instanceof ApiError ? "Không thể tải danh sách công thức. Vui lòng thử lại sau." : "Máy chủ chưa thể tải danh sách công thức lúc này."}</p>
      ) : recipes?.items.length ? (
        <>
          <p className="mb-4 text-sm text-zinc-400">{recipes.totalCount} công thức <span className="px-1">·</span> Trang {recipes.page} / {recipes.totalPages || 1}</p>
          <div className="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">{recipes.items.map((recipe) => <RecipeCard key={recipe.id} recipe={recipe} />)}</div>
          <nav aria-label="Phân trang công thức" className="mt-8 flex items-center justify-between border-t border-white/10 pt-5 text-sm">
            {recipes.hasPreviousPage ? <Link className="text-emerald-300 hover:underline" href={makeHref(currentParams, page - 1)}>← Trang trước</Link> : <span className="text-zinc-600">← Trang trước</span>}
            <span className="text-zinc-400">{recipes.page} / {recipes.totalPages || 1}</span>
            {recipes.hasNextPage ? <Link className="text-emerald-300 hover:underline" href={makeHref(currentParams, page + 1)}>Trang sau →</Link> : <span className="text-zinc-600">Trang sau →</span>}
          </nav>
        </>
      ) : <p className="border-y border-white/10 py-8 text-zinc-400">Chưa có công thức phù hợp. Hãy thử thay đổi bộ lọc.</p>}
    </main>
  );
}
