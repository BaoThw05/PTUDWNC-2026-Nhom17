import Image from "next/image";
import Link from "next/link";
import type { RecipeSummary } from "@/features/search/api/recipes";
import styles from "./SearchSurfaces.module.css";

function difficultyLabel(difficulty: RecipeSummary["difficulty"]): string {
  if (difficulty === 0 || difficulty === "Easy") return "Dễ";
  if (difficulty === 1 || difficulty === "Medium") return "Vừa";
  if (difficulty === 2 || difficulty === "Hard") return "Khó";
  return "Độ khó chưa rõ";
}

export function RecipeCard({ recipe }: { recipe: RecipeSummary }) {
  return (
    <article className={`${styles.recipeCard} group`}>
      <Link
        href={`/recipes/${encodeURIComponent(recipe.slug)}`}
        className="block focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-[var(--color-leaf)]"
      >
        <div className={styles.recipeCardMedia}>
          {recipe.thumbnailUrl ? (
            <Image
              src={recipe.thumbnailUrl}
              alt={`Ảnh món ${recipe.title}`}
              fill
              sizes="(max-width: 767px) 100vw, (max-width: 1199px) 50vw, 33vw"
              unoptimized
              className="object-cover transition-transform duration-300 group-hover:scale-[1.03]"
            />
          ) : (
            <div className={styles.noImage} aria-label="Công thức chưa có ảnh">
              <span className={styles.noImageMark} aria-hidden="true">CB</span>
              <small>Chưa có ảnh</small>
            </div>
          )}
        </div>
        <div className="flex items-start justify-between gap-3 pt-4">
          <h2 className="font-serif text-xl font-semibold leading-snug text-[var(--foreground)] group-hover:text-[var(--color-leaf)]">
            {recipe.title}
          </h2>
          <span className="shrink-0 text-sm font-medium text-[var(--color-tomato)]">
            {recipe.cookTimeMinutes} phút
          </span>
        </div>
        <p className="mt-2 line-clamp-2 text-sm leading-6 text-[var(--muted)]">
          {recipe.description}
        </p>
      </Link>
      <div className="mt-3 flex flex-wrap gap-x-4 gap-y-1 border-t border-black/10 pt-3 text-xs text-[var(--muted)]">
        <span>{difficultyLabel(recipe.difficulty)}</span>
        <span>{recipe.servings} khẩu phần</span>
      </div>
    </article>
  );
}