import type { Metadata } from "next";
import { AuthCard } from "@/features/auth/components/AuthCard";
import { ConfirmEmailForm } from "@/features/auth/components/ConfirmEmailForm";
import { FormAlert } from "@/features/auth/components/FormAlert";
import { authErrorMessage } from "@/features/auth/errors";

// URL chứa token xác nhận: không gửi kèm trong Referer khi rời trang.
export const metadata: Metadata = { title: "Xác nhận email", referrer: "no-referrer" };

export default async function ConfirmEmailPage({ searchParams }: PageProps<"/auth/confirm-email">) {
  const { userId, token } = await searchParams;
  const valid = typeof userId === "string" && typeof token === "string" && userId !== "" && token !== "";

  return (
    <AuthCard title="Xác nhận email" description="Bấm nút bên dưới để xác nhận địa chỉ email của tài khoản.">
      {valid ? (
        <ConfirmEmailForm userId={userId} token={token} />
      ) : (
        <FormAlert message={authErrorMessage("AUTH_EMAIL_TOKEN_INVALID")} />
      )}
    </AuthCard>
  );
}
