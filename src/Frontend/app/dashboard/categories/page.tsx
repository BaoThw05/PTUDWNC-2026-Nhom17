import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Quản lý danh mục" };

export default function ManageCategoriesPage() {
  return (
    <PlaceholderPage
      title="Quản lý danh mục"
      owner="TV3"
      requirements="FR-CAT-003 → 005"
    />
  );
}
