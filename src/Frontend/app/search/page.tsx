import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { ApiError, apiClient } from "@/lib/api/client";
import type { PagedResult } from "@/lib/api/types";
import { getCategories } from "@/features/categories/api/public-client";
import type { Category } from "@/features/categories/public-types";

export const metadata: Metadata = { title: "Tìm kiếm công thức" };

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
  relevanceScore?: number | null;
};
type SearchParams = Record<string, string | string[] | undefined>;

const publicRequest = { accessToken: null, cache: "no-store" as const };
const valueOf = (value: string | string[] | undefined) => Array.isArray(value) ? value[0] : value ?? "";

function pageHref(params: URLSearchParams, page: number) {
  const next = new URLSearchParams(params);
  next.set("page", String(page));
  return `/search?${next.toString()}`;
}

function RecipeCard({ recipe }: { recipe: Recipe }) {
  const difficulty = typeof recipe.difficulty === "number"
    ? ["Dễ", "Trung bình", "Khó"][recipe.difficulty] ?? "Không rõ"
    : ({ Easy: "Dễ", Medium: "Trung bình", Hard: "Khó" } as Record<string, string>)[recipe.difficulty] ?? recipe.difficulty;
  return (
    <article className="overflow-hidden border border-white/10 bg-zinc-900/70">
      <Link className="group block" href={`/recipes/${encodeURIComponent(recipe.slug)}`}>
        <div className="relative aspect-[4/3] bg-zinc-900">
          {recipe.thumbnailUrl ? <Image src={recipe.thumbnailUrl} alt={`Ảnh món ${recipe.title}`} fill unoptimized sizes="(max-width: 767px) 100vw, (max-width: 1199px) 50vw, 33vw" className="object-cover transition-transform duration-300 group-hover:scale-[1.03]" /> : <div className="flex h-full items-center justify-center bg-[repeating-linear-gradient(135deg,#18181b_0px,#18181b_16px,#27272a_16px,#27272a_17px)] text-sm text-zinc-400">Chưa có ảnh</div>}
        </div>
        <div className="p-4">
          <div className="flex items-start justify-between gap-3"><h2 className="font-serif text-xl font-semibold leading-snug group-hover:text-emerald-300">{recipe.title}</h2><span className="shrink-0 text-sm text-rose-300">{recipe.cookTimeMinutes} phút</span></div>
          <p className="mt-2 line-clamp-2 min-h-12 text-sm leading-6 text-zinc-400">{recipe.description}</p>
          <p className="mt-3 border-t border-white/10 pt-3 text-xs text-zinc-400">{difficulty}<span className="px-2 text-zinc-600">·</span>{recipe.servings} khẩu phần</p>
        </div>
      </Link>
    </article>
  );
}

export default async function SearchPage({ searchParams }: { searchParams: Promise<SearchParams> }) {
  const params = await searchParams;
  const q = valueOf(params.q).trim();
  const categoryId = valueOf(params.categoryId);
  const difficulty = valueOf(params.difficulty);
  const maxCookTime = valueOf(params.maxCookTime);
  const minServings = valueOf(params.minServings);
  const page = Math.max(1, Number.parseInt(valueOf(params.page) || "1", 10) || 1);
  const query = new URLSearchParams({ q, page: String(page), pageSize: "10" });
  if (categoryId) query.set("categoryId", categoryId);
  if (difficulty) query.set("difficulty", difficulty);
  if (maxCookTime) query.set("maxCookTime", maxCookTime);
  if (minServings) query.set("minServings", minServings);

  const [searchResult, categoriesResult] = await Promise.allSettled([
    q.length >= 2 ? apiClient.get<PagedResult<Recipe>>(`/api/v1/recipes/search?${query}`, publicRequest) : Promise.resolve(null),
    getCategories(publicRequest),
  ]);
  const results = searchResult.status === "fulfilled" ? searchResult.value : null;
  const categories: Category[] = categoriesResult.status === "fulfilled" ? categoriesResult.value : [];
  const currentParams = new URLSearchParams(query);
  const validationError = searchResult.status === "rejected" && searchResult.reason instanceof ApiError && searchResult.reason.status === 422;

  return (
    <main className="min-h-[70vh] rounded-2xl bg-zinc-950 p-5 text-zinc-100 sm:p-8">
      <header className="mb-8 border-b border-white/10 pb-6">
        <p className="text-xs font-bold uppercase tracking-widest text-rose-300">Tìm món bạn muốn nấu</p>
        <h1 className="mt-2 font-serif text-4xl font-semibold">Tìm kiếm</h1>
        <p className="mt-3 max-w-2xl text-zinc-400">Tìm theo tên món hoặc mô tả; tìm kiếm hỗ trợ tiếng Việt có dấu và không dấu.</p>
      </header>

      <form action="/search" className="mb-8 space-y-4 border border-white/10 bg-zinc-900/60 p-4">
        <label className="grid gap-2 text-sm font-medium text-zinc-300" htmlFor="recipe-search">Từ khóa
          <div className="flex flex-col gap-2 sm:flex-row">
            <input id="recipe-search" name="q" type="search" minLength={2} required defaultValue={q} placeholder="Ví dụ: phở bò, pho bo…" className="h-12 min-w-0 flex-1 border border-white/15 bg-zinc-950 px-4 text-base text-zinc-100 placeholder:text-zinc-500" />
            <button type="submit" className="h-12 bg-emerald-800 px-7 font-semibold hover:bg-emerald-700">Tìm công thức</button>
          </div>
        </label>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <label className="grid gap-1 text-sm text-zinc-400">Danh mục<select name="categoryId" defaultValue={categoryId} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100"><option value="">Tất cả danh mục</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label>
          <label className="grid gap-1 text-sm text-zinc-400">Độ khó<select name="difficulty" defaultValue={difficulty} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100"><option value="">Mọi mức độ</option><option value="Easy">Dễ</option><option value="Medium">Trung bình</option><option value="Hard">Khó</option></select></label>
          <label className="grid gap-1 text-sm text-zinc-400">Nấu tối đa (phút)<input name="maxCookTime" type="number" min="0" defaultValue={maxCookTime} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100" /></label>
          <label className="grid gap-1 text-sm text-zinc-400">Khẩu phần tối thiểu<input name="minServings" type="number" min="1" defaultValue={minServings} className="h-10 border border-white/15 bg-zinc-950 px-3 text-zinc-100" /></label>
        </div>
      </form>

      {!q ? <p className="border-y border-white/10 py-8 text-zinc-400">Nhập ít nhất 2 ký tự để tìm công thức.</p>
        : q.length < 2 ? <p className="border-l-4 border-amber-400 bg-amber-950/30 px-4 py-3 text-sm text-amber-200" role="status">Từ khóa cần có ít nhất 2 ký tự.</p>
          : searchResult.status === "rejected" ? <p className="border-l-4 border-rose-500 bg-rose-950/40 px-4 py-3 text-sm text-rose-200" role="alert">{validationError ? "Từ khóa không hợp lệ hoặc quá ngắn sau khi chuẩn hóa." : "Không thể tải kết quả tìm kiếm lúc này. Vui lòng thử lại sau."}</p>
            : results?.items.length ? <>
              <p className="mb-4 text-sm text-zinc-400">Tìm thấy {results.totalCount} công thức cho “{q}” <span className="px-1">·</span> Trang {results.page} / {results.totalPages || 1}</p>
              <div className="grid gap-6 sm:grid-cols-2 xl:grid-cols-3">{results.items.map((recipe) => <RecipeCard key={recipe.id} recipe={recipe} />)}</div>
              <nav aria-label="Phân trang kết quả tìm kiếm" className="mt-8 flex items-center justify-between border-t border-white/10 pt-5 text-sm">
                {results.hasPreviousPage ? <Link className="text-emerald-300 hover:underline" href={pageHref(currentParams, page - 1)}>← Trang trước</Link> : <span className="text-zinc-600">← Trang trước</span>}
                <span className="text-zinc-400">{results.page} / {results.totalPages || 1}</span>
                {results.hasNextPage ? <Link className="text-emerald-300 hover:underline" href={pageHref(currentParams, page + 1)}>Trang sau →</Link> : <span className="text-zinc-600">Trang sau →</span>}
              </nav>
            </> : <p className="border-y border-white/10 py-8 text-zinc-400">Không tìm thấy công thức phù hợp với “{q}”. Thử từ khóa khác hoặc bỏ bớt bộ lọc.</p>}
    </main>
  );
}
