import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Công thức" };

export default function RecipesPage() {
  return (
    <PlaceholderPage
      title="Công thức"
      owner="TV4"
      requirements="FR-RCP-001, FR-SRCH-002 → 004"
    />
  );
}
