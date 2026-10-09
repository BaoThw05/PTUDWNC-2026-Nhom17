import type { Metadata } from "next";
import { RecipeWizard } from "@/features/recipes/components/RecipeWizard";

export const metadata: Metadata = {
  title: "Tạo công thức mới",
  description: "Tạo công thức nấu ăn mới qua từng bước",
};

export default function NewRecipePage() {
  return <RecipeWizard />;
}
