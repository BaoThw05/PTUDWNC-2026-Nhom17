export function FormAlert({ message, tone = "error" }: { message: string | null; tone?: "error" | "info" }) {
  if (!message) {
    return null;
  }

  const toneClass =
    tone === "error"
      ? "border-(color:--auth-danger-border) bg-(color:--auth-danger-bg) text-(color:--auth-danger-strong)"
      : "border-(color:--auth-success-border) bg-(color:--auth-success-bg) text-(color:--auth-success-strong)";

  return (
    <p role={tone === "error" ? "alert" : "status"} className={`rounded-lg border px-3 py-2 text-sm ${toneClass}`}>
      {message}
    </p>
  );
}
