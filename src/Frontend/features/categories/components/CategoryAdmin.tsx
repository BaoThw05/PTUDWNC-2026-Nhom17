"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useState, type FormEvent } from "react";
import { ApiError } from "@/lib/api/client";
import { createCategory, deleteCategory, getCategories, updateCategory } from "../api/client";
import type { Category, CategoryInput } from "../types";

const queryKey = ["categories"] as const;
const emptyForm = { name: "", description: "", imageUrl: "", orderIndex: 0 };

function describeError(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.code === "CATEGORY_NAME_EXISTS") return "Tên danh mục đã tồn tại. Hãy chọn tên khác.";
    if (error.code === "CATEGORY_DELETE_HAS_RECIPES")
      return error.problem.detail ?? "Danh mục còn công thức nên chưa thể xóa.";
    if (error.status === 403) return "Bạn không có quyền quản lý danh mục.";
    if (error.status === 401) return "Phiên đăng nhập đã hết hạn. Hãy đăng nhập lại.";
    if (error.code === "CATEGORY_NOT_FOUND") return "Danh mục không còn tồn tại. Hãy tải lại danh sách.";
    if (error.status === 400) return "Thông tin danh mục chưa hợp lệ. Hãy kiểm tra lại.";
    return error.problem.detail ?? "Không thể lưu danh mục. Hãy thử lại.";
  }
  return "Không thể kết nối đến máy chủ. Hãy thử lại.";
}

export function CategoryAdmin() {
  const queryClient = useQueryClient();
  const { data: categories, isPending, error: loadError, refetch } = useQuery({ queryKey, queryFn: getCategories });
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  function startEdit(category: Category) {
    setEditingId(category.id);
    setForm({
      name: category.name,
      description: category.description ?? "",
      imageUrl: category.imageUrl ?? "",
      orderIndex: category.orderIndex,
    });
    setError(null);
    setMessage(null);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
  }

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setMessage(null);
    const input: CategoryInput = {
      name: form.name.trim(),
      description: form.description.trim() || null,
      imageUrl: form.imageUrl.trim() || null,
      orderIndex: form.orderIndex,
    };
    if (input.name.length < 2 || input.name.length > 100 || input.orderIndex < 0 || !Number.isInteger(input.orderIndex)) {
      setError("Tên phải có 2–100 ký tự và thứ tự phải là số nguyên không âm.");
      return;
    }
    setBusy(true);
    try {
      if (editingId) {
        await updateCategory(editingId, input);
        setMessage("Đã cập nhật danh mục.");
      } else {
        await createCategory(input);
        setMessage("Đã tạo danh mục.");
      }
      resetForm();
      await queryClient.invalidateQueries({ queryKey });
    } catch (saveError) {
      setError(describeError(saveError));
    } finally {
      setBusy(false);
    }
  }

  async function remove(category: Category) {
    if (!window.confirm(`Xóa danh mục “${category.name}”?`)) return;
    setError(null);
    setMessage(null);
    setBusy(true);
    try {
      await deleteCategory(category.id);
      if (editingId === category.id) resetForm();
      setMessage(`Đã xóa danh mục “${category.name}”.`);
      await queryClient.invalidateQueries({ queryKey });
    } catch (deleteError) {
      setError(describeError(deleteError));
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="space-y-8">
      <header>
        <h1 className="text-3xl font-semibold tracking-tight">Quản lý danh mục</h1>
        <p className="mt-2 text-sm text-zinc-600 dark:text-zinc-400">
          Tạo và chỉnh sửa danh mục để sắp xếp công thức trên trang công khai.
        </p>
      </header>

      <form onSubmit={(event) => void save(event)} className="space-y-4 rounded-2xl border border-black/10 p-5 dark:border-white/15">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-lg font-semibold">{editingId ? "Sửa danh mục" : "Thêm danh mục"}</h2>
          {editingId && <button type="button" onClick={resetForm} disabled={busy} className="text-sm text-blue-700 hover:underline dark:text-blue-300">Hủy sửa</button>}
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <label className="block text-sm font-medium">
            Tên danh mục <span aria-hidden="true">*</span>
            <input required minLength={2} maxLength={100} value={form.name}
              onChange={(event) => setForm({ ...form, name: event.target.value })}
              className="mt-1 block w-full rounded-lg border border-black/20 bg-transparent px-3 py-2 dark:border-white/25" />
          </label>
          <label className="block text-sm font-medium">
            Thứ tự hiển thị
            <input type="number" min={0} step={1} required value={form.orderIndex}
              onChange={(event) => setForm({ ...form, orderIndex: Number(event.target.value) })}
              className="mt-1 block w-full rounded-lg border border-black/20 bg-transparent px-3 py-2 dark:border-white/25" />
          </label>
        </div>
        <label className="block text-sm font-medium">
          Mô tả
          <textarea maxLength={2000} rows={3} value={form.description}
            onChange={(event) => setForm({ ...form, description: event.target.value })}
            className="mt-1 block w-full rounded-lg border border-black/20 bg-transparent px-3 py-2 dark:border-white/25" />
        </label>
        <label className="block text-sm font-medium">
          URL ảnh đại diện
          <input type="text" maxLength={500} value={form.imageUrl}
            onChange={(event) => setForm({ ...form, imageUrl: event.target.value })}
            placeholder="/media/... hoặc https://..."
            className="mt-1 block w-full rounded-lg border border-black/20 bg-transparent px-3 py-2 dark:border-white/25" />
        </label>
        <button type="submit" disabled={busy}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
          {busy ? "Đang lưu…" : editingId ? "Lưu thay đổi" : "Tạo danh mục"}
        </button>
      </form>

      {error && <p role="alert" className="rounded-lg bg-red-50 p-3 text-sm text-red-800 dark:bg-red-950 dark:text-red-200">{error}</p>}
      {message && <p role="status" className="rounded-lg bg-green-50 p-3 text-sm text-green-800 dark:bg-green-950 dark:text-green-200">{message}</p>}

      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold">Danh sách danh mục{categories ? ` (${categories.length})` : ""}</h2>
          <button type="button" onClick={() => void refetch()} className="text-sm text-blue-700 hover:underline dark:text-blue-300">Tải lại</button>
        </div>
        {isPending ? <p className="text-sm text-zinc-600 dark:text-zinc-400">Đang tải danh mục…</p> :
          loadError ? <p role="alert" className="text-sm text-red-700 dark:text-red-300">{describeError(loadError)}</p> :
          categories?.length === 0 ? <p className="rounded-xl border p-6 text-sm">Chưa có danh mục. Hãy tạo danh mục đầu tiên.</p> : (
            <ul className="divide-y divide-black/10 overflow-hidden rounded-2xl border border-black/10 dark:divide-white/15 dark:border-white/15">
              {categories?.map((category) => (
                <li key={category.id} className="flex flex-wrap items-center justify-between gap-4 p-4">
                  <div className="min-w-0">
                    <Link href={`/categories/${encodeURIComponent(category.slug)}`} className="font-medium hover:underline">{category.name}</Link>
                    <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
                      /{category.slug} · {category.recipeCount} công thức · thứ tự {category.orderIndex}
                    </p>
                    {category.description && <p className="mt-1 line-clamp-2 text-sm">{category.description}</p>}
                  </div>
                  <div className="flex gap-3 text-sm">
                    <button type="button" disabled={busy} onClick={() => startEdit(category)}
                      className="text-blue-700 hover:underline disabled:opacity-50 dark:text-blue-300">Sửa</button>
                    <button type="button" disabled={busy} onClick={() => void remove(category)}
                      className="text-red-700 hover:underline disabled:opacity-50 dark:text-red-300">Xóa</button>
                  </div>
                </li>
              ))}
            </ul>
          )}
      </div>
    </section>
  );
}
