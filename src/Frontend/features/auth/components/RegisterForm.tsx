"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm, useWatch, type UseFormSetError } from "react-hook-form";
import { ApiError } from "@/lib/api/client";
import { register as registerAccount } from "../api/client";
import { LOGIN_PATH } from "../constants";
import { authErrorMessage } from "../errors";
import { registerSchema, type RegisterValues } from "../schemas";
import { FormAlert } from "./FormAlert";
import { PasswordRequirements } from "./PasswordRequirements";
import { SubmitButton } from "./SubmitButton";
import { TextField } from "./TextField";

// Tên trường trong lỗi validation của backend (PascalCase) → tên trường trên form.
const BACKEND_FIELDS: Record<string, keyof RegisterValues> = {
  FullName: "fullName",
  Email: "email",
  UserName: "userName",
  Password: "password",
};

function applyBackendErrors(error: ApiError, setError: UseFormSetError<RegisterValues>): boolean {
  if (error.code === "AUTH_EMAIL_EXISTS") {
    setError("email", { message: authErrorMessage(error.code) });
    return true;
  }

  if (error.code === "AUTH_USERNAME_EXISTS") {
    setError("userName", { message: authErrorMessage(error.code) });
    return true;
  }

  let applied = false;
  for (const [field, messages] of Object.entries(error.problem.errors ?? {})) {
    const formField = BACKEND_FIELDS[field];
    if (formField && messages[0]) {
      setError(formField, { message: messages[0] });
      applied = true;
    }
  }
  return applied;
}

export function RegisterForm({ callbackUrl }: { callbackUrl: string }) {
  const router = useRouter();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    control,
    formState: { errors, isSubmitting },
  } = useForm<RegisterValues>({ resolver: zodResolver(registerSchema) });

  const password = useWatch({ control, name: "password" });

  const onSubmit = handleSubmit(async ({ fullName, email, userName, password }) => {
    setFormError(null);

    try {
      await registerAccount({ fullName, email, userName, password });
    } catch (error) {
      if (!(error instanceof ApiError)) {
        setFormError(authErrorMessage("SERVICE_UNAVAILABLE"));
      } else if (!applyBackendErrors(error, setError)) {
        setFormError(authErrorMessage(error.code));
      }
      return;
    }

    // FR-AUTH-001: đăng ký xong đăng nhập luôn.
    const result = await signIn("credentials", { email, password, redirect: false });
    if (!result || result.error) {
      router.replace(`${LOGIN_PATH}?callbackUrl=${encodeURIComponent(callbackUrl)}`);
      return;
    }

    router.replace(callbackUrl);
    router.refresh();
  });

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
      <FormAlert message={formError} />
      <TextField
        label="Họ tên"
        autoComplete="name"
        error={errors.fullName?.message}
        {...register("fullName")}
      />
      <TextField
        label="Email"
        type="email"
        autoComplete="email"
        error={errors.email?.message}
        {...register("email")}
      />
      <TextField
        label="Tên đăng nhập"
        autoComplete="username"
        error={errors.userName?.message}
        {...register("userName")}
      />
      <TextField
        label="Mật khẩu"
        type="password"
        autoComplete="new-password"
        error={errors.password?.message}
        {...register("password")}
      />
      <PasswordRequirements value={password ?? ""} />
      <TextField
        label="Nhập lại mật khẩu"
        type="password"
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register("confirmPassword")}
      />
      <SubmitButton pending={isSubmitting} pendingLabel="Đang tạo tài khoản…">
        Đăng ký
      </SubmitButton>
    </form>
  );
}
