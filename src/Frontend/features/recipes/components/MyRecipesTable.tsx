"use client";

import { useState } from "react";
import Link from "next/link";
import { useMyRecipes, useRecipeActions } from "../hooks/useMyRecipes";
import type { RecipeFilterTab, RecipeSummary } from "../types";
import { RecipeStatusBadge } from "./RecipeStatusBadge";

const TABS: { key: RecipeFilterTab; label: string }[] = [
  { key: "all", label: "Tất cả" },
  { key: "Draft", label: "Bản nháp" },
  { key: "Published", label: "Đã xuất bản" },
  { key: "Archived", label: "Đã lưu trữ" },
  { key: "trash", label: "Thùng rác" },
];

export function MyRecipesTable() {
  const [activeTab, setActiveTab] = useState<RecipeFilterTab>("all");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { data, isLoading, isError, error } = useMyRecipes(activeTab, page, pageSize);
  const actions = useRecipeActions();

  const [confirmModal, setConfirmModal] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    confirmLabel: string;
    isDanger?: boolean;
    onConfirm: () => Promise<void>;
  }>({
    isOpen: false,
    title: "",
    message: "",
    confirmLabel: "",
    onConfirm: async () => {},
  });

  const [actionError, setActionError] = useState<string | null>(null);

  const handleAction = async (actionFn: () => Promise<void>) => {
    try {
      setActionError(null);
      await actionFn();
      setConfirmModal((prev) => ({ ...prev, isOpen: false }));
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Thao tác thất bại, vui lòng thử lại.";
      setActionError(msg);
    }
  };

  const openConfirm = (
    title: string,
    message: string,
    confirmLabel: string,
    onConfirm: () => Promise<void>,
    isDanger = false,
  ) => {
    setActionError(null);
    setConfirmModal({
      isOpen: true,
      title,
      message,
      confirmLabel,
      isDanger,
      onConfirm,
    });
  };

  return (
    <div className="space-y-6">
      {/* Header & Tạo bài mới */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-zinc-900 dark:text-zinc-100">
            Công thức của tôi
          </h1>
          <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
            Quản lý, xuất bản, lưu trữ và khôi phục các công thức nấu ăn của bạn.
          </p>
        </div>
        <Link
          href="/dashboard/recipes/new"
          className="inline-flex items-center justify-center rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2 dark:focus:ring-offset-zinc-900"
        >
          + Tạo công thức mới
        </Link>
      </div>

      {/* Tabs */}
      <div className="border-b border-zinc-200 dark:border-zinc-800">
        <nav className="-mb-px flex space-x-6 overflow-x-auto">
          {TABS.map((tab) => {
            const isActive = activeTab === tab.key;
            return (
              <button
                key={tab.key}
                type="button"
                onClick={() => {
                  setActiveTab(tab.key);
                  setPage(1);
                }}
                className={`whitespace-nowrap border-b-2 py-3 px-1 text-sm font-medium transition-colors ${
                  isActive
                    ? "border-emerald-600 text-emerald-600 dark:border-emerald-400 dark:text-emerald-400"
                    : "border-transparent text-zinc-500 hover:border-zinc-300 hover:text-zinc-700 dark:text-zinc-400 dark:hover:border-zinc-700 dark:hover:text-zinc-300"
                }`}
              >
                {tab.label}
              </button>
            );
          })}
        </nav>
      </div>

      {/* Thông báo lỗi thao tác nếu có */}
      {actionError && (
        <div className="rounded-lg bg-rose-50 p-4 text-sm text-rose-700 ring-1 ring-inset ring-rose-600/20 dark:bg-rose-950/40 dark:text-rose-400">
          {actionError}
        </div>
      )}

      {/* Table Content */}
      <div className="overflow-hidden rounded-xl border border-zinc-200 bg-white shadow-sm dark:border-zinc-800 dark:bg-zinc-900">
        {isLoading ? (
          <div className="py-20 text-center text-sm text-zinc-500 dark:text-zinc-400">
            Đang tải danh sách công thức...
          </div>
        ) : isError ? (
          <div className="py-20 text-center text-sm text-rose-600 dark:text-rose-400">
            Có lỗi xảy ra: {(error as Error)?.message || "Không thể tải dữ liệu."}
          </div>
        ) : !data || data.items.length === 0 ? (
          <div className="py-20 text-center text-sm text-zinc-500 dark:text-zinc-400">
            Không có công thức nào trong mục này.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-zinc-600 dark:text-zinc-400">
              <thead className="border-b border-zinc-200 bg-zinc-50/75 text-xs font-medium uppercase text-zinc-500 dark:border-zinc-800 dark:bg-zinc-800/50 dark:text-zinc-400">
                <tr>
                  <th scope="col" className="px-6 py-3.5">
                    Tên công thức
                  </th>
                  <th scope="col" className="px-6 py-3.5">
                    Trạng thái
                  </th>
                  <th scope="col" className="px-6 py-3.5">
                    Thời gian nấu
                  </th>
                  <th scope="col" className="px-6 py-3.5">
                    Ngày tạo
                  </th>
                  <th scope="col" className="px-6 py-3.5 text-right">
                    Hành động
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-200 dark:divide-zinc-800">
                {data.items.map((recipe: RecipeSummary) => {
                  const isTrash = activeTab === "trash";
                  return (
                    <tr
                      key={recipe.id}
                      className="hover:bg-zinc-50/50 dark:hover:bg-zinc-800/50 transition-colors"
                    >
                      <td className="px-6 py-4">
                        <div className="font-semibold text-zinc-900 dark:text-zinc-100">
                          {recipe.title}
                        </div>
                        <div className="text-xs text-zinc-500 dark:text-zinc-400">
                          /{recipe.slug}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        {isTrash ? (
                          <span className="inline-flex items-center rounded-full bg-rose-50 px-2.5 py-0.5 text-xs font-medium text-rose-700 ring-1 ring-inset ring-rose-600/20 dark:bg-rose-950/50 dark:text-rose-400">
                            Trong thùng rác
                          </span>
                        ) : (
                          <RecipeStatusBadge status={recipe.status} />
                        )}
                      </td>
                      <td className="px-6 py-4 text-xs">
                        {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút ({recipe.servings} phần)
                      </td>
                      <td className="px-6 py-4 text-xs">
                        {new Date(recipe.createdAt).toLocaleDateString("vi-VN")}
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          {isTrash ? (
                            <button
                              type="button"
                              onClick={() =>
                                openConfirm(
                                  "Khôi phục công thức",
                                  `Bạn có chắc muốn khôi phục công thức "${recipe.title}"?`,
                                  "Khôi phục",
                                  () => actions.restoreMutation.mutateAsync(recipe.id),
                                )
                              }
                              className="rounded px-2 py-1 text-xs font-semibold text-emerald-600 hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/50"
                            >
                              Khôi phục
                            </button>
                          ) : (
                            <>
                              {(recipe.status === "Published" || (recipe.status as unknown as number) === 1) && (
                                <Link
                                  href={`/recipes/${recipe.slug}`}
                                  target="_blank"
                                  className="rounded px-2 py-1 text-xs font-semibold text-blue-600 hover:bg-blue-50 dark:text-blue-400 dark:hover:bg-blue-950/50"
                                >
                                  Xem
                                </Link>
                              )}

                              <Link
                                href={`/dashboard/recipes/${recipe.id}/edit`}
                                className="rounded px-2 py-1 text-xs font-semibold text-zinc-700 hover:bg-zinc-100 dark:text-zinc-300 dark:hover:bg-zinc-800"
                              >
                                Sửa
                              </Link>

                              {(recipe.status === "Draft" || (recipe.status as unknown as number) === 0) && (
                                <button
                                  type="button"
                                  onClick={() =>
                                    openConfirm(
                                      "Xuất bản công thức",
                                      `Xuất bản công thức "${recipe.title}" lên trang công khai? (Yêu cầu ít nhất 1 bước thực hiện)`,
                                      "Xuất bản",
                                      () => actions.publishMutation.mutateAsync(recipe.id),
                                    )
                                  }
                                  className="rounded px-2 py-1 text-xs font-semibold text-emerald-600 hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/50"
                                >
                                  Xuất bản
                                </button>
                              )}

                              {(recipe.status === "Published" || (recipe.status as unknown as number) === 1) && (
                                <button
                                  type="button"
                                  onClick={() =>
                                    openConfirm(
                                      "Gỡ xuất bản",
                                      `Chuyển công thức "${recipe.title}" về bản nháp? Người dùng công khai sẽ không thấy bài viết này nữa.`,
                                      "Gỡ xuất bản",
                                      () => actions.unpublishMutation.mutateAsync(recipe.id),
                                    )
                                  }
                                  className="rounded px-2 py-1 text-xs font-semibold text-amber-600 hover:bg-amber-50 dark:text-amber-400 dark:hover:bg-amber-950/50"
                                >
                                  Gỡ xuất bản
                                </button>
                              )}

                              {(recipe.status === "Published" || (recipe.status as unknown as number) === 1) && (
                                <button
                                  type="button"
                                  onClick={() =>
                                    openConfirm(
                                      "Lưu trữ công thức",
                                      `Lưu trữ công thức "${recipe.title}"?`,
                                      "Lưu trữ",
                                      () => actions.archiveMutation.mutateAsync(recipe.id),
                                    )
                                  }
                                  className="rounded px-2 py-1 text-xs font-semibold text-purple-600 hover:bg-purple-50 dark:text-purple-400 dark:hover:bg-purple-950/50"
                                >
                                  Lưu trữ
                                </button>
                              )}

                              {(recipe.status === "Archived" || (recipe.status as unknown as number) === 2) && (
                                <button
                                  type="button"
                                  onClick={() =>
                                    openConfirm(
                                      "Bỏ lưu trữ",
                                      `Chuyển công thức "${recipe.title}" về bản nháp để tiếp tục chỉnh sửa?`,
                                      "Bỏ lưu trữ",
                                      () => actions.unarchiveMutation.mutateAsync(recipe.id),
                                    )
                                  }
                                  className="rounded px-2 py-1 text-xs font-semibold text-purple-600 hover:bg-purple-50 dark:text-purple-400 dark:hover:bg-purple-950/50"
                                >
                                  Bỏ lưu trữ
                                </button>
                              )}

                              <button
                                type="button"
                                onClick={() =>
                                  openConfirm(
                                    "Xóa vào thùng rác",
                                    `Bạn có chắc chắn muốn chuyển công thức "${recipe.title}" vào thùng rác?`,
                                    "Xóa",
                                    () => actions.deleteMutation.mutateAsync(recipe.id),
                                    true,
                                  )
                                }
                                className="rounded px-2 py-1 text-xs font-semibold text-rose-600 hover:bg-rose-50 dark:text-rose-400 dark:hover:bg-rose-950/50"
                              >
                                Xóa
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}

        {/* Phân trang */}
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-zinc-200 px-6 py-3 dark:border-zinc-800">
            <span className="text-xs text-zinc-500">
              Trang {data.pageNumber} / {data.totalPages} (Tổng {data.totalCount} bài)
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={!data.hasPreviousPage}
                onClick={() => setPage((p) => Math.max(p - 1, 1))}
                className="rounded border border-zinc-300 px-3 py-1 text-xs font-medium text-zinc-700 disabled:opacity-50 dark:border-zinc-700 dark:text-zinc-300"
              >
                Trước
              </button>
              <button
                type="button"
                disabled={!data.hasNextPage}
                onClick={() => setPage((p) => p + 1)}
                className="rounded border border-zinc-300 px-3 py-1 text-xs font-medium text-zinc-700 disabled:opacity-50 dark:border-zinc-700 dark:text-zinc-300"
              >
                Sau
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Confirmation Modal */}
      {confirmModal.isOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 backdrop-blur-xs">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl dark:bg-zinc-900 dark:border dark:border-zinc-800">
            <h3 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
              {confirmModal.title}
            </h3>
            <p className="mt-2 text-sm text-zinc-600 dark:text-zinc-400">
              {confirmModal.message}
            </p>
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setConfirmModal((prev) => ({ ...prev, isOpen: false }))}
                className="rounded-lg border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-700 hover:bg-zinc-50 dark:border-zinc-700 dark:text-zinc-300 dark:hover:bg-zinc-800"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={() => handleAction(confirmModal.onConfirm)}
                className={`rounded-lg px-4 py-2 text-sm font-semibold text-white shadow-xs ${
                  confirmModal.isDanger
                    ? "bg-rose-600 hover:bg-rose-500"
                    : "bg-emerald-600 hover:bg-emerald-500"
                }`}
              >
                {confirmModal.confirmLabel}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
