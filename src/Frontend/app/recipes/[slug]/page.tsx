import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getRecipeBySlug } from "@/features/recipes/api/client";
import { RecipeDetailView } from "@/features/recipes/components/RecipeDetailView";
import { RecipeJsonLd, buildPageMetadata } from "@/features/search/seo/metadata";
import { ApiError } from "@/lib/api/client";

export const revalidate = 300;

interface RecipeDetailPageProps {
  params: Promise<{ slug: string }>;
}

function isPublished(status: string | number) {
  return status === "Published" || status === 1;
}

export async function generateMetadata({ params }: RecipeDetailPageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const recipe = await getRecipeBySlug(slug);
    if (!isPublished(recipe.status)) {
      return { title: "Không tìm thấy công thức", robots: { index: false, follow: false } };
    }
    return buildPageMetadata({
      title: recipe.title,
      description: recipe.description || `Xem cách làm món ${recipe.title}`,
      path: `/recipes/${encodeURIComponent(recipe.slug)}`,
    });
  } catch {
    return { title: "Không tìm thấy công thức", robots: { index: false, follow: false } };
  }
}

export default async function RecipeDetailPage({ params }: RecipeDetailPageProps) {
  const { slug } = await params;
  let recipe;
  try {
    recipe = await getRecipeBySlug(slug);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) notFound();
    throw error;
  }

  if (!isPublished(recipe.status)) notFound();

  return (
    <>
      <RecipeJsonLd recipe={{
        name: recipe.title,
        description: recipe.description,
        author: { name: "Culinary Blog" },
        datePublished: recipe.publishedAt,
        prepTimeMinutes: recipe.prepTimeMinutes,
        cookTimeMinutes: recipe.cookTimeMinutes,
        totalTimeMinutes: recipe.prepTimeMinutes + recipe.cookTimeMinutes,
        recipeYield: `${recipe.servings} khẩu phần`,
        recipeIngredient: recipe.ingredients.map((item) =>
          [item.quantity, item.unit, item.name].filter(Boolean).join(" "),
        ),
        recipeInstructions: recipe.steps.map((step) => step.description),
        nutrition: recipe.nutrition?.calories == null
          ? undefined
          : { calories: `${recipe.nutrition.calories} calories` },
      }} />
      <RecipeDetailView recipe={recipe} />
    </>
  );
}
