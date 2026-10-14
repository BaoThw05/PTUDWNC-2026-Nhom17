import type { Metadata } from "next";
import { HomeScreen } from "@/features/search/components/HomeScreen";

export const metadata: Metadata = {
  title: "Trang chủ",
  description: "Khám phá công thức nấu ăn mới nhất từ cộng đồng Culinary Blog.",
};

export const revalidate = 3600;

export default function HomePage() {
  return <HomeScreen />;
}