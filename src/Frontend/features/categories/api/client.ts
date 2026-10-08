import { apiClient } from "@/lib/api/client";
import type { Category, CategoryInput } from "../types";

const path = "/api/v1/categories";

export function getCategories(): Promise<Category[]> {
  return apiClient.get<Category[]>(path);
}

export function createCategory(input: CategoryInput): Promise<Category> {
  return apiClient.post<Category>(path, { body: input });
}

export function updateCategory(id: string, input: CategoryInput): Promise<Category> {
  return apiClient.put<Category>(`${path}/${encodeURIComponent(id)}`, { body: input });
}

export function deleteCategory(id: string): Promise<void> {
  return apiClient.delete<void>(`${path}/${encodeURIComponent(id)}`);
}
