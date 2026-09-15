import NextAuth, { type Session } from "next-auth";
import Credentials from "next-auth/providers/credentials";
import Google from "next-auth/providers/google";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5165";

interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    fullName: string;
    email: string;
    userName: string;
    avatarUrl: string | null;
    roles: string[];
  };
}

async function refreshAccessToken(refreshToken: string) {
  const res = await fetch(`${API_URL}/api/v1/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  });

  if (!res.ok) {
    throw new Error("Failed to refresh access token");
  }

  return (await res.json()) as AuthResponseDto;
}

// FR-AUTH-003: đổi Google ID token lấy access/refresh token của hệ thống
// mình (KHÔNG dùng thẳng session Google) — backend tự verify chữ ký/issuer/
// audience/hạn dùng qua Google.Apis.Auth trước khi tạo/liên kết tài khoản.
async function loginWithGoogle(idToken: string) {
  const res = await fetch(`${API_URL}/api/v1/auth/google`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ idToken }),
  });

  if (!res.ok) {
    throw new Error("Google login rejected by backend");
  }

  return (await res.json()) as AuthResponseDto;
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  session: { strategy: "jwt" },
  pages: {
    signIn: "/auth/login",
  },
  providers: [
    Google({
      clientId: process.env.AUTH_GOOGLE_ID,
      clientSecret: process.env.AUTH_GOOGLE_SECRET,
      authorization: { params: { scope: "openid email profile" } },
    }),
    Credentials({
      name: "Credentials",
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        const res = await fetch(`${API_URL}/api/v1/auth/login`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            email: credentials?.email,
            password: credentials?.password,
          }),
        });

        if (!res.ok) {
          return null;
        }

        const data = (await res.json()) as AuthResponseDto;

        return {
          id: data.user.id,
          fullName: data.user.fullName,
          email: data.user.email,
          userName: data.user.userName,
          avatarUrl: data.user.avatarUrl,
          roles: data.user.roles,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          expiresAt: new Date(data.expiresAt).getTime(),
        };
      },
    }),
  ],
  callbacks: {
    async jwt({ token, user, account }) {
      // Đăng nhập Google lần đầu: account.id_token là ID token Google vừa cấp.
      if (account?.provider === "google") {
        if (!account.id_token) {
          token.error = "GoogleLoginError";
          return token;
        }

        try {
          const data = await loginWithGoogle(account.id_token);
          token.accessToken = data.accessToken;
          token.refreshToken = data.refreshToken;
          token.expiresAt = new Date(data.expiresAt).getTime();
          token.user = data.user;
          token.error = undefined;
        } catch {
          token.error = "GoogleLoginError";
        }

        return token;
      }

      // Đăng nhập Credentials lần đầu: user chỉ có mặt ngay sau authorize() thành công.
      if (user) {
        token.accessToken = user.accessToken;
        token.refreshToken = user.refreshToken;
        token.expiresAt = user.expiresAt;
        token.user = {
          id: user.id,
          fullName: user.fullName,
          email: user.email,
          userName: user.userName,
          avatarUrl: user.avatarUrl,
          roles: user.roles,
        };
        return token;
      }

      // Access token còn hạn (trừ hao 30s) -> dùng tiếp, không gọi refresh.
      const expiresAt = token.expiresAt as number | undefined;
      if (expiresAt && Date.now() < expiresAt - 30_000) {
        return token;
      }

      // Hết hạn -> Refresh Token Rotation (FR-AUTH-004).
      try {
        const currentRefreshToken = token.refreshToken as string | undefined;
        if (!currentRefreshToken) {
          throw new Error("Missing refresh token");
        }

        const refreshed = await refreshAccessToken(currentRefreshToken);

        token.accessToken = refreshed.accessToken;
        token.refreshToken = refreshed.refreshToken;
        token.expiresAt = new Date(refreshed.expiresAt).getTime();
        token.error = undefined;
      } catch {
        token.error = "RefreshAccessTokenError";
      }

      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken as string | undefined;
      session.error = token.error as "RefreshAccessTokenError" | "GoogleLoginError" | undefined;
      const tokenUser = token.user as Session["user"] | undefined;
      if (tokenUser) {
        session.user = { ...session.user, ...tokenUser };
      }
      return session;
    },
  },
  events: {
    // FR-AUTH-005: thu hồi refresh token phía backend khi đăng xuất. Chạy
    // server-side (bên trong route handler NextAuth) nên refreshToken không
    // bao giờ phải gửi xuống client.
    async signOut(message) {
      const token = "token" in message ? message.token : undefined;
      if (!token?.accessToken || !token.refreshToken) {
        return;
      }

      await fetch(`${API_URL}/api/v1/auth/logout`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token.accessToken}`,
        },
        body: JSON.stringify({ refreshToken: token.refreshToken }),
      }).catch(() => {
        // Best-effort: refresh token sẽ tự hết hạn sau 7 ngày nếu revoke lỗi.
      });
    },
  },
});
