"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { ApiError } from "@/lib/api/client";
import { confirmEmailChange, requestEmailChange } from "../api/client";
import { authErrorMessage } from "../errors";
import { emailChangeSchema, emailCodeSchema, type EmailChangeValues, type EmailCodeValues } from "../schemas";
import { FormAlert } from "./FormAlert";
import { SubmitButton } from "./SubmitButton";
import { TextField } from "./TextField";

function errorMessage(error: Error | null): string | null {
  if (!error) return null;
  return error instanceof ApiError ? authErrorMessage(error.code) : authErrorMessage("SERVICE_UNAVAILABLE");
}

/** Bước 1: email mới + mật khẩu hiện tại → backend gửi mã. Bước 2: nhập mã 6 chữ số. */
export function ChangeEmailForm() {
  const [pendingEmail, setPendingEmail] = useState<string | null>(null);

  return pendingEmail ? (
    <ConfirmCodeStep newEmail={pendingEmail} onBack={() => setPendingEmail(null)} />
  ) : (
    <RequestStep onSent={setPendingEmail} />
  );
}

function RequestStep({ onSent }: { onSent: (newEmail: string) => void }) {
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<EmailChangeValues>({ resolver: zodResolver(emailChangeSchema) });

  const mutation = useMutation({
    mutationFn: requestEmailChange,
    onSuccess: (_, values) => onSent(values.newEmail),
    onError: (error) => {
      const fieldErrors = error instanceof ApiError ? error.problem.errors : undefined;
      if (fieldErrors?.CurrentPassword?.[0]) setError("currentPassword", { message: "Mật khẩu hiện tại không đúng." });
      if (fieldErrors?.NewEmail?.[0]) setError("newEmail", { message: "Email mới phải khác email hiện tại." });
    },
  });

  const fieldErrors = mutation.error instanceof ApiError ? mutation.error.problem.errors : undefined;
  const formError = fieldErrors?.CurrentPassword || fieldErrors?.NewEmail ? null : errorMessage(mutation.error);

  return (
    <form onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate className="flex flex-col gap-4">
      <FormAlert message={formError} />
      <TextField label="Email mới" type="email" autoComplete="email" error={errors.newEmail?.message} {...register("newEmail")} />
      <TextField
        label="Mật khẩu hiện tại"
        type="password"
        autoComplete="current-password"
        error={errors.currentPassword?.message}
        {...register("currentPassword")}
      />
      <SubmitButton pending={mutation.isPending} pendingLabel="Đang gửi mã…">
        Gửi mã xác minh
      </SubmitButton>
    </form>
  );
}

function ConfirmCodeStep({ newEmail, onBack }: { newEmail: string; onBack: () => void }) {
  const router = useRouter();
  const { update: updateSession } = useSession();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<EmailCodeValues>({ resolver: zodResolver(emailCodeSchema) });

  const mutation = useMutation({
    mutationFn: confirmEmailChange,
    onSuccess: async (updated) => {
      await updateSession({ email: updated.email });
      router.refresh();
    },
  });

  return (
    <form
      onSubmit={handleSubmit(({ code }) => mutation.mutate({ newEmail, code }))}
      noValidate
      className="flex flex-col gap-4"
    >
      <FormAlert message={errorMessage(mutation.error)} />
      <FormAlert message={mutation.isSuccess ? `Đã đổi email thành ${newEmail}.` : `Đã gửi mã tới ${newEmail}.`} tone="info" />
      <TextField
        label="Mã xác minh"
        inputMode="numeric"
        autoComplete="one-time-code"
        maxLength={6}
        error={errors.code?.message}
        {...register("code")}
      />
      <SubmitButton pending={mutation.isPending || mutation.isSuccess} pendingLabel="Đang xác minh…">
        Xác nhận đổi email
      </SubmitButton>
      {!mutation.isSuccess && (
        <button type="button" onClick={onBack} className="self-start text-sm underline">
          Dùng email khác / gửi lại mã
        </button>
      )}
    </form>
  );
}
