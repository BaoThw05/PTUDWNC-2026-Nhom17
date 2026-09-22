"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { ApiError } from "@/lib/api/client";
import { updateProfile } from "../api/client";
import { authErrorMessage } from "../errors";
import { profileSchema, type ProfileValues } from "../schemas";
import type { UserProfile } from "../types";
import { FormAlert } from "./FormAlert";
import { SubmitButton } from "./SubmitButton";
import { TextField } from "./TextField";

export function ProfileForm({ profile }: { profile: UserProfile }) {
  const router = useRouter();
  const { update: updateSession } = useSession();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<ProfileValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: { fullName: profile.fullName },
  });

  const mutation = useMutation({
    mutationFn: updateProfile,
    onSuccess: async (updated) => {
      reset({ fullName: updated.fullName });
      await updateSession({ fullName: updated.fullName });
      router.refresh();
    },
  });

  const errorMessage =
    mutation.error instanceof ApiError
      ? authErrorMessage(mutation.error.code)
      : mutation.error
        ? authErrorMessage("SERVICE_UNAVAILABLE")
        : null;

  return (
    <form
      onSubmit={handleSubmit((values) => mutation.mutate(values))}
      noValidate
      className="flex flex-col gap-4"
    >
      <FormAlert message={errorMessage} />
      <FormAlert message={mutation.isSuccess && !isDirty ? "Đã lưu thay đổi." : null} tone="info" />
      <TextField label="Họ tên" error={errors.fullName?.message} {...register("fullName")} />
      <TextField label="Email" value={profile.email} readOnly disabled name="email" />
      <SubmitButton pending={mutation.isPending} pendingLabel="Đang lưu…">
        Lưu thay đổi
      </SubmitButton>
    </form>
  );
}
