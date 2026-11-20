import { HomeScreen } from "@/features/search/components/HomeScreen";
import { buildPageMetadata } from "@/features/search/seo/metadata";

export const metadata = buildPageMetadata({
  title: "Công thức nấu ăn",
  description: "Khám phá và chia sẻ những công thức nấu ăn ngon mỗi ngày.",
  path: "/",
});

export const dynamic = "force-dynamic";

export default function HomePage() {
  return <HomeScreen />;
}
