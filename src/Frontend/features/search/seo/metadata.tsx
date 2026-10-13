import type { Metadata } from "next";

const siteName = "Culinary Blog";
const siteUrl = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

type PageMetadata = {
  title: string;
  description: string;
  path: string;
  image?: string;
};

export function buildPageMetadata({
  title,
  description,
  path,
  image,
}: PageMetadata): Metadata {
  const normalizedTitle = title.trim().slice(0, 60);
  const normalizedDescription = description.trim().slice(0, 160);
  const canonicalPath = path.startsWith("/") ? path : `/${path}`;
  const socialImage = image ? [image] : undefined;

  return {
    metadataBase: new URL(siteUrl),
    title: normalizedTitle,
    description: normalizedDescription,
    alternates: { canonical: canonicalPath },
    openGraph: {
      type: "website",
      siteName,
      title: normalizedTitle,
      description: normalizedDescription,
      url: canonicalPath,
      images: socialImage,
    },
    twitter: {
      card: image ? "summary_large_image" : "summary",
      title: normalizedTitle,
      description: normalizedDescription,
      images: socialImage,
    },
  };
}

export type RecipeStructuredData = {
  name: string;
  description?: string | null;
  image?: string | string[] | null;
  author?: string | { name: string } | null;
  datePublished?: string | null;
  prepTimeMinutes?: number | null;
  cookTimeMinutes?: number | null;
  totalTimeMinutes?: number | null;
  recipeYield?: string | null;
  recipeIngredient?: string[];
  recipeInstructions?: string[];
  nutrition?: {
    calories?: string | null;
  } | null;
};

function toIsoDuration(minutes: number | null | undefined): string | undefined {
  if (minutes === null || minutes === undefined || !Number.isFinite(minutes) || minutes < 0) {
    return undefined;
  }

  return `PT${Math.floor(minutes)}M`;
}

export function toRecipeJsonLd(recipe: RecipeStructuredData) {
  return {
    "@context": "https://schema.org",
    "@type": "Recipe",
    name: recipe.name,
    description: recipe.description || undefined,
    image: recipe.image || undefined,
    author: recipe.author
      ? typeof recipe.author === "string"
        ? { "@type": "Person", name: recipe.author }
        : { "@type": "Person", ...recipe.author }
      : undefined,
    datePublished: recipe.datePublished || undefined,
    prepTime: toIsoDuration(recipe.prepTimeMinutes),
    cookTime: toIsoDuration(recipe.cookTimeMinutes),
    totalTime: toIsoDuration(recipe.totalTimeMinutes),
    recipeYield: recipe.recipeYield || undefined,
    recipeIngredient: recipe.recipeIngredient?.length
      ? recipe.recipeIngredient
      : undefined,
    recipeInstructions: recipe.recipeInstructions?.length
      ? recipe.recipeInstructions.map((text) => ({ "@type": "HowToStep", text }))
      : undefined,
    nutrition: recipe.nutrition?.calories
      ? { "@type": "NutritionInformation", calories: recipe.nutrition.calories }
      : undefined,
  };
}

export function RecipeJsonLd({ recipe }: { recipe: RecipeStructuredData }) {
  const json = JSON.stringify(toRecipeJsonLd(recipe)).replace(/</g, "\\u003c");
  return <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: json }} />;
}
