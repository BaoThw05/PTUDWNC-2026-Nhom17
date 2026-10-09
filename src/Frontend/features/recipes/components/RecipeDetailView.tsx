import type { RecipeSummary } from "../types";

export function RecipeDetailView({ recipe }: { recipe: RecipeSummary }) {
  const totalTime = recipe.prepTimeMinutes + recipe.cookTimeMinutes;

  return (
    <article className="space-y-10">
      {/* Header Info */}
      <header className="space-y-4 border-b border-zinc-200 pb-8 dark:border-zinc-800">
        <h1 className="text-3xl font-bold tracking-tight text-zinc-900 sm:text-4xl dark:text-zinc-100">
          {recipe.title}
        </h1>
        {recipe.description && (
          <p className="text-lg text-zinc-600 dark:text-zinc-400">
            {recipe.description}
          </p>
        )}

        {/* Quick Stats Grid */}
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4 pt-4">
          <div className="rounded-xl border border-zinc-200 bg-zinc-50/50 p-4 dark:border-zinc-800 dark:bg-zinc-900/50">
            <span className="block text-xs font-medium text-zinc-500 uppercase tracking-wider">
              Chuẩn bị
            </span>
            <span className="mt-1 text-lg font-semibold text-zinc-900 dark:text-zinc-100">
              {recipe.prepTimeMinutes} phút
            </span>
          </div>

          <div className="rounded-xl border border-zinc-200 bg-zinc-50/50 p-4 dark:border-zinc-800 dark:bg-zinc-900/50">
            <span className="block text-xs font-medium text-zinc-500 uppercase tracking-wider">
              Nấu nướng
            </span>
            <span className="mt-1 text-lg font-semibold text-zinc-900 dark:text-zinc-100">
              {recipe.cookTimeMinutes} phút
            </span>
          </div>

          <div className="rounded-xl border border-zinc-200 bg-zinc-50/50 p-4 dark:border-zinc-800 dark:bg-zinc-900/50">
            <span className="block text-xs font-medium text-zinc-500 uppercase tracking-wider">
              Tổng thời gian
            </span>
            <span className="mt-1 text-lg font-semibold text-zinc-900 dark:text-zinc-100">
              {totalTime} phút
            </span>
          </div>

          <div className="rounded-xl border border-zinc-200 bg-zinc-50/50 p-4 dark:border-zinc-800 dark:bg-zinc-900/50">
            <span className="block text-xs font-medium text-zinc-500 uppercase tracking-wider">
              Khẩu phần
            </span>
            <span className="mt-1 text-lg font-semibold text-zinc-900 dark:text-zinc-100">
              {recipe.servings} người
            </span>
          </div>
        </div>
      </header>

      {/* Main Grid: Ingredients + Nutrition */}
      <div className="grid gap-10 md:grid-cols-3">
        {/* Nguyên liệu (2 cột) */}
        <div className="md:col-span-2 space-y-6">
          <h2 className="text-xl font-bold text-zinc-900 dark:text-zinc-100">
            Nguyên liệu cần chuẩn bị
          </h2>
          {recipe.ingredients.length === 0 ? (
            <p className="text-sm text-zinc-500 italic">Không có danh sách nguyên liệu cụ thể.</p>
          ) : (
            <ul className="divide-y divide-zinc-200 rounded-xl border border-zinc-200 bg-white p-4 shadow-2xs dark:divide-zinc-800 dark:border-zinc-800 dark:bg-zinc-900">
              {recipe.ingredients.map((ing) => (
                <li key={ing.id} className="flex items-center justify-between py-2.5 px-2 text-sm">
                  <span className="font-medium text-zinc-900 dark:text-zinc-100">
                    {ing.name}
                  </span>
                  <span className="text-zinc-600 dark:text-zinc-400">
                    {ing.quantity !== null && ing.quantity !== undefined
                      ? `${ing.quantity} ${ing.unit || ""}`.trim()
                      : ing.unit || "Tùy ý"}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>

        {/* Dinh dưỡng (1 cột) */}
        <div className="space-y-6">
          <h2 className="text-xl font-bold text-zinc-900 dark:text-zinc-100">
            Giá trị dinh dưỡng
          </h2>
          {recipe.nutrition ? (
            <div className="rounded-xl border border-zinc-200 bg-zinc-50/50 p-5 space-y-3 dark:border-zinc-800 dark:bg-zinc-900/50">
              {recipe.nutrition.calories !== null && recipe.nutrition.calories !== undefined && (
                <div className="flex justify-between text-sm border-b border-zinc-200 pb-2 dark:border-zinc-800">
                  <span className="text-zinc-600 dark:text-zinc-400">Năng lượng</span>
                  <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                    {recipe.nutrition.calories} kcal
                  </span>
                </div>
              )}
              {recipe.nutrition.proteinGrams !== null && recipe.nutrition.proteinGrams !== undefined && (
                <div className="flex justify-between text-sm border-b border-zinc-200 pb-2 dark:border-zinc-800">
                  <span className="text-zinc-600 dark:text-zinc-400">Chất đạm (Protein)</span>
                  <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                    {recipe.nutrition.proteinGrams} g
                  </span>
                </div>
              )}
              {recipe.nutrition.fatGrams !== null && recipe.nutrition.fatGrams !== undefined && (
                <div className="flex justify-between text-sm border-b border-zinc-200 pb-2 dark:border-zinc-800">
                  <span className="text-zinc-600 dark:text-zinc-400">Chất béo (Fat)</span>
                  <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                    {recipe.nutrition.fatGrams} g
                  </span>
                </div>
              )}
              {recipe.nutrition.carbsGrams !== null && recipe.nutrition.carbsGrams !== undefined && (
                <div className="flex justify-between text-sm border-b border-zinc-200 pb-2 dark:border-zinc-800">
                  <span className="text-zinc-600 dark:text-zinc-400">Carbohydrate</span>
                  <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                    {recipe.nutrition.carbsGrams} g
                  </span>
                </div>
              )}
              {recipe.nutrition.fiberGrams !== null && recipe.nutrition.fiberGrams !== undefined && (
                <div className="flex justify-between text-sm border-b border-zinc-200 pb-2 dark:border-zinc-800">
                  <span className="text-zinc-600 dark:text-zinc-400">Chất xơ</span>
                  <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                    {recipe.nutrition.fiberGrams} g
                  </span>
                </div>
              )}
              {recipe.nutrition.sugarGrams !== null && recipe.nutrition.sugarGrams !== undefined && (
                <div className="flex justify-between text-sm">
                  <span className="text-zinc-600 dark:text-zinc-400">Đường</span>
                  <span className="font-semibold text-zinc-900 dark:text-zinc-100">
                    {recipe.nutrition.sugarGrams} g
                  </span>
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm text-zinc-500 italic">Chưa có thông tin dinh dưỡng.</p>
          )}
        </div>
      </div>

      {/* Các bước thực hiện */}
      <div className="space-y-6 pt-6 border-t border-zinc-200 dark:border-zinc-800">
        <h2 className="text-2xl font-bold text-zinc-900 dark:text-zinc-100">
          Các bước thực hiện
        </h2>
        <div className="space-y-6">
          {recipe.steps.map((step) => (
            <div
              key={step.id}
              className="flex gap-4 rounded-xl border border-zinc-200 bg-white p-5 shadow-2xs dark:border-zinc-800 dark:bg-zinc-900"
            >
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-emerald-100 text-sm font-bold text-emerald-800 dark:bg-emerald-950 dark:text-emerald-400">
                {step.stepNumber}
              </div>
              <div className="space-y-1.5 flex-1">
                <div className="flex items-center justify-between">
                  <h3 className="text-base font-semibold text-zinc-900 dark:text-zinc-100">
                    {step.title}
                  </h3>
                  {step.durationMinutes && (
                    <span className="text-xs text-zinc-500 dark:text-zinc-400">
                      ⏱ {step.durationMinutes} phút
                    </span>
                  )}
                </div>
                <p className="text-sm leading-relaxed text-zinc-600 dark:text-zinc-300 whitespace-pre-line">
                  {step.description}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </article>
  );
}
