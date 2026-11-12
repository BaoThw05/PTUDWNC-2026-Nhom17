"use client";

import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { ApiError } from "@/lib/api/client";
import { confirmEmail } from "../api/client";
import { PROFILE_PATH } from "../constants";
import { authErrorMessage } from "../errors";
import { FormAlert } from "./FormAlert";
import { SubmitButton } from "./SubmitButton";

// Cần người dùng bấm nút (không tự gửi khi mở trang) để trình quét link trong email không xác nhận thay người dùng.
export function ConfirmEmailForm({ userId, token }: { userId: string; token: string }) {
  const mutation = useMutation({ mutationFn: confirmEmail });

  if (mutation.isSuccess) {
    return (
      <div className="flex flex-col gap-4">
        <FormAlert message="Email của bạn đã được xác nhận." tone="info" />
        <Link href={PROFILE_PATH} className="font-medium underline">
          Về trang hồ sơ
        </Link>
      </div>
    );
  }

  const error =
    mutation.error instanceof ApiError
      ? authErrorMessage(mutation.error.status === 404 ? "AUTH_EMAIL_TOKEN_INVALID" : mutation.error.code)
      : mutation.error
        ? authErrorMessage("SERVICE_UNAVAILABLE")
        : null;

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        mutation.mutate({ userId, token });
      }}
      className="flex flex-col gap-4"
    >
      <FormAlert message={error} />
      <SubmitButton pending={mutation.isPending} pendingLabel="Đang xác nhận…">
        Xác nhận email
      </SubmitButton>
    </form>
  );
}
