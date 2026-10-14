import type { Metadata } from "next";
import Link from "next/link";
import { AuthCard } from "@/features/auth/components/AuthCard";
import { FormAlert } from "@/features/auth/components/FormAlert";
import { ResetPasswordForm } from "@/features/auth/components/ResetPasswordForm";
import { FORGOT_PASSWORD_PATH } from "@/features/auth/constants";
import { authErrorMessage } from "@/features/auth/errors";

export const metadata: Metadata = { title: "Đặt lại mật khẩu" };

export default async function ResetPasswordPage({ searchParams }: PageProps<"/auth/reset-password">) {
  const { userId, token } = await searchParams;
  const valid = typeof userId === "string" && typeof token === "string" && userId !== "" && token !== "";

  return (
    <AuthCard
      title="Đặt lại mật khẩu"
      description="Nhập mật khẩu mới cho tài khoản của bạn."
      footer={
        <Link href={FORGOT_PASSWORD_PATH} className="font-medium text-foreground underline">
          Yêu cầu liên kết mới
        </Link>
      }
    >
      {valid ? (
        <ResetPasswordForm userId={userId} token={token} />
      ) : (
        <FormAlert message={authErrorMessage("AUTH_RESET_TOKEN_INVALID")} />
      )}
    </AuthCard>
  );
}
