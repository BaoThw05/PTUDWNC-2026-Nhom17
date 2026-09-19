import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Danh mục" };

export default function CategoriesPage() {
  return (
    <PlaceholderPage
      title="Danh mục"
      owner="TV3"
      requirements="FR-CAT-001"
    />
  );
}
