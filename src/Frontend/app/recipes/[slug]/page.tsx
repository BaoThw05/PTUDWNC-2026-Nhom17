import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getRecipeBySlug } from "@/features/recipes/api/client";
import { RecipeDetailView } from "@/features/recipes/components/RecipeDetailView";
import { ApiError } from "@/lib/api/client";

export const revalidate = 300; // ISR revalidate mỗi 5 phút (300 giây) theo kế hoạch 2.17

interface RecipeDetailPageProps {
  params: Promise<{
    slug: string;
  }>;
}

export async function generateMetadata({
  params,
}: RecipeDetailPageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const recipe = await getRecipeBySlug(slug);
    return {
      title: `${recipe.title} - Culinary Blog`,
      description: recipe.description || `Xem chi tiết cách làm món ${recipe.title}`,
    };
  } catch {
    return {
      title: "Không tìm thấy công thức",
    };
  }
}

export default async function RecipeDetailPage({ params }: RecipeDetailPageProps) {
  const { slug } = await params;

  let recipe;
  try {
    recipe = await getRecipeBySlug(slug);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      notFound();
    }
    // Lỗi khác hoặc mạng không kết nối
    throw error;
  }

  const isPublished =
    recipe &&
    (recipe.status === "Published" ||
      (recipe.status as unknown as number) === 1);

  if (!isPublished) {
    notFound();
  }

  return (
    <>
      {/* Vị trí chừa cho TV4 nhúng schema JSON-LD Recipe (SEO & Rich Results) */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify({
            "@context": "https://schema.org",
            "@type": "Recipe",
            name: recipe.title,
            description: recipe.description,
            // TV4 sẽ bổ sung chi tiết author, nutrition, step schema tại đây
          }),
        }}
      />
      <RecipeDetailView recipe={recipe} />
    </>
  );
}
