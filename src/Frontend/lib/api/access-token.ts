/**
 * Trình duyệt không giữ access token: request /api/v1/* đi qua Route Handler `app/api/v1/[...path]`,
 * nơi server tự gắn token từ cookie phiên. Code server lấy token bằng `getServerAccessToken()` rồi truyền vào `apiClient`.
 */
export async function getAccessToken(): Promise<string | null> {
  return null;
}
