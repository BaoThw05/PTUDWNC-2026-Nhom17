import Link from "next/link";

type RecipePaginationProps = {
  page: number;
  totalPages: number;
  query: URLSearchParams;
};

function pageHref(query: URLSearchParams, page: number): string {
  const next = new URLSearchParams(query);
  next.set("page", String(page));
  return `?${next.toString()}`;
}

export function RecipePagination({ page, totalPages, query }: RecipePaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <nav aria-label="Phân trang công thức" className="flex items-center justify-between border-t border-black/10 pt-5">
      {page > 1 ? (
        <Link className="text-sm font-semibold text-[var(--color-leaf)] underline-offset-4 hover:underline" href={pageHref(query, page - 1)}>
          Trang trước
        </Link>
      ) : (
        <span className="text-sm text-[var(--muted)]" aria-disabled="true">Trang trước</span>
      )}
      <span className="text-sm tabular-nums text-[var(--muted)]">
        {page} / {totalPages}
      </span>
      {page < totalPages ? (
        <Link className="text-sm font-semibold text-[var(--color-leaf)] underline-offset-4 hover:underline" href={pageHref(query, page + 1)}>
          Trang sau
        </Link>
      ) : (
        <span className="text-sm text-[var(--muted)]" aria-disabled="true">Trang sau</span>
      )}
    </nav>
  );
}