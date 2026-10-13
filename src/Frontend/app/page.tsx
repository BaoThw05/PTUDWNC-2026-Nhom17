import { HomeScreen } from "@/features/search/components/HomeScreen";
import { buildPageMetadata } from "@/features/search/seo/metadata";

export const metadata = buildPageMetadata({
  title: "Trang chủ",
  description: "Khám phá công thức nấu ăn mới nhất từ cộng đồng Culinary Blog.",
  path: "/",
});

export const revalidate = 3600;

export default function HomePage() {
  return <HomeScreen />;
}
