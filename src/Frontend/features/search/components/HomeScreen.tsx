import Image from "next/image";
import Link from "next/link";
import { ApiError } from "@/lib/api/client";
import { getCategories, getRecipes } from "@/features/search/api/recipes";
import type { RecipeCategory, RecipeSummary } from "@/features/search/api/recipes";
import { RecipeGrid } from "./RecipeGrid";
import styles from "./SearchSurfaces.module.css";

function errorLabel(error: unknown): string {
  if (error instanceof ApiError && error.status === 404) {
    return "Dữ liệu danh mục chưa sẵn sàng.";
  }
  return "Không thể tải danh mục lúc này.";
}

export async function HomeScreen() {
  const options = { accessToken: null, cache: "force-cache" as const, revalidate: 3600 };
  const [recipeResult, categoryResult] = await Promise.allSettled([
    getRecipes({ page: "1", pageSize: "50", sort: "-publishedAt" }, options),
    getCategories(options),
  ]);

  const recipes: RecipeSummary[] = recipeResult.status === "fulfilled" ? recipeResult.value.items : [];
  const categories: RecipeCategory[] = categoryResult.status === "fulfilled" ? categoryResult.value : [];
  const featured = recipes.find((recipe) => recipe.thumbnailUrl) ?? recipes[0];
  const recent = recipes.filter((recipe) => recipe.id !== featured?.id).slice(0, 6);

  return (
    <div className={`${styles.screen} space-y-14`}>
      <section aria-labelledby="home-title" className="grid gap-8 border-b border-black/10 pb-10 md:grid-cols-[1fr_1.05fr] md:items-center">
        <div>
          <p className="mb-3 text-xs font-bold uppercase text-[var(--color-tomato)]">Công thức nấu ăn</p>
          <h1 id="home-title" className="max-w-xl font-serif text-4xl font-semibold leading-tight sm:text-5xl">
            Món ngon bắt đầu từ căn bếp của bạn.
          </h1>
          <p className="mt-4 max-w-lg text-base leading-7 text-[var(--muted)]">
            Khám phá những công thức gần gũi, dễ làm và được chia sẻ từ cộng đồng.
          </p>
          <Link className="mt-6 inline-flex h-11 items-center bg-[var(--color-tomato)] px-5 text-sm font-semibold text-white hover:bg-[var(--color-tomato-dark)]" href="/recipes">
            Xem tất cả công thức
          </Link>
        </div>
        {recipeResult.status === "rejected" ? (
          <p className="border-l-4 border-[var(--color-tomato)] bg-[var(--surface-red)] px-4 py-3 text-sm" role="alert">
            Không thể tải công thức lúc này.
          </p>
        ) : featured ? (
          <article>
            <Link href={`/recipes/${encodeURIComponent(featured.slug)}`} className="group block">
              <div className={styles.featureMedia}>
                {featured.thumbnailUrl ? (
                  <Image src={featured.thumbnailUrl} alt={`Ảnh món ${featured.title}`} fill sizes="(max-width: 767px) 100vw, 50vw" unoptimized className="object-cover" />
                ) : (
                  <div className={styles.noImage} aria-label="Công thức chưa có ảnh">
                    <span className={styles.noImageMark} aria-hidden="true">CB</span>
                    <small>Chưa có ảnh</small>
                  </div>
                )}
              </div>
              <div className="flex items-end justify-between gap-4 border-b-2 border-[var(--color-tomato)] py-4">
                <div>
                  <p className="text-xs font-bold uppercase text-[var(--color-tomato)]">Món nổi bật</p>
                  <h2 className="mt-1 font-serif text-2xl font-semibold group-hover:text-[var(--color-leaf)]">{featured.title}</h2>
                </div>
                <span className="shrink-0 text-sm text-[var(--muted)]">{featured.cookTimeMinutes} phút</span>
              </div>
            </Link>
          </article>
        ) : (
          <div className="flex min-h-64 items-end border-b-2 border-[var(--color-tomato)] bg-[var(--surface-green)] p-7">
            <p className="max-w-sm font-serif text-2xl font-medium">Công thức mới sẽ xuất hiện ở đây.</p>
          </div>
        )}
      </section>

      <section aria-labelledby="recent-title">
        <div className="mb-6 flex items-end justify-between gap-4">
          <div>
            <p className="text-xs font-bold uppercase text-[var(--color-leaf)]">Từ cộng đồng</p>
            <h2 id="recent-title" className="mt-1 font-serif text-3xl font-semibold">Mới đăng</h2>
          </div>
          <Link className="text-sm font-semibold text-[var(--color-leaf)] underline-offset-4 hover:underline" href="/recipes">Xem tất cả</Link>
        </div>
        {recipeResult.status === "fulfilled" ? (
          <RecipeGrid recipes={recent} />
        ) : (
          <p className="text-sm text-[var(--muted)]">Danh sách mới đăng chưa thể tải.</p>
        )}
      </section>

      <section aria-labelledby="categories-title" className="border-t border-black/10 pt-8">
        <div className="mb-5">
          <p className="text-xs font-bold uppercase text-[var(--color-tomato)]">Chọn theo khẩu vị</p>
          <h2 id="categories-title" className="mt-1 font-serif text-3xl font-semibold">Danh mục</h2>
        </div>
        {categoryResult.status === "rejected" ? (
          <p className="border-y border-black/10 py-5 text-sm text-[var(--muted)]" role="status">{errorLabel(categoryResult.reason)}</p>
        ) : categories.length === 0 ? (
          <p className="border-y border-black/10 py-5 text-sm text-[var(--muted)]">Chưa có danh mục.</p>
        ) : (
          <ul className={styles.categoryGrid}>
            {categories.map((category) => (
              <li key={category.id} className="border-t border-black/10 py-4">
                <Link className="font-serif text-lg font-semibold hover:text-[var(--color-leaf)]" href={`/categories/${encodeURIComponent(category.slug)}`}>
                  {category.name}
                </Link>
                <p className="mt-1 text-xs text-[var(--muted)]">{category.recipeCount} công thức</p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}