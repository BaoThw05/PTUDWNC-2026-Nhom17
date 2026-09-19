import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Chi tiết công thức" };

export default async function RecipeDetailPage({ params }: PageProps<"/recipes/[slug]">) {
  const { slug } = await params;

  return (
    <PlaceholderPage
      title="Chi tiết công thức"
      owner="TV2"
      requirements="FR-RCP-002"
    >
      <p className="text-zinc-600 dark:text-zinc-400">
        Tham số <code>slug</code>: {slug}
      </p>
    </PlaceholderPage>
  );
}
