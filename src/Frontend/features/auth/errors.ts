/** Thông báo hiển thị theo mã lỗi của backend (docs/api/error-codes.md). */
const MESSAGES: Record<string, string> = {
  AUTH_INVALID_CREDENTIALS: "Email hoặc mật khẩu không đúng.",
  AUTH_ACCOUNT_LOCKED:
    "Tài khoản tạm bị khóa do nhập sai mật khẩu nhiều lần. Vui lòng thử lại sau 15 phút.",
  AUTH_ACCOUNT_DISABLED: "Tài khoản đã bị vô hiệu hóa.",
  AUTH_EMAIL_EXISTS: "Email này đã được đăng ký.",
  AUTH_GOOGLE_UNAVAILABLE: "Đăng nhập bằng Google đang tạm thời không dùng được.",
  AUTH_GOOGLE_EMAIL_UNVERIFIED: "Email của tài khoản Google chưa được xác minh.",
  TOO_MANY_REQUESTS: "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.",
  VALIDATION_ERROR: "Thông tin nhập chưa hợp lệ.",
  SERVICE_UNAVAILABLE: "Không kết nối được máy chủ. Vui lòng thử lại sau.",
};

const FALLBACK_MESSAGE = "Đã có lỗi xảy ra. Vui lòng thử lại.";

export function authErrorMessage(code: string | null | undefined): string {
  return (code && MESSAGES[code]) || FALLBACK_MESSAGE;
}

/** Mã lỗi Auth.js đặt trên URL khi đăng nhập Google thất bại. */
export const PASSWORD_CHANGED_MESSAGE = "Đã đổi mật khẩu. Vui lòng đăng nhập lại bằng mật khẩu mới.";

export const GOOGLE_SIGN_IN_FAILED_MESSAGE = "Đăng nhập bằng Google thất bại. Vui lòng thử lại.";
