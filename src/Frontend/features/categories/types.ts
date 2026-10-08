export type Category = {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
  recipeCount: number;
};

export type CategoryInput = {
  name: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
};
