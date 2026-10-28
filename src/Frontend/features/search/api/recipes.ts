import { apiClient } from "@/lib/api/client";
import type { PagedResult } from "@/lib/api/types";

export type RecipeSummary = {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: "Easy" | "Medium" | "Hard" | number;
  status: "Draft" | "Published" | "Archived" | number;
  publishedAt: string | null;
  categoryId: string | null;
  thumbnailUrl: string | null;
  relevanceScore?: number | null;
};

export type RecipeFilters = {
  categoryId?: string;
  difficulty?: string;
  maxCookTime?: string;
  minServings?: string;
  sort?: string;
  page?: string;
  pageSize?: string;
};

export type RecipeCategory = {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  recipeCount: number;
};

type RequestOptions = {
  accessToken?: string | null;
  cache?: RequestCache;
  revalidate?: number;
};

function toQueryString(filters: Record<string, string | undefined>): string {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(filters)) {
    if (value) {
      const apiValue =
        key === "sort" ? value.replace(/(^-?)cookTime$/, "$1cookTimeMinutes") : value;
      params.set(key, apiValue);
    }
  }

  return params.toString();
}

function requestOptions({ accessToken, cache, revalidate }: RequestOptions) {
  return {
    accessToken,
    ...(cache ? { cache } : {}),
    ...(revalidate ? { next: { revalidate } } : {}),
  };
}

export function getRecipes(
  filters: RecipeFilters,
  options: RequestOptions = {},
): Promise<PagedResult<RecipeSummary>> {
  const query = toQueryString(filters);
  const path = `/api/v1/recipes${query ? `?${query}` : ""}`;
  return apiClient.get<PagedResult<RecipeSummary>>(path, requestOptions(options));
}

export function searchRecipes(
  q: string,
  filters: Omit<RecipeFilters, "sort">,
  options: RequestOptions = {},
): Promise<PagedResult<RecipeSummary>> {
  const query = toQueryString({ q, ...filters });
  return apiClient.get<PagedResult<RecipeSummary>>(
    `/api/v1/recipes/search?${query}`,
    requestOptions(options),
  );
}

export function getCategories(
  options: RequestOptions = {},
): Promise<RecipeCategory[]> {
  return apiClient.get<RecipeCategory[]>(
    "/api/v1/categories",
    requestOptions(options),
  );
}