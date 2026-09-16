import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Chi tiết danh mục" };

export default async function CategoryDetailPage({ params }: PageProps<"/categories/[slug]">) {
  const { slug } = await params;

  return (
    <PlaceholderPage
      title="Chi tiết danh mục"
      owner="TV3"
      requirements="FR-CAT-002"
    >
      <p className="text-zinc-600 dark:text-zinc-400">
        Tham số <code>slug</code>: {slug}
      </p>
    </PlaceholderPage>
  );
}
