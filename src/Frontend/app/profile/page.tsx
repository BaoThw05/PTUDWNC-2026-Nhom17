"use client";

import { FormEvent, useEffect, useState } from "react";
import { useSession, signOut } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api-client";

interface UserProfileDto {
  id: string;
  fullName: string;
  email: string;
  userName: string;
  avatarUrl: string | null;
  roles: string[];
  emailConfirmed: boolean;
  createdAt: string;
}

export default function ProfilePage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const queryClient = useQueryClient();

  const [fullName, setFullName] = useState("");
  const [avatarUrl, setAvatarUrl] = useState("");

  useEffect(() => {
    if (status === "unauthenticated") {
      router.push("/auth/login");
      return;
    }

    // jwt() callback không đổi được Google ID token / refresh token lấy token
    // hệ thống thành công (backend từ chối, mất mạng...) -> đăng xuất, không
    // giữ session nửa vời.
    if (session?.error) {
      signOut({ callbackUrl: "/auth/login?error=GoogleLoginError" });
    }
  }, [status, session, router]);

  const { data: profile, isLoading } = useQuery({
    queryKey: ["auth", "me"],
    queryFn: async () => {
      const res = await apiClient.get<UserProfileDto>("/api/v1/auth/me");
      return res.data;
    },
    enabled: status === "authenticated",
  });

  useEffect(() => {
    if (profile) {
      setFullName(profile.fullName);
      setAvatarUrl(profile.avatarUrl ?? "");
    }
  }, [profile]);

  const updateMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.patch<UserProfileDto>("/api/v1/auth/me", {
        fullName,
        avatarUrl: avatarUrl || null,
      });
      return res.data;
    },
    onSuccess: (data) => {
      queryClient.setQueryData(["auth", "me"], data);
    },
  });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    updateMutation.mutate();
  }

  if (status === "loading" || isLoading) {
    return <p className="p-8">Đang tải...</p>;
  }

  if (!profile) {
    return null;
  }

  return (
    <main className="mx-auto flex min-h-screen max-w-sm flex-col justify-center gap-6 p-8">
      <h1 className="text-2xl font-bold">Hồ sơ cá nhân</h1>

      <div className="text-sm text-gray-600">
        <p>Email: {profile.email}</p>
        <p>Tên đăng nhập: {profile.userName}</p>
        <p>Vai trò: {profile.roles.join(", ")}</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <label className="flex flex-col gap-1">
          <span className="text-sm font-medium">Họ và tên</span>
          <input
            required
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            className="rounded border border-gray-300 px-3 py-2"
          />
        </label>

        <label className="flex flex-col gap-1">
          <span className="text-sm font-medium">Avatar URL</span>
          <input
            value={avatarUrl}
            onChange={(e) => setAvatarUrl(e.target.value)}
            className="rounded border border-gray-300 px-3 py-2"
          />
        </label>

        {updateMutation.isError && (
          <p className="text-sm text-red-600">Cập nhật thất bại.</p>
        )}
        {updateMutation.isSuccess && (
          <p className="text-sm text-green-600">Đã lưu thay đổi.</p>
        )}

        <button
          type="submit"
          disabled={updateMutation.isPending}
          className="rounded bg-blue-600 px-4 py-2 font-medium text-white disabled:opacity-50"
        >
          {updateMutation.isPending ? "Đang lưu..." : "Lưu thay đổi"}
        </button>
      </form>

      <button
        onClick={() => signOut({ callbackUrl: "/auth/login" })}
        className="text-sm text-gray-600 underline"
      >
        Đăng xuất
      </button>
    </main>
  );
}
