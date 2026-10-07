import type { Metadata } from "next";
import Link from "next/link";
import { connection } from "next/server";
import { getCategories } from "@/features/categories/api/public-client";

export const metadata: Metadata = {
  title: "Danh mục món ăn",
  description: "Khám phá công thức nấu ăn theo từng danh mục.",
};

export const revalidate = 3600;

export default async function CategoriesPage() {
  // Backend không cần chạy trong lúc build; dữ liệu vẫn được cache và làm mới theo revalidate.
  await connection();
  const categories = await getCategories({ accessToken: null, next: { revalidate } });

  return (
    <section className="space-y-8">
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tight">Danh mục món ăn</h1>
        <p className="text-zinc-600 dark:text-zinc-400">
          Chọn một danh mục để khám phá các công thức đã được chia sẻ.
        </p>
      </div>

      {categories.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-zinc-300 px-6 py-12 text-center text-zinc-600 dark:border-zinc-700 dark:text-zinc-400">
          Chưa có danh mục nào. Hãy quay lại sau nhé.
        </div>
      ) : (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {categories.map((category) => (
            <Link
              key={category.id}
              href={`/categories/${encodeURIComponent(category.slug)}`}
              className="group flex min-h-48 flex-col justify-between rounded-2xl border border-zinc-200 bg-white p-6 transition hover:border-orange-400 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-500 dark:border-zinc-800 dark:bg-zinc-900"
            >
              <div className="space-y-3">
                <h2 className="text-xl font-semibold group-hover:text-orange-700 dark:group-hover:text-orange-300">
                  {category.name}
                </h2>
                {category.description && (
                  <p className="line-clamp-3 text-sm leading-6 text-zinc-600 dark:text-zinc-400">
                    {category.description}
                  </p>
                )}
              </div>
              <span className="mt-6 text-sm font-medium text-zinc-500 dark:text-zinc-400">
                {category.recipeCount} công thức
              </span>
            </Link>
          ))}
        </div>
      )}
    </section>
  );
}
