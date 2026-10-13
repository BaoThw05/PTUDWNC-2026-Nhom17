import { HealthStatus } from "@/components/HealthStatus";
import { PlaceholderPage } from "@/components/PlaceholderPage";
import { buildPageMetadata } from "@/features/search/seo/metadata";

export const metadata = buildPageMetadata({
  title: "Công thức nấu ăn",
  description: "Khám phá và chia sẻ những công thức nấu ăn ngon mỗi ngày.",
  path: "/",
});

export default function HomePage() {
  return (
    <PlaceholderPage
      title="Trang chủ"
      owner="TV4"
      requirements="FR-RCP-001 (công thức mới nhất, danh mục nổi bật)"
    >
      <HealthStatus />
    </PlaceholderPage>
  );
}
