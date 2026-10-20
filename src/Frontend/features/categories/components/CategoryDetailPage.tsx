import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { getCategoryBySlug } from "../api/public-client";
import { ApiError } from "@/lib/api/client";

export async function loadCategory(slug: string, page: number) {
  try {
    return await getCategoryBySlug(slug, page, {
      accessToken: null,
      next: { revalidate: 300 },
    });
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }
}

export async function CategoryDetailPage({ slug, page }: { slug: string; page: number }) {
  const { category, recipes } = await loadCategory(slug, page);
  const categoryPath = `/categories/${encodeURIComponent(category.slug)}`;
  if (page > Math.max(1, recipes.totalPages)) {
    redirect(`${categoryPath}/page/${Math.max(1, recipes.totalPages)}`);
  }

  const pagePath = (value: number) => value === 1 ? categoryPath : `${categoryPath}/page/${value}`;

  return (
    <section className="space-y-8">
      <nav aria-label="Đường dẫn" className="text-sm text-zinc-500 dark:text-zinc-400">
        <Link href="/categories" className="hover:underline">Danh mục</Link>
        <span aria-hidden="true" className="mx-2">/</span>
        <span aria-current="page">{category.name}</span>
      </nav>

      <header className="space-y-3">
        <h1 className="text-3xl font-bold tracking-tight">{category.name}</h1>
        {category.description && <p className="max-w-2xl text-zinc-600 dark:text-zinc-400">{category.description}</p>}
        <p className="text-sm text-zinc-500 dark:text-zinc-400">{recipes.totalCount} công thức đã xuất bản</p>
      </header>

      {recipes.items.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-zinc-300 px-6 py-12 text-center text-zinc-600 dark:border-zinc-700 dark:text-zinc-400">
          {page > 1 ? "Trang này không có công thức." : "Danh mục này chưa có công thức được xuất bản."}
        </div>
      ) : (
        <div className="grid gap-5 sm:grid-cols-2">
          {recipes.items.map((recipe) => (
            <Link
              key={recipe.id}
              href={`/recipes/${encodeURIComponent(recipe.slug)}`}
              className="group rounded-2xl border border-zinc-200 bg-white p-6 transition hover:border-orange-400 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-500 dark:border-zinc-800 dark:bg-zinc-900"
            >
              <h2 className="text-lg font-semibold group-hover:text-orange-700 dark:group-hover:text-orange-300">
                {recipe.title}
              </h2>
              {recipe.description && (
                <p className="mt-3 line-clamp-3 text-sm leading-6 text-zinc-600 dark:text-zinc-400">
                  {recipe.description}
                </p>
              )}
              <p className="mt-5 text-sm text-zinc-500 dark:text-zinc-400">
                {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút · {recipe.servings} khẩu phần
              </p>
            </Link>
          ))}
        </div>
      )}

      {recipes.totalPages > 1 && (
        <nav aria-label="Phân trang công thức" className="flex items-center justify-center gap-4">
          {recipes.hasPreviousPage ? (
            <Link href={pagePath(page - 1)} className="rounded-lg border px-4 py-2 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800">
              Trang trước
            </Link>
          ) : <span className="w-25" />}
          <span className="text-sm text-zinc-600 dark:text-zinc-400">Trang {page} / {recipes.totalPages}</span>
          {recipes.hasNextPage ? (
            <Link href={pagePath(page + 1)} className="rounded-lg border px-4 py-2 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800">
              Trang sau
            </Link>
          ) : <span className="w-25" />}
        </nav>
      )}
    </section>
  );
}
