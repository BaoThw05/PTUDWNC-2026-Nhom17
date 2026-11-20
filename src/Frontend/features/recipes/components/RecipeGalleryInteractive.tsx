"use client";

/* eslint-disable @next/next/no-img-element */
import type { RecipeImage } from "@/features/images/types";
import { useState } from "react";

export function RecipeGalleryInteractive({ images }: { images: RecipeImage[] }) {
  const primary = images.find((image) => image.isPrimary) ?? images[0];
  const [selectedIndex, setSelectedIndex] = useState(images.indexOf(primary));
  const activeImage = images[selectedIndex] ?? primary;

  return (
    <div className="space-y-4">
      <figure className="relative aspect-video w-full overflow-hidden rounded-2xl border border-zinc-200 bg-zinc-100 shadow-sm dark:border-zinc-800 dark:bg-zinc-800">
        <img
          src={activeImage.originalUrl}
          alt={activeImage.altText || "Hình ảnh món ăn"}
          fetchPriority={activeImage.isPrimary ? "high" : "auto"}
          className="h-full w-full object-cover"
        />
        {activeImage.altText && (
          <figcaption className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/70 via-black/40 to-transparent p-4 text-sm text-white">
            {activeImage.altText}
          </figcaption>
        )}
      </figure>
      <div className="flex gap-2 overflow-x-auto pb-2">
        {images.map((image, index) => (
          <button
            key={image.imageId}
            type="button"
            onClick={() => setSelectedIndex(index)}
            aria-label={`Xem ảnh ${index + 1}`}
            aria-pressed={selectedIndex === index}
            className={`relative h-20 w-28 shrink-0 overflow-hidden rounded-xl border-2 transition ${
              selectedIndex === index
                ? "border-orange-600 ring-2 ring-orange-500/30"
                : "border-transparent opacity-70 hover:opacity-100"
            }`}
          >
            <img
              src={image.originalUrl}
              alt={image.altText || `Ảnh ${index + 1}`}
              loading="lazy"
              className="h-full w-full object-cover"
            />
          </button>
        ))}
      </div>
    </div>
  );
}
