import { getAccessToken } from "./access-token";
import { ApiError, toProblemDetails } from "./problem-details";

export { ApiError } from "./problem-details";
export type { ProblemDetails } from "./problem-details";

const DEFAULT_API_INTERNAL_URL = "http://localhost:5000";

type JsonBody = Record<string, unknown> | unknown[];

export type ApiRequestOptions = Omit<RequestInit, "body" | "method"> & {
  body?: JsonBody | FormData;
  /**
   * Token gắn vào header Authorization. Bỏ trống: trình duyệt tự lấy từ phiên đăng nhập.
   * Truyền `null` để gửi request không kèm token.
   */
  accessToken?: string | null;
};

// Server gọi thẳng backend; trình duyệt đi qua rewrite /api/* của Next.js.
function resolveBaseUrl(): string {
  if (typeof window === "undefined") {
    return process.env.API_INTERNAL_URL ?? DEFAULT_API_INTERNAL_URL;
  }
  return process.env.NEXT_PUBLIC_API_BASE_URL ?? "";
}

function isJson(response: Response): boolean {
  const contentType = response.headers.get("content-type") ?? "";
  return contentType.includes("json");
}

async function request<T>(
  method: string,
  path: string,
  { body, headers, accessToken, ...init }: ApiRequestOptions = {},
): Promise<T> {
  const requestHeaders = new Headers(headers);
  requestHeaders.set("Accept", "application/json, application/problem+json");

  const token = accessToken === undefined ? await getAccessToken() : accessToken;
  if (token) {
    requestHeaders.set("Authorization", `Bearer ${token}`);
  }

  let requestBody: BodyInit | undefined;
  if (body instanceof FormData) {
    requestBody = body;
  } else if (body !== undefined) {
    requestHeaders.set("Content-Type", "application/json");
    requestBody = JSON.stringify(body);
  }

  const response = await fetch(`${resolveBaseUrl()}${path}`, {
    ...init,
    method,
    headers: requestHeaders,
    body: requestBody,
  });

  if (!response.ok) {
    const payload: unknown = isJson(response)
      ? await response.json().catch(() => undefined)
      : undefined;
    throw new ApiError(response.status, toProblemDetails(payload));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (isJson(response) ? await response.json() : await response.text()) as T;
}

/** Điểm duy nhất để gọi backend. Đường dẫn giữ nguyên như backend, ví dụ `/api/v1/recipes`. */
export const apiClient = {
  get: <T>(path: string, options?: ApiRequestOptions) =>
    request<T>("GET", path, options),
  post: <T>(path: string, options?: ApiRequestOptions) =>
    request<T>("POST", path, options),
  put: <T>(path: string, options?: ApiRequestOptions) =>
    request<T>("PUT", path, options),
  patch: <T>(path: string, options?: ApiRequestOptions) =>
    request<T>("PATCH", path, options),
  delete: <T>(path: string, options?: ApiRequestOptions) =>
    request<T>("DELETE", path, options),
};
