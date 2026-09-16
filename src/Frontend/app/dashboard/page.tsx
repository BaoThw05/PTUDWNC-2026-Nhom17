import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Bảng điều khiển" };

export default function DashboardPage() {
  return (
    <PlaceholderPage
      title="Bảng điều khiển"
      owner="TV2"
      requirements="Tổng quan công thức của tác giả"
    />
  );
}
