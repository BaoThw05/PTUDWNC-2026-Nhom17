import type { RecipeSummary } from "@/features/search/api/recipes";
import { RecipeCard } from "./RecipeCard";
import styles from "./SearchSurfaces.module.css";

export function RecipeGrid({ recipes }: { recipes: RecipeSummary[] }) {
  if (recipes.length === 0) {
    return (
      <p className="border-y border-black/10 py-8 text-sm text-[var(--muted)]">
        Chưa có công thức để hiển thị.
      </p>
    );
  }

  return (
    <div className={styles.recipeGrid}>
      {recipes.map((recipe) => (
        <RecipeCard key={recipe.id} recipe={recipe} />
      ))}
    </div>
  );
}