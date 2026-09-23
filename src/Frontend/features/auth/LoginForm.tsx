"use client";

import Link from "next/link";
import { useState, type FormEvent } from "react";

/**
 * Giao diện đăng nhập (FR-AUTH-002, FR-AUTH-003).
 * Hiện tại CHỈ LÀ GIAO DIỆN — chưa gọi API thật.
 * TODO(TV1): nối vào Auth.js Credentials provider / signIn("credentials", ...).
 * TODO(TV1): nối nút Google vào signIn("google", ...) khi làm FR-AUTH-003.
 * Mã lỗi cần map khi có API thật: AUTH_INVALID_CREDENTIALS (401),
 * AUTH_ACCOUNT_LOCKED (423), AUTH_ACCOUNT_DISABLED (403), VALIDATION_ERROR (422).
 */
export function LoginForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [remember, setRemember] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const emailError =
    email.length > 0 && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
      ? "Email không đúng định dạng"
      : null;
  const passwordError =
    password.length > 0 && password.length < 8
      ? "Mật khẩu phải có ít nhất 8 ký tự"
      : null;

  function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMessage(null);

    if (!email || !password || emailError || passwordError) {
      setErrorMessage("Vui lòng kiểm tra lại thông tin đã nhập.");
      return;
    }

    // TODO(TV1): thay đoạn giả lập này bằng gọi signIn("credentials", { email, password })
    setIsSubmitting(true);
    setTimeout(() => {
      setIsSubmitting(false);
      setErrorMessage("Email hoặc mật khẩu không đúng.");
    }, 900);
  }

  return (
    <div className="mx-auto flex w-full max-w-sm flex-col gap-6">
      <div className="flex flex-col gap-1 text-center">
        <h1 className="text-2xl font-semibold tracking-tight">Đăng nhập</h1>
        <p className="text-sm text-foreground/60">
          Chào mừng quay lại Culinary Blog
        </p>
      </div>

      <div className="rounded-xl border border-foreground/10 bg-foreground/[0.02] p-6 shadow-sm">
        {errorMessage ? (
          <div
            role="alert"
            className="mb-4 rounded-lg border border-red-500/30 bg-red-500/10 px-3 py-2 text-sm text-red-600 dark:text-red-400"
          >
            {errorMessage}
          </div>
        ) : null}

        <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <label htmlFor="email" className="text-sm font-medium">
              Email
            </label>
            <input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              placeholder="ban@vidu.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              aria-invalid={Boolean(emailError)}
              className="rounded-lg border border-foreground/15 bg-background px-3 py-2 text-sm outline-none transition focus:border-foreground/40 focus:ring-2 focus:ring-foreground/10 aria-[invalid=true]:border-red-500/60"
            />
            {emailError ? (
              <p className="text-xs text-red-500">{emailError}</p>
            ) : null}
          </div>

          <div className="flex flex-col gap-1.5">
            <div className="flex items-center justify-between">
              <label htmlFor="password" className="text-sm font-medium">
                Mật khẩu
              </label>
              <Link
                href="#"
                className="text-xs font-medium text-foreground/60 hover:text-foreground hover:underline"
              >
                Quên mật khẩu?
              </Link>
            </div>
            <div className="relative">
              <input
                id="password"
                name="password"
                type={showPassword ? "text" : "password"}
                autoComplete="current-password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                aria-invalid={Boolean(passwordError)}
                className="w-full rounded-lg border border-foreground/15 bg-background px-3 py-2 pr-16 text-sm outline-none transition focus:border-foreground/40 focus:ring-2 focus:ring-foreground/10 aria-[invalid=true]:border-red-500/60"
              />
              <button
                type="button"
                onClick={() => setShowPassword((v) => !v)}
                className="absolute inset-y-0 right-0 px-3 text-xs font-medium text-foreground/60 hover:text-foreground"
              >
                {showPassword ? "Ẩn" : "Hiện"}
              </button>
            </div>
            {passwordError ? (
              <p className="text-xs text-red-500">{passwordError}</p>
            ) : null}
          </div>

          <label className="flex items-center gap-2 text-sm text-foreground/70">
            <input
              type="checkbox"
              checked={remember}
              onChange={(e) => setRemember(e.target.checked)}
              className="h-4 w-4 rounded border-foreground/30"
            />
            Ghi nhớ đăng nhập
          </label>

          <button
            type="submit"
            disabled={isSubmitting}
            className="mt-1 flex h-10 items-center justify-center rounded-lg bg-foreground text-sm font-medium text-background transition hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? "Đang đăng nhập..." : "Đăng nhập"}
          </button>
        </form>

        <div className="my-5 flex items-center gap-3">
          <div className="h-px flex-1 bg-foreground/10" />
          <span className="text-xs text-foreground/50">hoặc</span>
          <div className="h-px flex-1 bg-foreground/10" />
        </div>

        <button
          type="button"
          className="flex h-10 w-full items-center justify-center gap-2 rounded-lg border border-foreground/15 text-sm font-medium transition hover:bg-foreground/5"
        >
          <GoogleIcon />
          Đăng nhập với Google
        </button>
      </div>

      <p className="text-center text-sm text-foreground/60">
        Chưa có tài khoản?{" "}
        <Link href="/auth/register" className="font-medium text-foreground hover:underline">
          Đăng ký ngay
        </Link>
      </p>
    </div>
  );
}

function GoogleIcon() {
  return (
    <svg viewBox="0 0 48 48" className="h-4 w-4" aria-hidden="true">
      <path
        fill="#FFC107"
        d="M43.6 20.5H42V20H24v8h11.3c-1.6 4.7-6.1 8-11.3 8-6.6 0-12-5.4-12-12s5.4-12 12-12c3.1 0 5.9 1.1 8 3l5.7-5.7C34.6 6.1 29.6 4 24 4 12.9 4 4 12.9 4 24s8.9 20 20 20 20-8.9 20-20c0-1.3-.1-2.7-.4-3.5z"
      />
      <path
        fill="#FF3D00"
        d="M6.3 14.7l6.6 4.8C14.5 15.9 18.9 13 24 13c3.1 0 5.9 1.1 8 3l5.7-5.7C34.6 6.1 29.6 4 24 4c-7.4 0-13.8 4.2-17 10.3z"
      />
      <path
        fill="#4CAF50"
        d="M24 44c5.5 0 10.4-1.9 14.3-5.1l-6.6-5.6C29.6 34.9 26.9 36 24 36c-5.2 0-9.6-3.3-11.3-7.9l-6.6 5.1C9.9 39.6 16.4 44 24 44z"
      />
      <path
        fill="#1976D2"
        d="M43.6 20.5H42V20H24v8h11.3c-.8 2.3-2.2 4.2-4.1 5.6l6.6 5.6C40.9 36.7 44 30.9 44 24c0-1.3-.1-2.7-.4-3.5z"
      />
    </svg>
  );
}