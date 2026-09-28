import type { DefaultSession } from "next-auth";
import type { AuthResponse, SessionError, SessionUser } from "./types";

declare module "next-auth" {
  interface Session {
    accessToken?: string;
    error?: SessionError;
    user: SessionUser & DefaultSession["user"];
  }

  interface User {
    /** Kết quả đăng nhập từ backend, chỉ có ngay sau khi authorize() thành công. */
    backendAuth?: AuthResponse;
  }
}

declare module "@auth/core/jwt" {
  interface JWT {
    accessToken?: string;
    accessTokenExpiresAt?: number;
    refreshToken?: string;
    user?: SessionUser;
    error?: SessionError;
  }
}
