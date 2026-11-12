"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { ApiError } from "@/lib/api/client";
import { forgotPassword } from "../api/client";
import { authErrorMessage, FORGOT_PASSWORD_SENT_MESSAGE } from "../errors";
import { forgotPasswordSchema, type ForgotPasswordValues } from "../schemas";
import { FormAlert } from "./FormAlert";
import { SubmitButton } from "./SubmitButton";
import { TextField } from "./TextField";

export function ForgotPasswordForm() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordValues>({ resolver: zodResolver(forgotPasswordSchema) });

  const mutation = useMutation({ mutationFn: forgotPassword });

  const error = mutation.error;
  const formError = error ? authErrorMessage(error instanceof ApiError ? error.code : "SERVICE_UNAVAILABLE") : null;

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate className="flex flex-col gap-4">
      <FormAlert message={formError} />
      <FormAlert message={mutation.isSuccess ? FORGOT_PASSWORD_SENT_MESSAGE : null} tone="info" />
      <TextField
        label="Email"
        type="email"
        autoComplete="email"
        error={errors.email?.message}
        {...register("email")}
      />
      <SubmitButton pending={mutation.isPending} pendingLabel="Đang gửi…">
        Gửi liên kết đặt lại
      </SubmitButton>
    </form>
  );
}
