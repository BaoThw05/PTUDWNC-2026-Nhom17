import type { Metadata } from "next";
import Link from "next/link";
import { AuthCard } from "@/features/auth/components/AuthCard";
import { ForgotPasswordForm } from "@/features/auth/components/ForgotPasswordForm";
import { LOGIN_PATH } from "@/features/auth/constants";

export const metadata: Metadata = { title: "Quên mật khẩu" };

export default function ForgotPasswordPage() {
  return (
    <AuthCard
      title="Quên mật khẩu"
      description="Nhập email đã đăng ký, chúng tôi sẽ gửi liên kết để đặt lại mật khẩu."
      footer={
        <Link href={LOGIN_PATH} className="font-medium text-foreground underline">
          Quay lại đăng nhập
        </Link>
      }
    >
      <ForgotPasswordForm />
    </AuthCard>
  );
}
