"use client";

import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api/client";

const HEALTH_REFRESH_MS = 30_000;

export function HealthStatus() {
  const { isPending, isError, data } = useQuery({
    queryKey: ["health"],
    queryFn: () => apiClient.get<string>("/api/health", { cache: "no-store" }),
    refetchInterval: HEALTH_REFRESH_MS,
    retry: false,
  });

  const { label, dotClass } = isPending
    ? { label: "Đang kiểm tra…", dotClass: "bg-zinc-400" }
    : isError
      ? { label: "Không kết nối được", dotClass: "bg-red-500" }
      : { label: `OK (${data})`, dotClass: "bg-emerald-500" };

  return (
    <div className="flex items-center gap-2 rounded-lg border border-black/10 px-4 py-3 text-sm dark:border-white/15">
      <span className={`size-2.5 rounded-full ${dotClass}`} aria-hidden />
      <span>
        Backend API: <strong>{label}</strong>
      </span>
    </div>
  );
}
