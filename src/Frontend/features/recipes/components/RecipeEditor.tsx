"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  addRecipeIngredient,
  addRecipeStep,
  deleteRecipeIngredient,
  deleteRecipeStep,
  getRecipeById,
  updateRecipe,
} from "../api/client";
import type { RecipeSummary, UpdateRecipeRequest } from "../types";
import { ApiError } from "@/lib/api/client";

interface RecipeEditorProps {
  recipeId: string;
}

export function RecipeEditor({ recipeId }: RecipeEditorProps) {
  const router = useRouter();
  const [recipe, setRecipe] = useState<RecipeSummary | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [concurrencyConflict, setConcurrencyConflict] = useState<boolean>(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Form fields
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [prepTimeMinutes, setPrepTimeMinutes] = useState(15);
  const [cookTimeMinutes, setCookTimeMinutes] = useState(30);
  const [servings, setServings] = useState(2);
  const [difficulty, setDifficulty] = useState(0);
  const [version, setVersion] = useState<number>(0);

  // Input thêm nguyên liệu mới
  const [newIngName, setNewIngName] = useState("");
  const [newIngQty, setNewIngQty] = useState<string>("");
  const [newIngUnit, setNewIngUnit] = useState("");

  // Input thêm bước mới
  const [newStepTitle, setNewStepTitle] = useState("");
  const [newStepDesc, setNewStepDesc] = useState("");
  const [newStepDuration, setNewStepDuration] = useState<string>("");

  const loadRecipe = useCallback(async () => {
    try {
      setIsLoading(true);
      setErrorMessage(null);
      setConcurrencyConflict(false);
      const data = await getRecipeById(recipeId);
      setRecipe(data);
      setTitle(data.title);
      setDescription(data.description);
      setPrepTimeMinutes(data.prepTimeMinutes);
      setCookTimeMinutes(data.cookTimeMinutes);
      setServings(data.servings);
      setDifficulty(typeof data.difficulty === "number" ? data.difficulty : 0);
      setVersion(data.version);
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || "Không thể tải thông tin công thức.");
    } finally {
      setIsLoading(false);
    }
  }, [recipeId]);

  useEffect(() => {
    let ignore = false;
    async function fetchRecipe() {
      try {
        setIsLoading(true);
        setErrorMessage(null);
        setConcurrencyConflict(false);
        const data = await getRecipeById(recipeId);
        if (!ignore) {
          setRecipe(data);
          setTitle(data.title);
          setDescription(data.description);
          setPrepTimeMinutes(data.prepTimeMinutes);
          setCookTimeMinutes(data.cookTimeMinutes);
          setServings(data.servings);
          setDifficulty(typeof data.difficulty === "number" ? data.difficulty : 0);
          setVersion(data.version);
        }
      } catch (err: unknown) {
        if (!ignore) {
          setErrorMessage((err as Error).message || "Không thể tải thông tin công thức.");
        }
      } finally {
        if (!ignore) {
          setIsLoading(false);
        }
      }
    }

    void fetchRecipe();

    return () => {
      ignore = true;
    };
  }, [recipeId]);

  // Cập nhật thông tin chung (PUT /recipes/{id})
  const handleSaveGeneral = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!recipe) return;

    try {
      setIsSaving(true);
      setErrorMessage(null);
      setSuccessMessage(null);
      setConcurrencyConflict(false);

      const payload: UpdateRecipeRequest = {
        id: recipe.id,
        title: title.trim(),
        description: description.trim(),
        prepTimeMinutes: Number(prepTimeMinutes),
        cookTimeMinutes: Number(cookTimeMinutes),
        servings: Number(servings),
        difficulty: Number(difficulty),
        version: version,
      };

      await updateRecipe(recipe.id, payload);
      setSuccessMessage("Đã lưu thay đổi thành công!");
      await loadRecipe(); // Tải lại để lấy version mới nhất
    } catch (err: unknown) {
      if (err instanceof ApiError && err.status === 409) {
        // Task 2.19: Bắt xung đột phiên bản 409 Concurrency
        setConcurrencyConflict(true);
        setErrorMessage(
          "Dữ liệu đã bị thay đổi bởi phiên làm việc khác! Vui lòng tải lại bài viết để lấy nội dung mới nhất trước khi lưu tiếp.",
        );
      } else {
        setErrorMessage((err as Error).message || "Lỗi khi lưu bài viết.");
      }
    } finally {
      setIsSaving(false);
    }
  };

  // Thêm nguyên liệu
  const handleAddIngredient = async () => {
    if (!newIngName.trim()) return;
    try {
      await addRecipeIngredient(recipeId, {
        name: newIngName.trim(),
        quantity: newIngQty ? Number(newIngQty) : null,
        unit: newIngUnit.trim() || null,
      });
      setNewIngName("");
      setNewIngQty("");
      setNewIngUnit("");
      await loadRecipe();
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || "Không thể thêm nguyên liệu.");
    }
  };

  // Xóa nguyên liệu
  const handleDeleteIngredient = async (ingredientId: string) => {
    try {
      await deleteRecipeIngredient(recipeId, ingredientId);
      await loadRecipe();
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || "Không thể xóa nguyên liệu.");
    }
  };

  // Thêm bước
  const handleAddStep = async () => {
    if (!newStepDesc.trim()) return;
    try {
      await addRecipeStep(recipeId, {
        title: newStepTitle.trim() || undefined,
        description: newStepDesc.trim(),
        durationMinutes: newStepDuration ? Number(newStepDuration) : null,
      });
      setNewStepTitle("");
      setNewStepDesc("");
      setNewStepDuration("");
      await loadRecipe();
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || "Không thể thêm bước thực hiện.");
    }
  };

  // Xóa bước
  const handleDeleteStep = async (stepId: string) => {
    try {
      await deleteRecipeStep(recipeId, stepId);
      await loadRecipe();
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || "Không thể xóa bước thực hiện.");
    }
  };

  if (isLoading) {
    return (
      <div className="py-20 text-center text-sm text-zinc-500">
        Đang tải dữ liệu công thức...
      </div>
    );
  }

  if (!recipe) {
    return (
      <div className="py-20 text-center text-sm text-rose-500">
        Không tìm thấy công thức này.
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <button
            type="button"
            onClick={() => router.push("/dashboard/recipes")}
            className="text-xs text-zinc-500 hover:text-zinc-700 dark:hover:text-zinc-300 mb-1"
          >
            ← Quay lại danh sách
          </button>
          <h1 className="text-2xl font-bold tracking-tight text-zinc-900 dark:text-zinc-100">
            Chỉnh sửa công thức
          </h1>
          <p className="text-xs text-zinc-500">
            Phiên bản (Version): {version} | Slug: /{recipe.slug}
          </p>
        </div>
      </div>

      {/* CẢNH BÁO XUNG ĐỘT 409 (Task 2.19) */}
      {concurrencyConflict && (
        <div className="rounded-xl border border-rose-600 bg-rose-50 p-5 shadow-xs dark:bg-rose-950/40">
          <div className="flex items-start justify-between">
            <div>
              <h3 className="text-sm font-bold text-rose-800 dark:text-rose-300">
                ⚠️ Xung đột dữ liệu (409 Conflict)
              </h3>
              <p className="mt-1 text-sm text-rose-700 dark:text-rose-400">
                {errorMessage}
              </p>
            </div>
            <button
              type="button"
              onClick={loadRecipe}
              className="rounded-lg bg-rose-600 px-4 py-2 text-xs font-semibold text-white shadow-xs hover:bg-rose-500"
            >
              🔄 Tải lại dữ liệu mới nhất
            </button>
          </div>
        </div>
      )}

      {/* Thông báo thường */}
      {!concurrencyConflict && errorMessage && (
        <div className="rounded-lg bg-rose-50 p-4 text-sm text-rose-700 ring-1 ring-rose-600/20 dark:bg-rose-950/40 dark:text-rose-400">
          {errorMessage}
        </div>
      )}

      {successMessage && (
        <div className="rounded-lg bg-emerald-50 p-4 text-sm text-emerald-700 ring-1 ring-emerald-600/20 dark:bg-emerald-950/40 dark:text-emerald-400">
          {successMessage}
        </div>
      )}

      {/* PHẦN 1: THÔNG TIN CHUNG */}
      <form onSubmit={handleSaveGeneral} className="rounded-xl border border-zinc-200 bg-white p-6 shadow-xs dark:border-zinc-800 dark:bg-zinc-900 space-y-4">
        <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
          Thông tin cơ bản
        </h2>

        <div>
          <label className="block text-sm font-medium text-zinc-700 dark:text-zinc-300">
            Tiêu đề *
          </label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-zinc-700 dark:text-zinc-300">
            Mô tả *
          </label>
          <textarea
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
          />
        </div>

        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          <div>
            <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">Chuẩn bị (phút)</label>
            <input
              type="number"
              min="1"
              value={prepTimeMinutes}
              onChange={(e) => setPrepTimeMinutes(Number(e.target.value))}
              className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-1.5 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">Nấu (phút)</label>
            <input
              type="number"
              min="0"
              value={cookTimeMinutes}
              onChange={(e) => setCookTimeMinutes(Number(e.target.value))}
              className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-1.5 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">Khẩu phần</label>
            <input
              type="number"
              min="1"
              value={servings}
              onChange={(e) => setServings(Number(e.target.value))}
              className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-1.5 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">Độ khó</label>
            <select
              value={difficulty}
              onChange={(e) => setDifficulty(Number(e.target.value))}
              className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-1.5 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
            >
              <option value={0}>Dễ</option>
              <option value={1}>Trung bình</option>
              <option value={2}>Khó</option>
            </select>
          </div>
        </div>

        <div className="pt-2 flex justify-end">
          <button
            type="submit"
            disabled={isSaving}
            className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white shadow-xs hover:bg-emerald-500 disabled:opacity-50"
          >
            {isSaving ? "Đang lưu..." : "Lưu thông tin chung"}
          </button>
        </div>
      </form>

      {/* PHẦN 2: NGUYÊN LIỆU */}
      <div className="rounded-xl border border-zinc-200 bg-white p-6 shadow-xs dark:border-zinc-800 dark:bg-zinc-900 space-y-4">
        <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
          Nguyên liệu
        </h2>

        {/* Danh sách hiện tại */}
        <div className="divide-y divide-zinc-200 dark:divide-zinc-800">
          {recipe.ingredients.map((ing) => (
            <div key={ing.id} className="flex items-center justify-between py-2 text-sm">
              <span className="font-medium text-zinc-900 dark:text-zinc-100">
                {ing.name} ({ing.quantity ? `${ing.quantity} ${ing.unit || ""}` : ing.unit || "Tùy ý"})
              </span>
              <button
                type="button"
                onClick={() => handleDeleteIngredient(ing.id)}
                className="text-xs text-rose-500 hover:text-rose-700"
              >
                Xóa
              </button>
            </div>
          ))}
        </div>

        {/* Thêm mới nguyên liệu */}
        <div className="flex gap-2 pt-3 border-t border-zinc-200 dark:border-zinc-800">
          <input
            type="text"
            placeholder="Tên nguyên liệu mới..."
            value={newIngName}
            onChange={(e) => setNewIngName(e.target.value)}
            className="flex-1 rounded-lg border border-zinc-300 px-3 py-1.5 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
          />
          <input
            type="number"
            placeholder="Số lượng"
            value={newIngQty}
            onChange={(e) => setNewIngQty(e.target.value)}
            className="w-24 rounded-lg border border-zinc-300 px-3 py-1.5 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
          />
          <input
            type="text"
            placeholder="Đơn vị"
            value={newIngUnit}
            onChange={(e) => setNewIngUnit(e.target.value)}
            className="w-24 rounded-lg border border-zinc-300 px-3 py-1.5 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
          />
          <button
            type="button"
            onClick={handleAddIngredient}
            className="rounded-lg bg-zinc-800 px-3 py-1.5 text-xs font-semibold text-white hover:bg-zinc-700 dark:bg-zinc-200 dark:text-zinc-900"
          >
            Thêm
          </button>
        </div>
      </div>

      {/* PHẦN 3: CÁC BƯỚC THỰC HIỆN */}
      <div className="rounded-xl border border-zinc-200 bg-white p-6 shadow-xs dark:border-zinc-800 dark:bg-zinc-900 space-y-4">
        <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
          Các bước thực hiện
        </h2>

        {/* Danh sách các bước */}
        <div className="space-y-3">
          {recipe.steps.map((st) => (
            <div key={st.id} className="rounded-lg border border-zinc-200 p-3 space-y-1 dark:border-zinc-800">
              <div className="flex items-center justify-between">
                <span className="font-bold text-sm text-zinc-900 dark:text-zinc-100">
                  Bước {st.stepNumber}: {st.title} {st.durationMinutes && `(${st.durationMinutes} phút)`}
                </span>
                <button
                  type="button"
                  onClick={() => handleDeleteStep(st.id)}
                  className="text-xs text-rose-500 hover:text-rose-700"
                >
                  Xóa bước
                </button>
              </div>
              <p className="text-xs text-zinc-600 dark:text-zinc-400">{st.description}</p>
            </div>
          ))}
        </div>

        {/* Thêm bước mới */}
        <div className="rounded-lg border border-dashed border-zinc-300 p-3 space-y-2 dark:border-zinc-700">
          <span className="text-xs font-semibold text-zinc-500">Thêm bước mới</span>
          <div className="flex gap-2">
            <input
              type="text"
              placeholder="Tiêu đề bước (ví dụ: Nấu sôi)"
              value={newStepTitle}
              onChange={(e) => setNewStepTitle(e.target.value)}
              className="flex-1 rounded-md border border-zinc-300 px-2.5 py-1 text-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
            />
            <input
              type="number"
              placeholder="Phút"
              value={newStepDuration}
              onChange={(e) => setNewStepDuration(e.target.value)}
              className="w-20 rounded-md border border-zinc-300 px-2.5 py-1 text-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
            />
          </div>
          <textarea
            rows={2}
            placeholder="Mô tả bước làm..."
            value={newStepDesc}
            onChange={(e) => setNewStepDesc(e.target.value)}
            className="w-full rounded-md border border-zinc-300 p-2 text-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
          />
          <div className="flex justify-end">
            <button
              type="button"
              onClick={handleAddStep}
              className="rounded-lg bg-zinc-800 px-3 py-1 text-xs font-semibold text-white hover:bg-zinc-700 dark:bg-zinc-200 dark:text-zinc-900"
            >
              Thêm bước
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
