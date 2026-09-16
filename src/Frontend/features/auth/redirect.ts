export const DEFAULT_AFTER_SIGN_IN = "/dashboard";

/** Chỉ cho quay về đường dẫn nội bộ, tránh open redirect qua tham số callbackUrl. */
export function safeCallbackUrl(value: string | string[] | undefined | null): string {
  const url = Array.isArray(value) ? value[0] : value;
  if (!url || !url.startsWith("/") || url.startsWith("//") || url.startsWith("/\\")) {
    return DEFAULT_AFTER_SIGN_IN;
  }
  return url;
}
