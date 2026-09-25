/** SessionProvider hỏi lại phiên định kỳ để token được làm mới và cookie được ghi lại trước khi access token (15 phút) hết hạn. */
export const SESSION_REFETCH_SECONDS = 4 * 60;

export const LOGIN_PATH = "/auth/login";
export const REGISTER_PATH = "/auth/register";
export const PROFILE_PATH = "/profile";
