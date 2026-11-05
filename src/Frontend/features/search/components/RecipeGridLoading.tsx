import styles from "./SearchSurfaces.module.css";

export function RecipeGridLoading() {
  return (
    <div aria-busy="true" aria-label="Đang tải công thức" className={styles.recipeGridLoading}>
      {Array.from({ length: 6 }, (_, index) => (
        <div className={styles.loadingCard} key={index}>
          <div className={styles.loadingMedia} />
          <div className={styles.loadingLine} />
          <div className={styles.loadingLineShort} />
        </div>
      ))}
    </div>
  );
}