export type RecipeImage = {
  imageId: string;
  originalUrl: string;
  altText: string | null;
  isPrimary: boolean;
  orderIndex: number;
};

export type RecipeImagePatch = {
  altText?: string | null;
  isPrimary?: boolean;
  orderIndex?: number;
};
