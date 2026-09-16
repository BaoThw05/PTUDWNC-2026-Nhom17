import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Tạo công thức" };

export default function NewRecipePage() {
  return (
    <PlaceholderPage
      title="Tạo công thức"
      owner="TV2"
      requirements="FR-RCP-003, 009, 010"
    />
  );
}
