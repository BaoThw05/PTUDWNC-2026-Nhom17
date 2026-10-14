import { apiClient } from "@/lib/api/client";
import type { RecipeImage, RecipeImagePatch } from "../types";

function imagesPath(recipeId: string): string {
  return `/api/v1/recipes/${encodeURIComponent(recipeId)}/images`;
}

export function getRecipeImages(recipeId: string): Promise<RecipeImage[]> {
  return apiClient.get<RecipeImage[]>(imagesPath(recipeId));
}

export function uploadRecipeImage(
  recipeId: string,
  file: File,
  onProgress: (percent: number) => void,
  signal?: AbortSignal,
): Promise<RecipeImage> {
  const body = new FormData();
  body.append("file", file);
  return apiClient.upload<RecipeImage>(imagesPath(recipeId), { body, onProgress, signal });
}

export function updateRecipeImage(
  recipeId: string,
  imageId: string,
  patch: RecipeImagePatch,
): Promise<RecipeImage> {
  return apiClient.patch<RecipeImage>(`${imagesPath(recipeId)}/${encodeURIComponent(imageId)}`, {
    body: patch,
  });
}

export function deleteRecipeImage(recipeId: string, imageId: string): Promise<void> {
  return apiClient.delete<void>(`${imagesPath(recipeId)}/${encodeURIComponent(imageId)}`);
}
