import type { Metadata } from "next";
import { MyRecipesTable } from "@/features/recipes/components/MyRecipesTable";

export const metadata: Metadata = {
  title: "Công thức của tôi",
  description: "Quản lý công thức nấu ăn cá nhân",
};

export default function MyRecipesPage() {
  return <MyRecipesTable />;
}
