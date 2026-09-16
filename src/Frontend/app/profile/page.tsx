import type { Metadata } from "next";
import Image from "next/image";
import { redirect } from "next/navigation";
import { ApiError } from "@/lib/api/client";
import { getProfile } from "@/features/auth/api/backend";
import { ProfileForm } from "@/features/auth/components/ProfileForm";
import { SignOutButton } from "@/features/auth/components/SignOutButton";
import { LOGIN_PATH, PROFILE_PATH } from "@/features/auth/constants";
import { requireSession } from "@/features/auth/server";
import type { UserProfile } from "@/features/auth/types";

export const metadata: Metadata = { title: "Hồ sơ cá nhân" };

const ROLE_LABELS: Record<string, string> = { Admin: "Quản trị viên", Author: "Tác giả" };

const joinedDate = new Intl.DateTimeFormat("vi-VN", { dateStyle: "long" });

async function loadProfile(accessToken: string): Promise<UserProfile> {
  try {
    return await getProfile(accessToken);
  } catch (error) {
    if (error instanceof ApiError && (error.status === 401 || error.status === 404)) {
      redirect(`${LOGIN_PATH}?reason=expired`);
    }
    throw error;
  }
}

export default async function ProfilePage() {
  const session = await requireSession(PROFILE_PATH);
  const profile = await loadProfile(session.accessToken);

  return (
    <section className="mx-auto flex w-full max-w-2xl flex-col gap-8">
      <header className="flex items-center gap-4">
        {profile.avatarUrl ? (
          <Image
            src={profile.avatarUrl}
            alt=""
            width={64}
            height={64}
            unoptimized
            className="size-16 rounded-full object-cover"
          />
        ) : (
          <span className="flex size-16 items-center justify-center rounded-full bg-foreground text-2xl font-semibold text-background">
            {profile.displayName.charAt(0).toUpperCase()}
          </span>
        )}
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold tracking-tight">{profile.displayName}</h1>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            {profile.roles.map((role) => ROLE_LABELS[role] ?? role).join(" · ")} · tham gia{" "}
            {joinedDate.format(new Date(profile.createdAt))}
          </p>
        </div>
      </header>

      <div className="rounded-2xl border border-black/10 p-6 dark:border-white/15">
        <h2 className="mb-4 text-lg font-semibold">Thông tin tài khoản</h2>
        <ProfileForm profile={profile} />
      </div>

      <SignOutButton className="self-start text-sm text-red-600 hover:underline dark:text-red-400" />
    </section>
  );
}
