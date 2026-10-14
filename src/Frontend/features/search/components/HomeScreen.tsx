import Image from "next/image";
import Link from "next/link";
import { ApiError, apiClient } from "@/lib/api/client";
import type { PagedResult } from "@/lib/api/types";
import { getCategories } from "@/features/categories/api/public-client";
import type { Category } from "@/features/categories/public-types";

type RecipeSummary = {
  id: string;
  title: string;
  slug: string;
  description: string;
  cookTimeMinutes: number;
  servings: number;
  thumbnailUrl: string | null;
};

const cachedPublicRequest = {
  accessToken: null,
  cache: "force-cache" as const,
  next: { revalidate: 3600 },
};

async function getLatestRecipes(): Promise<PagedResult<RecipeSummary>> {
  const query = new URLSearchParams({ page: "1", pageSize: "50", sort: "-publishedAt" });
  return apiClient.get<PagedResult<RecipeSummary>>(
    `/api/v1/recipes?${query}`,
    cachedPublicRequest,
  );
}

function errorMessage(error: unknown, content: string): string {
  if (error instanceof ApiError && error.status === 404) {
    return `${content} chưa sẵn sàng.`;
  }
  return `Không thể tải ${content} lúc này.`;
}

function RecipeCard({ recipe }: { recipe: RecipeSummary }) {
  return (
    <article className="min-w-0">
      <Link
        className="group block focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-emerald-800"
        href={`/recipes/${encodeURIComponent(recipe.slug)}`}
      >
        <div className="relative aspect-[4/3] overflow-hidden bg-emerald-50">
          {recipe.thumbnailUrl ? (
            <Image
              src={recipe.thumbnailUrl}
              alt={`Ảnh món ${recipe.title}`}
              fill
              sizes="(max-width: 767px) 100vw, (max-width: 1199px) 50vw, 33vw"
              unoptimized
              className="object-cover transition-transform duration-300 group-hover:scale-[1.03]"
            />
          ) : (
            <div className="flex h-full min-h-36 flex-col items-center justify-center gap-2 bg-[repeating-linear-gradient(135deg,#edf4ee_0px,#edf4ee_16px,#e6efe8_16px,#e6efe8_17px)] text-emerald-900">
              <span className="grid size-11 place-items-center rounded-full border border-emerald-800/40 font-serif text-sm font-bold" aria-hidden="true">CB</span>
              <span className="text-xs">Chưa có ảnh</span>
            </div>
          )}
        </div>
        <div className="flex items-start justify-between gap-3 border-b border-black/10 py-3">
          <h3 className="font-serif text-lg font-semibold leading-snug group-hover:text-emerald-800">{recipe.title}</h3>
          <span className="shrink-0 text-xs font-medium text-rose-800">{recipe.cookTimeMinutes} phút</span>
        </div>
        <p className="mt-2 line-clamp-2 text-sm leading-6 text-neutral-600">{recipe.description}</p>
      </Link>
    </article>
  );
}

export async function HomeScreen() {
  const [recipeResult, categoryResult] = await Promise.allSettled([
    getLatestRecipes(),
    getCategories(cachedPublicRequest),
  ]);

  const recipes = recipeResult.status === "fulfilled" ? recipeResult.value.items : [];
  const categories: Category[] = categoryResult.status === "fulfilled" ? categoryResult.value : [];
  const featured = recipes.find((recipe) => recipe.thumbnailUrl) ?? recipes[0];
  const latest = recipes.filter((recipe) => recipe.id !== featured?.id).slice(0, 6);

  return (
    <div className="space-y-12">
      <section aria-labelledby="home-title" className="grid gap-7 border-b border-black/10 pb-9 md:grid-cols-[1fr_1.1fr] md:items-center">
        <div>
          <p className="text-xs font-bold uppercase text-rose-800">Công thức từ cộng đồng</p>
          <h1 id="home-title" className="mt-3 max-w-xl font-serif text-4xl font-semibold leading-tight sm:text-5xl">
            Món ngon bắt đầu từ căn bếp của bạn.
          </h1>
          <p className="mt-4 max-w-lg text-base leading-7 text-neutral-600">
            Khám phá công thức gần gũi, dễ làm và được chia sẻ mỗi ngày.
          </p>
          <Link className="mt-6 inline-flex h-11 items-center bg-emerald-800 px-5 text-sm font-semibold text-white hover:bg-emerald-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-800" href="/recipes">
            Xem công thức
          </Link>
        </div>
        {recipeResult.status === "rejected" ? (
          <p className="border-l-4 border-rose-700 bg-rose-50 px-4 py-3 text-sm text-rose-900" role="alert">
            {errorMessage(recipeResult.reason, "Công thức")}
          </p>
        ) : featured ? (
          <article>
            <Link className="group block" href={`/recipes/${encodeURIComponent(featured.slug)}`}>
              <div className="relative aspect-[16/10] overflow-hidden bg-emerald-50">
                {featured.thumbnailUrl ? (
                  <Image
                    src={featured.thumbnailUrl}
                    alt={`Ảnh món ${featured.title}`}
                    fill
                    sizes="(max-width: 767px) 100vw, 55vw"
                    unoptimized
                    className="object-cover"
                  />
                ) : (
                  <div className="flex h-full flex-col items-center justify-center gap-2 bg-[repeating-linear-gradient(135deg,#edf4ee_0px,#edf4ee_16px,#e6efe8_16px,#e6efe8_17px)] text-emerald-900">
                    <span className="grid size-12 place-items-center rounded-full border border-emerald-800/40 font-serif font-bold" aria-hidden="true">CB</span>
                    <span className="text-xs">Chưa có ảnh</span>
                  </div>
                )}
              </div>
              <div className="flex items-end justify-between gap-4 border-b-2 border-rose-700 py-4">
                <div>
                  <p className="text-xs font-bold uppercase text-rose-800">Món nổi bật</p>
                  <h2 className="mt-1 font-serif text-2xl font-semibold group-hover:text-emerald-800">{featured.title}</h2>
                </div>
                <span className="shrink-0 text-sm text-neutral-600">{featured.cookTimeMinutes} phút</span>
              </div>
            </Link>
          </article>
        ) : (
          <div className="flex min-h-56 items-end border-b-2 border-rose-700 bg-emerald-50 p-6">
            <p className="max-w-sm font-serif text-2xl font-medium">Công thức mới sẽ xuất hiện ở đây.</p>
          </div>
        )}
      </section>

      <section aria-labelledby="latest-title">
        <div className="mb-5 flex items-end justify-between gap-4">
          <div>
            <p className="text-xs font-bold uppercase text-emerald-800">Từ cộng đồng</p>
            <h2 id="latest-title" className="mt-1 font-serif text-3xl font-semibold">Mới đăng</h2>
          </div>
          <Link className="text-sm font-semibold text-emerald-800 underline-offset-4 hover:underline" href="/recipes">Xem tất cả</Link>
        </div>
        {recipeResult.status === "rejected" ? (
          <p className="text-sm text-neutral-600" role="status">Danh sách công thức mới chưa thể tải.</p>
        ) : latest.length > 0 ? (
          <div className="grid grid-cols-1 gap-x-6 gap-y-8 md:grid-cols-2 min-[1200px]:grid-cols-3">
            {latest.map((recipe) => <RecipeCard key={recipe.id} recipe={recipe} />)}
          </div>
        ) : (
          <p className="border-y border-black/10 py-6 text-sm text-neutral-600">Chưa có công thức mới.</p>
        )}
      </section>

      <section aria-labelledby="categories-title" className="border-t border-black/10 pt-7">
        <div className="mb-4">
          <p className="text-xs font-bold uppercase text-rose-800">Chọn theo khẩu vị</p>
          <h2 id="categories-title" className="mt-1 font-serif text-3xl font-semibold">Danh mục</h2>
        </div>
        {categoryResult.status === "rejected" ? (
          <p className="border-y border-black/10 py-5 text-sm text-neutral-600" role="status">
            {errorMessage(categoryResult.reason, "Danh mục")}
          </p>
        ) : categories.length === 0 ? (
          <p className="border-y border-black/10 py-5 text-sm text-neutral-600">Chưa có danh mục.</p>
        ) : (
          <ul className="grid grid-cols-1 gap-x-6 md:grid-cols-2 min-[1200px]:grid-cols-4">
            {categories.map((category) => (
              <li key={category.id} className="border-t border-black/10 py-4">
                <Link className="font-serif text-lg font-semibold hover:text-emerald-800" href={`/categories/${encodeURIComponent(category.slug)}`}>
                  {category.name}
                </Link>
                <p className="mt-1 text-xs text-neutral-600">{category.recipeCount} công thức</p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}