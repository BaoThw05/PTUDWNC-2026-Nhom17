"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useForm, useWatch } from "react-hook-form";
import { ApiError } from "@/lib/api/client";
import { resetPassword } from "../api/client";
import { LOGIN_PATH } from "../constants";
import { authErrorMessage } from "../errors";
import { resetPasswordSchema, type ResetPasswordValues } from "../schemas";
import { FormAlert } from "./FormAlert";
import { PasswordRequirements } from "./PasswordRequirements";
import { SubmitButton } from "./SubmitButton";
import { TextField } from "./TextField";

export function ResetPasswordForm({ userId, token }: { userId: string; token: string }) {
  const router = useRouter();
  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors },
  } = useForm<ResetPasswordValues>({ resolver: zodResolver(resetPasswordSchema) });
  const newPassword = useWatch({ control, name: "newPassword" });

  const mutation = useMutation({
    mutationFn: resetPassword,
    onSuccess: () => router.replace(`${LOGIN_PATH}?reason=password-reset`),
    onError: (error) => {
      const fieldError = error instanceof ApiError ? (error.problem.errors?.NewPassword ?? error.problem.errors?.Password) : undefined;
      if (fieldError?.[0]) {
        setError("newPassword", { message: fieldError[0] });
      }
    },
  });

  const error = mutation.error;
  const formError =
    error instanceof ApiError
      ? error.problem.errors?.NewPassword || error.problem.errors?.Password
        ? null
        : authErrorMessage(error.code)
      : error
        ? authErrorMessage("SERVICE_UNAVAILABLE")
        : null;

  return (
    <form
      onSubmit={handleSubmit(({ newPassword }) => mutation.mutate({ userId, token, newPassword }))}
      noValidate
      className="flex flex-col gap-4"
    >
      <FormAlert message={formError} />
      <TextField
        label="Mật khẩu mới"
        type="password"
        autoComplete="new-password"
        error={errors.newPassword?.message}
        {...register("newPassword")}
      />
      <PasswordRequirements value={newPassword ?? ""} />
      <TextField
        label="Nhập lại mật khẩu mới"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register("confirmPassword")}
      />
      <SubmitButton pending={mutation.isPending || mutation.isSuccess} pendingLabel="Đang đặt lại mật khẩu…">
        Đặt lại mật khẩu
      </SubmitButton>
    </form>
  );
}
