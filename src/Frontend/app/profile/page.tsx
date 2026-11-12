import type { Metadata } from "next";
import Image from "next/image";
import { redirect } from "next/navigation";
import { ApiError } from "@/lib/api/client";
import { getLoginHistory, getProfile } from "@/features/auth/api/backend";
import { ChangeEmailForm } from "@/features/auth/components/ChangeEmailForm";
import { ChangePasswordForm } from "@/features/auth/components/ChangePasswordForm";
import { LoginHistoryTable } from "@/features/auth/components/LoginHistoryTable";
import { ProfileForm } from "@/features/auth/components/ProfileForm";
import { ResendConfirmationButton } from "@/features/auth/components/ResendConfirmationButton";
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
  // Lần lượt: loadProfile chuyển về trang đăng nhập khi phiên hết hạn trước khi gọi tiếp.
  const profile = await loadProfile(session.accessToken);
  const loginHistory = await getLoginHistory(session.accessToken);

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
            {profile.fullName.charAt(0).toUpperCase()}
          </span>
        )}
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold tracking-tight">{profile.fullName}</h1>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            @{profile.userName} · {profile.roles.map((role) => ROLE_LABELS[role] ?? role).join(" · ")} · tham gia{" "}
            {joinedDate.format(new Date(profile.createdAt))}
          </p>
        </div>
      </header>

      <div className="rounded-2xl border border-black/10 p-6 dark:border-white/15">
        <h2 className="mb-4 text-lg font-semibold">Thông tin tài khoản</h2>
        <ProfileForm profile={profile} />
        <div className="mt-4 flex flex-col gap-2 text-sm">
          <p>
            Email {profile.email}:{" "}
            {profile.emailConfirmed ? (
              <span className="font-medium text-emerald-700 dark:text-emerald-400">đã xác nhận</span>
            ) : (
              <span className="font-medium text-amber-700 dark:text-amber-400">chưa xác nhận</span>
            )}
          </p>
          {!profile.emailConfirmed && <ResendConfirmationButton />}
        </div>
      </div>

      <div className="rounded-2xl border border-black/10 p-6 dark:border-white/15">
        <h2 className="mb-1 text-lg font-semibold">Đổi email</h2>
        <p className="mb-4 text-sm text-zinc-600 dark:text-zinc-400">
          Chúng tôi gửi mã 6 chữ số tới email mới. Tài khoản đăng nhập bằng Google chưa có mật khẩu: dùng &quot;Quên mật
          khẩu&quot; để đặt mật khẩu trước.
        </p>
        <ChangeEmailForm />
      </div>

      <div className="rounded-2xl border border-black/10 p-6 dark:border-white/15">
        <h2 className="mb-1 text-lg font-semibold">Đổi mật khẩu</h2>
        <p className="mb-4 text-sm text-zinc-600 dark:text-zinc-400">
          Sau khi đổi, bạn sẽ được đăng xuất khỏi mọi thiết bị và cần đăng nhập lại.
        </p>
        <ChangePasswordForm />
      </div>

      <div className="rounded-2xl border border-black/10 p-6 dark:border-white/15">
        <h2 className="mb-1 text-lg font-semibold">Nhật ký đăng nhập</h2>
        <p className="mb-4 text-sm text-zinc-600 dark:text-zinc-400">
          20 lần đăng nhập gần nhất. Thấy thiết bị lạ? Hãy đổi mật khẩu ngay.
        </p>
        <LoginHistoryTable attempts={loginHistory} />
      </div>

      <SignOutButton className="self-start text-sm text-red-600 hover:underline dark:text-red-400" />
    </section>
  );
}
