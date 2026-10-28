import styles from "./SearchSurfaces.module.css";

export function RecipeSearchForm({
  defaultValue = "",
  compact = false,
}: {
  defaultValue?: string;
  compact?: boolean;
}) {
  return (
    <form action="/search" method="get" role="search" className={`${styles.recipeSearchForm}${compact ? ` ${styles.compactSearchForm}` : ""}`}>
      <label className="sr-only" htmlFor={compact ? "header-search" : "page-search"}>Tìm công thức</label>
      <input
        id={compact ? "header-search" : "page-search"}
        autoComplete="off"
        minLength={2}
        name="q"
        placeholder="Thử “phở bò” hoặc “bánh xèo”"
        required
        type="search"
        defaultValue={defaultValue}
      />
      <button type="submit">Tìm</button>
    </form>
  );
}