import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getRecipeBySlug } from "@/features/recipes/api/client";
import { getRecipeImages } from "@/features/images/api/client";
import { RecipeDetailView } from "@/features/recipes/components/RecipeDetailView";
import { RecipeJsonLd, buildPageMetadata, siteUrl } from "@/features/search/seo/metadata";
import { SocialSharePanel } from "@/features/search/components/SocialSharePanel";
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
    const images = await getRecipeImages(recipe.id).catch(() => []);
    const primaryImage = images.find((image) => image.isPrimary) ?? images[0];
    return buildPageMetadata({
      title: recipe.title,
      description: recipe.description || `Xem cách làm món ${recipe.title}`,
      path: `/recipes/${encodeURIComponent(recipe.slug)}`,
      image: primaryImage?.originalUrl ?? `/recipes/${encodeURIComponent(recipe.slug)}/opengraph-image`,
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

  const images = await getRecipeImages(recipe.id).catch(() => []);
  const primaryImage = images.find((image) => image.isPrimary) ?? images[0];
  const recipeImage = primaryImage?.originalUrl
    ? new URL(primaryImage.originalUrl, siteUrl).href
    : undefined;

  return (
    <>
      {recipeImage && <link rel="preload" as="image" href={recipeImage} fetchPriority="high" />}
      <RecipeJsonLd recipe={{
        name: recipe.title,
        description: recipe.description,
        image: recipeImage,
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
      <RecipeDetailView recipe={recipe} images={images} />
      <SocialSharePanel
        path={`/recipes/${encodeURIComponent(recipe.slug)}`}
        title={recipe.title}
      />
    </>
  );
}
