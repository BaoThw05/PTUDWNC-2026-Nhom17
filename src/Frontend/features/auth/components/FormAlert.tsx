export function FormAlert({ message, tone = "error" }: { message: string | null; tone?: "error" | "info" }) {
  if (!message) {
    return null;
  }

  const toneClass =
    tone === "error"
      ? "border-red-500/40 bg-red-50 text-red-800 dark:bg-red-950/40 dark:text-red-200"
      : "border-emerald-500/40 bg-emerald-50 text-emerald-800 dark:bg-emerald-950/40 dark:text-emerald-200";

  return (
    <p role={tone === "error" ? "alert" : "status"} className={`rounded-lg border px-3 py-2 text-sm ${toneClass}`}>
      {message}
    </p>
  );
}
