import { apiClient } from "@/lib/api/client";
import type {
  CreateRecipeIngredientDto,
  CreateRecipeRequest,
  CreateRecipeStepDto,
  PagedResult,
  RecipeFilterTab,
  RecipeSummary,
  UpdateRecipeRequest,
} from "../types";

const RECIPES_PATH = "/api/v1/recipes";

export async function getMyRecipes(params: {
  status?: RecipeFilterTab;
  page?: number;
  pageSize?: number;
}): Promise<PagedResult<RecipeSummary>> {
  const query = new URLSearchParams();
  if (params.status && params.status !== "all") {
    query.set("status", params.status);
  }
  if (params.page) {
    query.set("page", params.page.toString());
  }
  if (params.pageSize) {
    query.set("pageSize", params.pageSize.toString());
  }

  const queryString = query.toString();
  const path = `/api/v1/me/recipes${queryString ? `?${queryString}` : ""}`;
  return apiClient.get<PagedResult<RecipeSummary>>(path);
}

export async function getRecipeById(id: string): Promise<RecipeSummary> {
  return apiClient.get<RecipeSummary>(`${RECIPES_PATH}/${id}`);
}

export async function getRecipeBySlug(slug: string): Promise<RecipeSummary> {
  return apiClient.get<RecipeSummary>(`${RECIPES_PATH}/${encodeURIComponent(slug)}`);
}

export async function createRecipe(data: CreateRecipeRequest): Promise<{ id: string }> {
  return apiClient.post<{ id: string }>(`${RECIPES_PATH}`, { body: data as unknown as Record<string, unknown> });
}

export async function updateRecipe(id: string, data: UpdateRecipeRequest): Promise<void> {
  return apiClient.put<void>(`${RECIPES_PATH}/${id}`, { body: data as unknown as Record<string, unknown> });
}

export async function addRecipeStep(recipeId: string, data: CreateRecipeStepDto): Promise<{ id: string }> {
  return apiClient.post<{ id: string }>(`${RECIPES_PATH}/${recipeId}/steps`, {
    body: data as unknown as Record<string, unknown>,
  });
}

export async function deleteRecipeStep(recipeId: string, stepId: string): Promise<void> {
  return apiClient.delete<void>(`${RECIPES_PATH}/${recipeId}/steps/${stepId}`);
}

export async function addRecipeIngredient(
  recipeId: string,
  data: CreateRecipeIngredientDto,
): Promise<{ id: string }> {
  return apiClient.post<{ id: string }>(`${RECIPES_PATH}/${recipeId}/ingredients`, {
    body: data as unknown as Record<string, unknown>,
  });
}

export async function deleteRecipeIngredient(recipeId: string, ingredientId: string): Promise<void> {
  return apiClient.delete<void>(`${RECIPES_PATH}/${recipeId}/ingredients/${ingredientId}`);
}

export async function publishRecipe(id: string): Promise<void> {
  return apiClient.post<void>(`${RECIPES_PATH}/${id}/publish`);
}

export async function unpublishRecipe(id: string): Promise<void> {
  return apiClient.post<void>(`${RECIPES_PATH}/${id}/unpublish`);
}

export async function archiveRecipe(id: string): Promise<void> {
  return apiClient.post<void>(`${RECIPES_PATH}/${id}/archive`);
}

export async function unarchiveRecipe(id: string): Promise<void> {
  return apiClient.post<void>(`${RECIPES_PATH}/${id}/unarchive`);
}

export async function deleteRecipe(id: string): Promise<void> {
  return apiClient.delete<void>(`${RECIPES_PATH}/${id}`);
}

export async function restoreRecipe(id: string): Promise<void> {
  return apiClient.post<void>(`${RECIPES_PATH}/${id}/restore`);
}
