"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  archiveRecipe,
  deleteRecipe,
  getMyRecipes,
  publishRecipe,
  restoreRecipe,
  unarchiveRecipe,
  unpublishRecipe,
} from "../api/client";
import type { RecipeFilterTab } from "../types";

export const RECIPES_QUERY_KEY = ["recipes", "my-recipes"] as const;

export function useMyRecipes(status: RecipeFilterTab = "all", page = 1, pageSize = 10) {
  return useQuery({
    queryKey: [...RECIPES_QUERY_KEY, { status, page, pageSize }],
    queryFn: () => getMyRecipes({ status, page, pageSize }),
  });
}

export function useRecipeActions() {
  const queryClient = useQueryClient();

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: RECIPES_QUERY_KEY });
  };

  const publishMutation = useMutation({
    mutationFn: (id: string) => publishRecipe(id),
    onSuccess: invalidate,
  });

  const unpublishMutation = useMutation({
    mutationFn: (id: string) => unpublishRecipe(id),
    onSuccess: invalidate,
  });

  const archiveMutation = useMutation({
    mutationFn: (id: string) => archiveRecipe(id),
    onSuccess: invalidate,
  });

  const unarchiveMutation = useMutation({
    mutationFn: (id: string) => unarchiveRecipe(id),
    onSuccess: invalidate,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteRecipe(id),
    onSuccess: invalidate,
  });

  const restoreMutation = useMutation({
    mutationFn: (id: string) => restoreRecipe(id),
    onSuccess: invalidate,
  });

  return {
    publishMutation,
    unpublishMutation,
    archiveMutation,
    unarchiveMutation,
    deleteMutation,
    restoreMutation,
  };
}
