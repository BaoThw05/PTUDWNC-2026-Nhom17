/**
 * Trả về access token của người dùng hiện tại để gắn vào header Authorization.
 * Khung gốc chưa có đăng nhập nên luôn trả về null.
 */
export async function getAccessToken(): Promise<string | null> {
  // TODO(TV1): lấy accessToken từ phiên Auth.js (việc 1.09).
  return null;
}
