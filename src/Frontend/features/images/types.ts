export type RecipeImage = {
  imageId: string;
  originalUrl: string;
  mediumUrl?: string | null;
  thumbnailUrl?: string | null;
  altText: string | null;
  isPrimary: boolean;
  orderIndex: number;
};

export type RecipeImagePatch = {
  altText?: string | null;
  isPrimary?: boolean;
  orderIndex?: number;
};
