"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { signOut } from "next-auth/react";
import { useForm, useWatch } from "react-hook-form";
import { ApiError } from "@/lib/api/client";
import { changePassword } from "../api/client";
import { LOGIN_PATH } from "../constants";
import { authErrorMessage } from "../errors";
import { changePasswordSchema, type ChangePasswordValues } from "../schemas";
import { FormAlert } from "./FormAlert";
import { PasswordRequirements } from "./PasswordRequirements";
import { SubmitButton } from "./SubmitButton";
import { TextField } from "./TextField";

export function ChangePasswordForm() {
  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors },
  } = useForm<ChangePasswordValues>({ resolver: zodResolver(changePasswordSchema) });
  const newPassword = useWatch({ control, name: "newPassword" });

  const mutation = useMutation({
    mutationFn: changePassword,
    // Backend đã thu hồi mọi refresh token nên phiên hiện tại không còn dùng được.
    onSuccess: () => signOut({ redirectTo: `${LOGIN_PATH}?reason=password-changed` }),
    onError: (error) => {
      const fieldErrors = error instanceof ApiError ? error.problem.errors : undefined;
      if (fieldErrors?.CurrentPassword?.[0]) {
        setError("currentPassword", { message: "Mật khẩu hiện tại không đúng." });
      }
      if (fieldErrors?.NewPassword?.[0]) {
        setError("newPassword", { message: fieldErrors.NewPassword[0] });
      }
    },
  });

  const error = mutation.error;
  const formError =
    error instanceof ApiError
      ? error.problem.errors?.CurrentPassword || error.problem.errors?.NewPassword
        ? null
        : authErrorMessage(error.code)
      : error
        ? authErrorMessage("SERVICE_UNAVAILABLE")
        : null;

  return (
    <form
      onSubmit={handleSubmit(({ currentPassword, newPassword }) =>
        mutation.mutate({ currentPassword, newPassword }),
      )}
      noValidate
      className="flex flex-col gap-4"
    >
      <FormAlert message={formError} />
      <TextField
        label="Mật khẩu hiện tại"
        type="password"
        autoComplete="current-password"
        error={errors.currentPassword?.message}
        {...register("currentPassword")}
      />
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
      <SubmitButton pending={mutation.isPending || mutation.isSuccess} pendingLabel="Đang đổi mật khẩu…">
        Đổi mật khẩu
      </SubmitButton>
    </form>
  );
}
