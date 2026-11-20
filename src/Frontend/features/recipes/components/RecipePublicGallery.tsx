/* eslint-disable @next/next/no-img-element */
import type { RecipeImage } from "@/features/images/types";
import { RecipeGalleryInteractive } from "./RecipeGalleryInteractive";

export function RecipePublicGallery({ images }: { images: RecipeImage[] }) {
  if (images.length === 0) return null;
  if (images.length > 1) return <RecipeGalleryInteractive images={images} />;

  const image = images[0];
  return (
    <figure className="relative aspect-video w-full overflow-hidden rounded-2xl border border-zinc-200 bg-zinc-100 shadow-sm dark:border-zinc-800 dark:bg-zinc-800">
      <img
        src={image.thumbnailUrl ?? image.mediumUrl ?? image.originalUrl}
        alt={image.altText || "Hình ảnh món ăn"}
        fetchPriority="high"
        className="h-full w-full object-cover"
      />
      {image.altText && (
        <figcaption className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/70 via-black/40 to-transparent p-4 text-sm text-white">
          {image.altText}
        </figcaption>
      )}
    </figure>
  );
}
