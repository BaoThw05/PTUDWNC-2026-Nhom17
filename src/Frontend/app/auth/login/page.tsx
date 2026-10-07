import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { auth, googleEnabled } from "@/features/auth/auth";
import { AuthCard } from "@/features/auth/components/AuthCard";
import { LoginForm } from "@/features/auth/components/LoginForm";
import { REGISTER_PATH } from "@/features/auth/constants";
import { authErrorMessage, GOOGLE_SIGN_IN_FAILED_MESSAGE, PASSWORD_CHANGED_MESSAGE } from "@/features/auth/errors";
import { safeCallbackUrl } from "@/features/auth/redirect";

export const metadata: Metadata = { title: "Đăng nhập" };

const EXPIRED_MESSAGE = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";

type SearchParams = Record<string, string | string[] | undefined>;

function initialMessageFor(params: SearchParams): string | null {
  // Form dùng redirect: false nên ?error chỉ xuất hiện khi Auth.js tự chuyển hướng (Google, trình duyệt tắt JS).
  if (params.error === "CredentialsSignin") {
    return authErrorMessage(typeof params.code === "string" ? params.code : null);
  }
  if (params.error) {
    return GOOGLE_SIGN_IN_FAILED_MESSAGE;
  }
  if (params.reason === "password-changed") {
    return PASSWORD_CHANGED_MESSAGE;
  }
  return params.reason === "expired" ? EXPIRED_MESSAGE : null;
}

export default async function LoginPage({ searchParams }: PageProps<"/auth/login">) {
  const params = await searchParams;
  const callbackUrl = safeCallbackUrl(params.callbackUrl);

  const session = await auth();
  if (session && !session.error) {
    redirect(callbackUrl);
  }

  return (
    <AuthCard
      title="Đăng nhập"
      description="Đăng nhập để viết và quản lý công thức của bạn."
      footer={
        <>
          Chưa có tài khoản?{" "}
          <Link
            href={`${REGISTER_PATH}?callbackUrl=${encodeURIComponent(callbackUrl)}`}
            className="font-medium text-foreground underline"
          >
            Đăng ký
          </Link>
        </>
      }
    >
      <LoginForm
        callbackUrl={callbackUrl}
        googleEnabled={googleEnabled}
        initialMessage={initialMessageFor(params)}
        initialTone={params.reason === "password-changed" ? "info" : "error"}
      />
    </AuthCard>
  );
}
