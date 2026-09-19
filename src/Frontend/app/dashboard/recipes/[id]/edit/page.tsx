import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Sửa công thức" };

export default async function EditRecipePage({ params }: PageProps<"/dashboard/recipes/[id]/edit">) {
  const { id } = await params;

  return (
    <PlaceholderPage
      title="Sửa công thức"
      owner="TV2"
      requirements="FR-RCP-004, 009, 010"
    >
      <p className="text-zinc-600 dark:text-zinc-400">
        Tham số <code>id</code>: {id}
      </p>
    </PlaceholderPage>
  );
}
