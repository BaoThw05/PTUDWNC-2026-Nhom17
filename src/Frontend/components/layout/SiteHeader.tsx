import Link from "next/link";
import { UserMenu } from "@/features/auth/components/UserMenu";

const NAV_LINKS = [
  { href: "/recipes", label: "Công thức" },
  { href: "/categories", label: "Danh mục" },
  { href: "/search", label: "Tìm kiếm" },
  { href: "/dashboard", label: "Bảng điều khiển" },
] as const;

export function SiteHeader() {
  return (
    <header className="border-b border-black/10 dark:border-white/15">
      <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-4 px-4 py-4">
        <Link href="/" className="text-lg font-semibold">
          Culinary Blog
        </Link>
        <nav className="flex flex-wrap items-center gap-4 text-sm">
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} className="hover:underline">
              {link.label}
            </Link>
          ))}
          <UserMenu />
        </nav>
      </div>
    </header>
  );
}
