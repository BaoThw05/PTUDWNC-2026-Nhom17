"use client";

import { useEffect } from "react";

export default function Error({
  error,
  retry,
}: {
  error: Error & { digest?: string };
  retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <section className="flex flex-col items-start gap-4">
      <p className="text-sm font-medium text-zinc-500">500</p>
      <h1 className="text-2xl font-semibold">Đã có lỗi xảy ra</h1>
      <p className="text-zinc-600 dark:text-zinc-400">
        Vui lòng thử lại sau ít phút.
      </p>
      <button
        type="button"
        onClick={() => retry()}
        className="rounded-full bg-foreground px-4 py-2 text-background"
      >
        Thử lại
      </button>
    </section>
  );
}
