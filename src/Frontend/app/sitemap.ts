import type { MetadataRoute } from "next";
import { apiClient } from "@/lib/api/client";
import type { PagedResult } from "@/lib/api/types";
import { getCategories } from "@/features/categories/api/public-client";
import { siteUrl } from "@/features/search/seo/metadata";

export const revalidate = 3600;

type PublishedRecipe = {
  slug: string;
  status: string | number;
  publishedAt?: string | null;
  updatedAt?: string | null;
};

async function getPublishedRecipes(): Promise<PublishedRecipe[]> {
  const pageSize = 50;
  const firstPage = await apiClient.get<PagedResult<PublishedRecipe>>(
    `/api/v1/recipes?page=1&pageSize=${pageSize}&sort=-publishedAt`,
    { accessToken: null, next: { revalidate } },
  );
  const recipes = [...firstPage.items];

  for (let page = 2; page <= firstPage.totalPages; page += 1) {
    const result = await apiClient.get<PagedResult<PublishedRecipe>>(
      `/api/v1/recipes?page=${page}&pageSize=${pageSize}&sort=-publishedAt`,
      { accessToken: null, next: { revalidate } },
    );
    recipes.push(...result.items);
  }

  return recipes.filter((recipe) => recipe.status === 1 || recipe.status === "Published");
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const generatedAt = new Date();
  const [recipes, categories] = await Promise.all([
    getPublishedRecipes(),
    getCategories({ accessToken: null, next: { revalidate } }),
  ]);

  const staticPages: MetadataRoute.Sitemap = [
    { url: siteUrl, lastModified: generatedAt, changeFrequency: "weekly", priority: 1 },
    { url: new URL("/recipes", siteUrl).toString(), lastModified: generatedAt, changeFrequency: "daily", priority: 0.9 },
    { url: new URL("/search", siteUrl).toString(), lastModified: generatedAt, changeFrequency: "weekly", priority: 0.7 },
    { url: new URL("/categories", siteUrl).toString(), lastModified: generatedAt, changeFrequency: "weekly", priority: 0.8 },
  ];

  const categoryPages: MetadataRoute.Sitemap = categories.map((category) => ({
    url: new URL(`/categories/${encodeURIComponent(category.slug)}`, siteUrl).toString(),
    lastModified: generatedAt,
    changeFrequency: "weekly",
    priority: 0.7,
  }));

  const recipePages: MetadataRoute.Sitemap = recipes.map((recipe) => ({
    url: new URL(`/recipes/${encodeURIComponent(recipe.slug)}`, siteUrl).toString(),
    lastModified: recipe.updatedAt ?? recipe.publishedAt ?? generatedAt,
    changeFrequency: "monthly",
    priority: 0.8,
  }));

  return [...staticPages, ...categoryPages, ...recipePages];
}
