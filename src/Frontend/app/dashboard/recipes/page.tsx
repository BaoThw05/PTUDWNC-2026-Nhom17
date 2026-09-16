import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Công thức của tôi" };

export default function MyRecipesPage() {
  return (
    <PlaceholderPage
      title="Công thức của tôi"
      owner="TV2"
      requirements="FR-RCP-005 → 007, thùng rác"
    />
  );
}
