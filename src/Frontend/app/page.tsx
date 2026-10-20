import { HomeScreen } from "@/features/search/components/HomeScreen";
import { buildPageMetadata } from "@/features/search/seo/metadata";

export const metadata = buildPageMetadata({
  title: "Công thức nấu ăn",
  description: "Khám phá và chia sẻ những công thức nấu ăn ngon mỗi ngày.",
  path: "/",
});

export const revalidate = 3600;

export default function HomePage() {
  return <HomeScreen />;
}
