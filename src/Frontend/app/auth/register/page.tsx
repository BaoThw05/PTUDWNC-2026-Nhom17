import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { auth, googleEnabled } from "@/features/auth/auth";
import { AuthCard } from "@/features/auth/components/AuthCard";
import { GoogleSignInButton } from "@/features/auth/components/GoogleSignInButton";
import { RegisterForm } from "@/features/auth/components/RegisterForm";
import { LOGIN_PATH } from "@/features/auth/constants";
import { safeCallbackUrl } from "@/features/auth/redirect";

export const metadata: Metadata = { title: "Đăng ký" };

export default async function RegisterPage({ searchParams }: PageProps<"/auth/register">) {
  const callbackUrl = safeCallbackUrl((await searchParams).callbackUrl);

  const session = await auth();
  if (session && !session.error) {
    redirect(callbackUrl);
  }

  return (
    <AuthCard
      title="Tạo tài khoản"
      description="Chia sẻ công thức nấu ăn của bạn với mọi người."
      footer={
        <>
          Đã có tài khoản?{" "}
          <Link
            href={`${LOGIN_PATH}?callbackUrl=${encodeURIComponent(callbackUrl)}`}
            className="font-medium text-foreground underline"
          >
            Đăng nhập
          </Link>
        </>
      }
    >
      <RegisterForm callbackUrl={callbackUrl} />
      {googleEnabled && <GoogleSignInButton callbackUrl={callbackUrl} />}
    </AuthCard>
  );
}
