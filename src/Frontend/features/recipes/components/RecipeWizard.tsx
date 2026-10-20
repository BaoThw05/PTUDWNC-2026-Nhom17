"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { createRecipe, publishRecipe } from "../api/client";
import type {
  CreateRecipeIngredientDto,
  CreateRecipeRequest,
  CreateRecipeStepDto,
  RecipeNutrition,
} from "../types";
import { ApiError } from "@/lib/api/client";

export function RecipeWizard() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState<number>(1);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  // B1: Thông tin chung & dinh dưỡng
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [prepTimeMinutes, setPrepTimeMinutes] = useState<number>(15);
  const [cookTimeMinutes, setCookTimeMinutes] = useState<number>(30);
  const [servings, setServings] = useState<number>(2);
  const [difficulty, setDifficulty] = useState<number>(0); // 0: Dễ, 1: Trung bình, 2: Khó
  const [nutrition, setNutrition] = useState<RecipeNutrition>({
    calories: null,
    proteinGrams: null,
    fatGrams: null,
    carbsGrams: null,
  });

  // B2: Nguyên liệu
  const [ingredients, setIngredients] = useState<CreateRecipeIngredientDto[]>([
    { name: "", quantity: null, unit: "" },
  ]);

  // B3: Các bước thực hiện
  const [steps, setSteps] = useState<CreateRecipeStepDto[]>([
    { title: "Bước 1", description: "", durationMinutes: 10 },
  ]);

  // Điều hướng wizard
  const nextStep = () => {
    setErrorMessage(null);
    setFieldErrors({});

    if (currentStep === 1) {
      const errors: Record<string, string> = {};
      if (!title.trim() || title.trim().length < 5) {
        errors.title = "Tiêu đề phải từ 5 ký tự trở lên.";
      }
      if (!description.trim()) {
        errors.description = "Vui lòng nhập mô tả món ăn.";
      }
      if (prepTimeMinutes <= 0) {
        errors.prepTimeMinutes = "Thời gian chuẩn bị phải lớn hơn 0.";
      }
      if (servings <= 0) {
        errors.servings = "Khẩu phần phải lớn hơn 0.";
      }

      if (Object.keys(errors).length > 0) {
        setFieldErrors(errors);
        return;
      }
    }

    if (currentStep === 2) {
      const validIngredients = ingredients.filter((i) => i.name.trim().length > 0);
      if (validIngredients.length === 0) {
        setErrorMessage("Vui lòng thêm ít nhất một nguyên liệu.");
        return;
      }
    }

    if (currentStep === 3) {
      const validSteps = steps.filter((s) => s.description.trim().length > 0);
      if (validSteps.length === 0) {
        setErrorMessage("Vui lòng nhập ít nhất một bước thực hiện.");
        return;
      }
    }

    setCurrentStep((prev) => Math.min(prev + 1, 4));
  };

  const prevStep = () => {
    setErrorMessage(null);
    setCurrentStep((prev) => Math.max(prev - 1, 1));
  };

  // Thêm / xóa nguyên liệu
  const handleAddIngredient = () => {
    setIngredients([...ingredients, { name: "", quantity: null, unit: "" }]);
  };

  const handleRemoveIngredient = (index: number) => {
    if (ingredients.length > 1) {
      setIngredients(ingredients.filter((_, i) => i !== index));
    }
  };

  // Thêm / xóa bước
  const handleAddStep = () => {
    setSteps([
      ...steps,
      { title: `Bước ${steps.length + 1}`, description: "", durationMinutes: 10 },
    ]);
  };

  const handleRemoveStep = (index: number) => {
    if (steps.length > 1) {
      setSteps(steps.filter((_, i) => i !== index));
    }
  };

  // Submit tạo bài (Lưu nháp hoặc Xuất bản)
  const handleSubmit = async (shouldPublish: boolean) => {
    try {
      setIsSubmitting(true);
      setErrorMessage(null);
      setFieldErrors({});

      const filteredIngredients = ingredients.filter((i) => i.name.trim().length > 0);
      const filteredSteps = steps.filter((s) => s.description.trim().length > 0);

      if (shouldPublish && filteredSteps.length === 0) {
        setErrorMessage("Để xuất bản bài viết, bắt buộc phải có ít nhất 1 bước thực hiện.");
        setIsSubmitting(false);
        return;
      }

      const payload: CreateRecipeRequest = {
        title: title.trim(),
        description: description.trim(),
        prepTimeMinutes: Number(prepTimeMinutes),
        cookTimeMinutes: Number(cookTimeMinutes),
        servings: Number(servings),
        difficulty: Number(difficulty),
        nutrition: nutrition.calories ? nutrition : null,
        ingredients: filteredIngredients,
        steps: filteredSteps,
      };

      const result = await createRecipe(payload);

      if (shouldPublish) {
        await publishRecipe(result.id);
      }

      router.push("/dashboard/recipes");
      router.refresh();
    } catch (err: unknown) {
      if (err instanceof ApiError && err.problem) {
        const pd = err.problem;
        if (pd.errors) {
          const mappedErrors: Record<string, string> = {};
          for (const [key, msgs] of Object.entries(pd.errors)) {
            mappedErrors[key.toLowerCase()] = Array.isArray(msgs) ? msgs[0] : String(msgs);
          }
          setFieldErrors(mappedErrors);
        }
        setErrorMessage(pd.detail || pd.title || "Lỗi dữ liệu không hợp lệ (422). Vui lòng kiểm tra lại.");
      } else {
        setErrorMessage((err as Error).message || "Không thể tạo bài viết, vui lòng thử lại.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="mx-auto max-w-3xl space-y-8">
      {/* Step Indicator Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight text-zinc-900 dark:text-zinc-100">
          Tạo công thức mới
        </h1>
        <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
          Hoàn thành các bước dưới đây để chia sẻ món ăn của bạn.
        </p>

        {/* Stepper Bar */}
        <div className="mt-6 grid grid-cols-4 gap-2 border-b border-zinc-200 pb-4 dark:border-zinc-800">
          {[
            { step: 1, label: "1. Thông tin" },
            { step: 2, label: "2. Nguyên liệu" },
            { step: 3, label: "3. Các bước" },
            { step: 4, label: "4. Hoàn tất" },
          ].map((item) => (
            <div
              key={item.step}
              className={`text-center text-xs font-semibold py-1 rounded-md transition-colors ${
                currentStep === item.step
                  ? "bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300"
                  : currentStep > item.step
                  ? "text-zinc-700 dark:text-zinc-300"
                  : "text-zinc-400 dark:text-zinc-600"
              }`}
            >
              {item.label}
            </div>
          ))}
        </div>
      </div>

      {/* Global Error Banner */}
      {errorMessage && (
        <div className="rounded-lg bg-rose-50 p-4 text-sm text-rose-700 ring-1 ring-inset ring-rose-600/20 dark:bg-rose-950/40 dark:text-rose-400">
          {errorMessage}
        </div>
      )}

      {/* BƯỚC 1: THÔNG TIN CHUNG & DINH DƯỠNG */}
      {currentStep === 1 && (
        <div className="space-y-6 rounded-xl border border-zinc-200 bg-white p-6 shadow-xs dark:border-zinc-800 dark:bg-zinc-900">
          <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
            Thông tin cơ bản & Dinh dưỡng
          </h2>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-zinc-700 dark:text-zinc-300">
                Tiêu đề công thức *
              </label>
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Ví dụ: Bún chả Hà Nội nướng than hoa"
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
              />
              {fieldErrors.title && (
                <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{fieldErrors.title}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-zinc-700 dark:text-zinc-300">
                Mô tả món ăn *
              </label>
              <textarea
                rows={3}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Giới thiệu đôi nét về hương vị, nguồn gốc món ăn..."
                className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
              />
              {fieldErrors.description && (
                <p className="mt-1 text-xs text-rose-600 dark:text-rose-400">{fieldErrors.description}</p>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
              <div>
                <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">
                  Chuẩn bị (phút) *
                </label>
                <input
                  type="number"
                  min="1"
                  value={prepTimeMinutes}
                  onChange={(e) => setPrepTimeMinutes(Number(e.target.value))}
                  className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">
                  Nấu (phút)
                </label>
                <input
                  type="number"
                  min="0"
                  value={cookTimeMinutes}
                  onChange={(e) => setCookTimeMinutes(Number(e.target.value))}
                  className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">
                  Khẩu phần (người) *
                </label>
                <input
                  type="number"
                  min="1"
                  value={servings}
                  onChange={(e) => setServings(Number(e.target.value))}
                  className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-zinc-700 dark:text-zinc-300">
                  Độ khó
                </label>
                <select
                  value={difficulty}
                  onChange={(e) => setDifficulty(Number(e.target.value))}
                  className="mt-1 block w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500 dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                >
                  <option value={0}>Dễ</option>
                  <option value={1}>Trung bình</option>
                  <option value={2}>Khó</option>
                </select>
              </div>
            </div>

            {/* Dinh dưỡng tùy chọn */}
            <div className="pt-4 border-t border-zinc-200 dark:border-zinc-800">
              <span className="block text-xs font-semibold text-zinc-500 uppercase tracking-wider mb-3">
                Giá trị dinh dưỡng (Tùy chọn)
              </span>
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                <div>
                  <label className="block text-xs text-zinc-600 dark:text-zinc-400">Calories (kcal)</label>
                  <input
                    type="number"
                    min="0"
                    value={nutrition.calories ?? ""}
                    onChange={(e) =>
                      setNutrition({
                        ...nutrition,
                        calories: e.target.value ? Number(e.target.value) : null,
                      })
                    }
                    className="mt-1 block w-full rounded-lg border border-zinc-300 px-2.5 py-1.5 text-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                  />
                </div>
                <div>
                  <label className="block text-xs text-zinc-600 dark:text-zinc-400">Protein (g)</label>
                  <input
                    type="number"
                    min="0"
                    value={nutrition.proteinGrams ?? ""}
                    onChange={(e) =>
                      setNutrition({
                        ...nutrition,
                        proteinGrams: e.target.value ? Number(e.target.value) : null,
                      })
                    }
                    className="mt-1 block w-full rounded-lg border border-zinc-300 px-2.5 py-1.5 text-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                  />
                </div>
                <div>
                  <label className="block text-xs text-zinc-600 dark:text-zinc-400">Chất béo (g)</label>
                  <input
                    type="number"
                    min="0"
                    value={nutrition.fatGrams ?? ""}
                    onChange={(e) =>
                      setNutrition({
                        ...nutrition,
                        fatGrams: e.target.value ? Number(e.target.value) : null,
                      })
                    }
                    className="mt-1 block w-full rounded-lg border border-zinc-300 px-2.5 py-1.5 text-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                  />
                </div>
                <div>
                  <label className="block text-xs text-zinc-600 dark:text-zinc-400">Carb (g)</label>
                  <input
                    type="number"
                    min="0"
                    value={nutrition.carbsGrams ?? ""}
                    onChange={(e) =>
                      setNutrition({
                        ...nutrition,
                        carbsGrams: e.target.value ? Number(e.target.value) : null,
                      })
                    }
                    className="mt-1 block w-full rounded-lg border border-zinc-300 px-2.5 py-1.5 text-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* BƯỚC 2: NGUYÊN LIỆU */}
      {currentStep === 2 && (
        <div className="space-y-6 rounded-xl border border-zinc-200 bg-white p-6 shadow-xs dark:border-zinc-800 dark:bg-zinc-900">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
              Danh sách nguyên liệu
            </h2>
            <button
              type="button"
              onClick={handleAddIngredient}
              className="text-xs font-semibold text-emerald-600 hover:text-emerald-500 dark:text-emerald-400"
            >
              + Thêm nguyên liệu
            </button>
          </div>

          <div className="space-y-3">
            {ingredients.map((ing, idx) => (
              <div key={idx} className="flex gap-2 items-center">
                <input
                  type="text"
                  placeholder="Tên nguyên liệu (ví dụ: Thịt bò)"
                  value={ing.name}
                  onChange={(e) => {
                    const newIngs = [...ingredients];
                    newIngs[idx].name = e.target.value;
                    setIngredients(newIngs);
                  }}
                  className="flex-1 rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                />
                <input
                  type="number"
                  placeholder="Số lượng"
                  value={ing.quantity ?? ""}
                  onChange={(e) => {
                    const newIngs = [...ingredients];
                    newIngs[idx].quantity = e.target.value ? Number(e.target.value) : null;
                    setIngredients(newIngs);
                  }}
                  className="w-24 rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                />
                <input
                  type="text"
                  placeholder="Đơn vị (g, ml, quả...)"
                  value={ing.unit ?? ""}
                  onChange={(e) => {
                    const newIngs = [...ingredients];
                    newIngs[idx].unit = e.target.value;
                    setIngredients(newIngs);
                  }}
                  className="w-28 rounded-lg border border-zinc-300 px-3 py-2 text-sm shadow-xs dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                />
                {ingredients.length > 1 && (
                  <button
                    type="button"
                    onClick={() => handleRemoveIngredient(idx)}
                    className="p-2 text-rose-500 hover:text-rose-700 text-sm"
                  >
                    ✕
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* BƯỚC 3: CÁC BƯỚC THỰC HIỆN */}
      {currentStep === 3 && (
        <div className="space-y-6 rounded-xl border border-zinc-200 bg-white p-6 shadow-xs dark:border-zinc-800 dark:bg-zinc-900">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
              Các bước thực hiện
            </h2>
            <button
              type="button"
              onClick={handleAddStep}
              className="text-xs font-semibold text-emerald-600 hover:text-emerald-500 dark:text-emerald-400"
            >
              + Thêm bước làm
            </button>
          </div>

          <div className="space-y-5">
            {steps.map((st, idx) => (
              <div
                key={idx}
                className="rounded-lg border border-zinc-200 p-4 space-y-3 dark:border-zinc-800 dark:bg-zinc-800/40"
              >
                <div className="flex items-center justify-between gap-3">
                  <span className="flex h-6 w-6 items-center justify-center rounded-full bg-emerald-100 text-xs font-bold text-emerald-800 dark:bg-emerald-950 dark:text-emerald-400">
                    {idx + 1}
                  </span>
                  <input
                    type="text"
                    placeholder="Tiêu đề bước (ví dụ: Sơ chế)"
                    value={st.title}
                    onChange={(e) => {
                      const newSteps = [...steps];
                      newSteps[idx].title = e.target.value;
                      setSteps(newSteps);
                    }}
                    className="flex-1 rounded-md border border-zinc-300 px-2.5 py-1 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                  />
                  <input
                    type="number"
                    placeholder="Phút"
                    value={st.durationMinutes ?? ""}
                    onChange={(e) => {
                      const newSteps = [...steps];
                      newSteps[idx].durationMinutes = e.target.value ? Number(e.target.value) : null;
                      setSteps(newSteps);
                    }}
                    className="w-20 rounded-md border border-zinc-300 px-2.5 py-1 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                  />
                  {steps.length > 1 && (
                    <button
                      type="button"
                      onClick={() => handleRemoveStep(idx)}
                      className="text-xs text-rose-500 hover:text-rose-700"
                    >
                      Xóa
                    </button>
                  )}
                </div>

                <textarea
                  rows={2}
                  placeholder="Mô tả chi tiết cách thực hiện bước này..."
                  value={st.description}
                  onChange={(e) => {
                    const newSteps = [...steps];
                    newSteps[idx].description = e.target.value;
                    setSteps(newSteps);
                  }}
                  className="w-full rounded-md border border-zinc-300 p-2 text-sm dark:border-zinc-700 dark:bg-zinc-800 dark:text-zinc-100"
                />
              </div>
            ))}
          </div>
        </div>
      )}

      {/* BƯỚC 4: XÁC NHẬN & LƯU BÀI */}
      {currentStep === 4 && (
        <div className="space-y-6 rounded-xl border border-zinc-200 bg-white p-6 shadow-xs dark:border-zinc-800 dark:bg-zinc-900">
          <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
            Xem lại và Lưu công thức
          </h2>

          <div className="rounded-lg bg-zinc-50 p-4 space-y-3 dark:bg-zinc-800/50 text-sm">
            <div>
              <span className="font-semibold text-zinc-700 dark:text-zinc-300">Tên món: </span>
              <span className="text-zinc-900 dark:text-zinc-100 font-bold">{title}</span>
            </div>
            <div>
              <span className="font-semibold text-zinc-700 dark:text-zinc-300">Thời gian: </span>
              <span>Chuẩn bị {prepTimeMinutes} phút, nấu {cookTimeMinutes} phút ({servings} người)</span>
            </div>
            <div>
              <span className="font-semibold text-zinc-700 dark:text-zinc-300">Nguyên liệu: </span>
              <span>{ingredients.filter((i) => i.name.trim()).length} mục</span>
            </div>
            <div>
              <span className="font-semibold text-zinc-700 dark:text-zinc-300">Các bước làm: </span>
              <span>{steps.filter((s) => s.description.trim()).length} bước</span>
            </div>
          </div>

          <div className="rounded-lg border border-emerald-500/30 bg-emerald-50/50 p-4 text-xs text-emerald-800 dark:bg-emerald-950/20 dark:text-emerald-300">
            💡 Lưu ý: Bạn có thể chọn <strong>Lưu nháp</strong> để chỉnh sửa thêm sau này, hoặc <strong>Xuất bản ngay</strong> để hiển thị công khai tới độc giả.
          </div>
        </div>
      )}

      {/* Nút điều hướng Wizard */}
      <div className="flex items-center justify-between border-t border-zinc-200 pt-6 dark:border-zinc-800">
        {currentStep > 1 ? (
          <button
            type="button"
            onClick={prevStep}
            disabled={isSubmitting}
            className="rounded-lg border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-700 hover:bg-zinc-50 dark:border-zinc-700 dark:text-zinc-300 dark:hover:bg-zinc-800"
          >
            ← Quay lại
          </button>
        ) : <div />}

        <div className="flex gap-3">
          {currentStep < 4 ? (
            <button
              type="button"
              onClick={nextStep}
              className="rounded-lg bg-emerald-600 px-5 py-2 text-sm font-semibold text-white shadow-xs hover:bg-emerald-500"
            >
              Tiếp tục →
            </button>
          ) : (
            <>
              <button
                type="button"
                disabled={isSubmitting}
                onClick={() => handleSubmit(false)}
                className="rounded-lg border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-700 hover:bg-zinc-50 dark:border-zinc-700 dark:text-zinc-300 dark:hover:bg-zinc-800"
              >
                {isSubmitting ? "Đang lưu..." : "Lưu dạng bản nháp"}
              </button>
              <button
                type="button"
                disabled={isSubmitting}
                onClick={() => handleSubmit(true)}
                className="rounded-lg bg-emerald-600 px-5 py-2 text-sm font-semibold text-white shadow-xs hover:bg-emerald-500"
              >
                {isSubmitting ? "Đang xuất bản..." : "Xuất bản ngay"}
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
