import type { Metadata } from "next";
import { RecipeEditor } from "@/features/recipes/components/RecipeEditor";

export const metadata: Metadata = {
  title: "Chỉnh sửa công thức",
  description: "Chỉnh sửa công thức nấu ăn của bạn",
};

interface EditRecipePageProps {
  params: Promise<{
    id: string;
  }>;
}

export default async function EditRecipePage({ params }: EditRecipePageProps) {
  const { id } = await params;

  return <RecipeEditor recipeId={id} />;
}
