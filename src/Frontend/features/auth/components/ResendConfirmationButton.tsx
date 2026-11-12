"use client";

import { useMutation } from "@tanstack/react-query";
import { ApiError } from "@/lib/api/client";
import { resendEmailConfirmation } from "../api/client";
import { authErrorMessage } from "../errors";
import { FormAlert } from "./FormAlert";

export function ResendConfirmationButton() {
  const mutation = useMutation({ mutationFn: resendEmailConfirmation });
  const error =
    mutation.error instanceof ApiError
      ? authErrorMessage(mutation.error.code)
      : mutation.error
        ? authErrorMessage("SERVICE_UNAVAILABLE")
        : null;

  return (
    <div className="flex flex-col gap-2">
      <FormAlert message={error} />
      <FormAlert message={mutation.isSuccess ? "Đã gửi lại email xác nhận. Vui lòng kiểm tra hộp thư." : null} tone="info" />
      <button
        type="button"
        onClick={() => mutation.mutate()}
        disabled={mutation.isPending || mutation.isSuccess}
        className="self-start text-sm font-medium underline disabled:opacity-60"
      >
        {mutation.isPending ? "Đang gửi…" : "Gửi lại email xác nhận"}
      </button>
    </div>
  );
}
