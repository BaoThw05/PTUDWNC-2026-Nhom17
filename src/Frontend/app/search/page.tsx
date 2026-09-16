import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Tìm kiếm" };

export default function SearchPage() {
  return (
    <PlaceholderPage
      title="Tìm kiếm"
      owner="TV4"
      requirements="FR-SRCH-001"
    />
  );
}
