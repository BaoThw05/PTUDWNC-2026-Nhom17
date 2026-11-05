import type { LoginAttempt } from "../types";

const RESULT_LABELS: Record<LoginAttempt["result"], string> = {
  Succeeded: "Thành công",
  InvalidPassword: "Sai mật khẩu",
  LockedOut: "Bị khóa tạm",
  Disabled: "Tài khoản bị vô hiệu hóa",
};

const METHOD_LABELS: Record<LoginAttempt["method"], string> = { Password: "Mật khẩu", Google: "Google" };

const timeFormat = new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short", timeZone: "Asia/Ho_Chi_Minh" });

// shortcut: chỉ nhận ra vài trình duyệt/hệ điều hành phổ biến, còn lại hiện nguyên User-Agent; dùng thư viện ua-parser nếu cần chính xác hơn.
function describeDevice(userAgent: string | null): string {
  if (!userAgent) return "Không rõ";
  const browser = /Edg\//.test(userAgent)
    ? "Edge"
    : /OPR\//.test(userAgent)
      ? "Opera"
      : /Chrome\//.test(userAgent)
        ? "Chrome"
        : /Firefox\//.test(userAgent)
          ? "Firefox"
          : /Safari\//.test(userAgent)
            ? "Safari"
            : null;
  const os = /Windows/.test(userAgent)
    ? "Windows"
    : /Android/.test(userAgent)
      ? "Android"
      : /iPhone|iPad/.test(userAgent)
        ? "iOS"
        : /Mac OS X/.test(userAgent)
          ? "macOS"
          : /Linux/.test(userAgent)
            ? "Linux"
            : null;
  return browser && os ? `${browser} trên ${os}` : userAgent;
}

export function LoginHistoryTable({ attempts }: { attempts: LoginAttempt[] }) {
  if (attempts.length === 0) {
    return <p className="text-sm text-zinc-600 dark:text-zinc-400">Chưa có lần đăng nhập nào được ghi lại.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead className="text-zinc-600 dark:text-zinc-400">
          <tr>
            <th scope="col" className="py-2 pr-4 font-medium">Thời gian</th>
            <th scope="col" className="py-2 pr-4 font-medium">Thiết bị</th>
            <th scope="col" className="py-2 pr-4 font-medium">IP</th>
            <th scope="col" className="py-2 font-medium">Kết quả</th>
          </tr>
        </thead>
        <tbody>
          {attempts.map((attempt) => (
            <tr key={`${attempt.occurredAt}-${attempt.result}`} className="border-t border-black/10 dark:border-white/15">
              <td className="py-2 pr-4 whitespace-nowrap">{timeFormat.format(new Date(attempt.occurredAt))}</td>
              <td className="py-2 pr-4" title={attempt.userAgent ?? undefined}>
                {describeDevice(attempt.userAgent)} · {METHOD_LABELS[attempt.method]}
              </td>
              <td className="py-2 pr-4 whitespace-nowrap">{attempt.ipAddress ?? "—"}</td>
              <td className={`py-2 ${attempt.result === "Succeeded" ? "" : "text-red-600 dark:text-red-400"}`}>
                {RESULT_LABELS[attempt.result]}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
