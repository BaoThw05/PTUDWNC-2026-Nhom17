"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { authErrorMessage } from "../errors";
import { loginSchema, type LoginValues } from "../schemas";
import { FormAlert } from "./FormAlert";
import { GoogleSignInButton } from "./GoogleSignInButton";
import { SubmitButton } from "./SubmitButton";
import { TextField } from "./TextField";

type LoginFormProps = {
  callbackUrl: string;
  googleEnabled: boolean;
  initialMessage: string | null;
  initialTone?: "error" | "info";
};

export function LoginForm({ callbackUrl, googleEnabled, initialMessage, initialTone = "error" }: LoginFormProps) {
  const router = useRouter();
  const [formError, setFormError] = useState<string | null>(initialMessage);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({ resolver: zodResolver(loginSchema) });

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null);
    const result = await signIn("credentials", { ...values, redirect: false });

    if (!result || result.error) {
      setFormError(authErrorMessage(result?.code));
      return;
    }

    router.replace(callbackUrl);
    router.refresh();
  });

  return (
    <div className="flex flex-col gap-5">
      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
        <FormAlert message={formError} tone={formError === initialMessage ? initialTone : "error"} />
        <TextField
          label="Email"
          type="email"
          autoComplete="email"
          error={errors.email?.message}
          {...register("email")}
        />
        <TextField
          label="Mật khẩu"
          type="password"
          autoComplete="current-password"
          error={errors.password?.message}
          {...register("password")}
        />
        <SubmitButton pending={isSubmitting} pendingLabel="Đang đăng nhập…">
          Đăng nhập
        </SubmitButton>
      </form>

      {googleEnabled && (
        <>
          <div className="flex items-center gap-3 text-xs text-zinc-500">
            <span className="h-px flex-1 bg-black/10 dark:bg-white/15" />
            hoặc
            <span className="h-px flex-1 bg-black/10 dark:bg-white/15" />
          </div>
          <GoogleSignInButton callbackUrl={callbackUrl} />
        </>
      )}
    </div>
  );
}
