import Link from "next/link";

export default function NotFound() {
  return (
    <section className="flex flex-col items-start gap-4">
      <p className="text-sm font-medium text-zinc-500">404</p>
      <h1 className="text-2xl font-semibold">Không tìm thấy trang</h1>
      <p className="text-zinc-600 dark:text-zinc-400">
        Trang bạn tìm không tồn tại hoặc đã bị xóa.
      </p>
      <Link href="/" className="font-medium underline">
        Về trang chủ
      </Link>
    </section>
  );
}
