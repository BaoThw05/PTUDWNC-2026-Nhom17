export type RecipeStatus = "Draft" | "Published" | "Archived" | "Deleted";
export type Difficulty = "Easy" | "Medium" | "Hard" | 0 | 1 | 2;

export interface RecipeNutrition {
  calories?: number | null;
  proteinGrams?: number | null;
  fatGrams?: number | null;
  carbsGrams?: number | null;
  fiberGrams?: number | null;
  sugarGrams?: number | null;
}

export interface RecipeStep {
  id: string;
  stepNumber: number;
  title: string;
  description: string;
  durationMinutes?: number | null;
}

export interface RecipeIngredient {
  id: string;
  name: string;
  quantity?: number | null;
  unit?: string | null;
  orderIndex: number;
}

export interface RecipeSummary {
  id: string;
  title: string;
  slug: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: Difficulty;
  status: RecipeStatus | number;
  publishedAt?: string | null;
  authorId: string;
  categoryId: string;
  version: number;
  createdAt: string;
  updatedAt: string;
  nutrition?: RecipeNutrition | null;
  steps: RecipeStep[];
  ingredients: RecipeIngredient[];
}

export interface CreateRecipeStepDto {
  title?: string;
  description: string;
  durationMinutes?: number | null;
}

export interface CreateRecipeIngredientDto {
  name: string;
  quantity?: number | null;
  unit?: string | null;
}

export interface CreateRecipeRequest {
  title: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: number;
  categoryId?: string | null;
  nutrition?: RecipeNutrition | null;
  steps?: CreateRecipeStepDto[];
  ingredients?: CreateRecipeIngredientDto[];
}

export interface UpdateRecipeRequest {
  id: string;
  title: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: number;
  categoryId?: string | null;
  nutrition?: RecipeNutrition | null;
  version: number;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export type RecipeFilterTab = "all" | "Draft" | "Published" | "Archived" | "trash";
