import { HealthStatus } from "@/components/HealthStatus";
import { PlaceholderPage } from "@/components/PlaceholderPage";

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
