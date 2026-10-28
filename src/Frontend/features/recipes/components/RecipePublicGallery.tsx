"use client";

/* eslint-disable @next/next/no-img-element */
import { useQuery } from "@tanstack/react-query";
import { getRecipeImages } from "@/features/images/api/client";
import { useState } from "react";

export function RecipePublicGallery({ recipeId }: { recipeId: string }) {
  const { data: images, isLoading } = useQuery({
    queryKey: ["recipe-images", recipeId],
    queryFn: () => getRecipeImages(recipeId),
    retry: false,
  });

  const [selectedIndex, setSelectedIndex] = useState(0);

  if (isLoading) {
    return (
      <div className="h-64 w-full animate-pulse rounded-2xl bg-zinc-100 dark:bg-zinc-800" />
    );
  }

  if (!images || images.length === 0) {
    return null;
  }

  const primaryOrFirst = images.find((img) => img.isPrimary) || images[0];
  const activeImage = images[selectedIndex] || primaryOrFirst;

  return (
    <div className="space-y-4">
      {/* Ảnh chính to */}
      <div className="relative aspect-video w-full overflow-hidden rounded-2xl border border-zinc-200 bg-zinc-100 shadow-sm dark:border-zinc-800 dark:bg-zinc-800">
        <img
          src={activeImage.originalUrl}
          alt={activeImage.altText || "Hình ảnh món ăn"}
          className="h-full w-full object-cover"
        />
        {activeImage.altText && (
          <div className="absolute bottom-0 inset-x-0 bg-gradient-to-t from-black/70 via-black/40 to-transparent p-4 text-white text-sm">
            {activeImage.altText}
          </div>
        )}
      </div>

      {/* Danh sách ảnh nhỏ thumbnails nếu có nhiều hơn 1 ảnh */}
      {images.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {images.map((img, idx) => (
            <button
              key={img.imageId}
              type="button"
              onClick={() => setSelectedIndex(idx)}
              className={`relative h-20 w-28 shrink-0 overflow-hidden rounded-xl border-2 transition ${
                selectedIndex === idx
                  ? "border-orange-600 ring-2 ring-orange-500/30"
                  : "border-transparent opacity-70 hover:opacity-100"
              }`}
            >
              <img
                src={img.originalUrl}
                alt={img.altText || `Ảnh ${idx + 1}`}
                className="h-full w-full object-cover"
              />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
