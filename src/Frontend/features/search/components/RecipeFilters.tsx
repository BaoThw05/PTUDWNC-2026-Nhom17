"use client";

import { useSearchParams } from "next/navigation";
import type { RecipeCategory, RecipeFilters as RecipeFilterValues } from "@/features/search/api/recipes";
import styles from "./SearchSurfaces.module.css";

type RecipeFiltersProps = {
  action: "/recipes" | "/search";
  values: RecipeFilterValues;
  categories: RecipeCategory[];
  categoryError?: string;
  searchTerm?: string;
  showSort?: boolean;
};

const fieldClassName =
  "mt-1 h-11 w-full border border-black/15 bg-white px-3 text-sm text-[var(--foreground)] outline-none focus:border-[var(--color-leaf)] focus:ring-2 focus:ring-[var(--color-leaf)]/20";

export function RecipeFilters({
  action,
  values,
  categories,
  categoryError,
  searchTerm,
  showSort = true,
}: RecipeFiltersProps) {
  const searchParams = useSearchParams();

  return (
    <form key={searchParams.toString()} action={action} method="get" className="border-y border-black/10 py-4">
      {searchTerm !== undefined && <input type="hidden" name="q" value={searchTerm} />}
      <div className={styles.filterGrid}>
        <label className="text-xs font-semibold text-[var(--muted)]">
          Danh mục
          <select className={fieldClassName} name="categoryId" defaultValue={values.categoryId ?? ""}>
            <option value="">Tất cả danh mục</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>{category.name}</option>
            ))}
          </select>
        </label>
        <label className="text-xs font-semibold text-[var(--muted)]">
          Độ khó
          <select className={fieldClassName} name="difficulty" defaultValue={values.difficulty ?? ""}>
            <option value="">Mọi mức độ</option>
            <option value="Easy">Dễ</option>
            <option value="Medium">Vừa</option>
            <option value="Hard">Khó</option>
          </select>
        </label>
        <label className="text-xs font-semibold text-[var(--muted)]">
          Thời gian nấu tối đa
          <input className={fieldClassName} type="number" min="0" name="maxCookTime" placeholder="Phút" defaultValue={values.maxCookTime ?? ""} />
        </label>
        <label className="text-xs font-semibold text-[var(--muted)]">
          Khẩu phần từ
          <input className={fieldClassName} type="number" min="1" name="minServings" placeholder="Số người" defaultValue={values.minServings ?? ""} />
        </label>
        {showSort ? (
          <label className="text-xs font-semibold text-[var(--muted)]">
            Sắp xếp
            <select className={fieldClassName} name="sort" defaultValue={values.sort ?? "-createdAt"}>
              <option value="-createdAt">Mới nhất</option>
              <option value="createdAt">Cũ nhất</option>
              <option value="title">Tên A–Z</option>
              <option value="-title">Tên Z–A</option>
              <option value="cookTime">Nấu nhanh trước</option>
              <option value="-cookTime">Nấu lâu trước</option>
            </select>
          </label>
        ) : (
          <label className="text-xs font-semibold text-[var(--muted)]">
            Số kết quả
            <select className={fieldClassName} name="pageSize" defaultValue={values.pageSize ?? "12"}>
              <option value="10">10 mỗi trang</option>
              <option value="12">12 mỗi trang</option>
              <option value="24">24 mỗi trang</option>
              <option value="50">50 mỗi trang</option>
            </select>
          </label>
        )}
      </div>
      <div className="mt-4 flex flex-wrap items-center gap-4">
        {showSort && (
          <label className="text-xs font-semibold text-[var(--muted)]">
            Số kết quả
            <select className="ml-2 h-10 border border-black/15 bg-white px-3 text-sm font-normal text-[var(--foreground)]" name="pageSize" defaultValue={values.pageSize ?? "12"}>
              <option value="12">12</option>
              <option value="24">24</option>
              <option value="50">50</option>
            </select>
          </label>
        )}
        <button className="h-10 bg-[var(--color-leaf)] px-5 text-sm font-semibold text-white hover:bg-[var(--color-leaf-dark)] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-leaf)]" type="submit">
          Áp dụng
        </button>
        <a className="text-sm font-semibold text-[var(--muted)] underline-offset-4 hover:underline" href={action}>
          Xóa bộ lọc
        </a>
        {categoryError && <span className="text-sm text-[var(--color-tomato)]" role="status">Danh mục chưa tải được.</span>}
      </div>
    </form>
  );
}