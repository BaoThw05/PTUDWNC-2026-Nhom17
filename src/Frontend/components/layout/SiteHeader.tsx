import Link from "next/link";
import { UserMenu } from "@/features/auth/components/UserMenu";
import { RecipeSearchForm } from "@/features/search/components/RecipeSearchForm";

const NAV_LINKS = [
  { href: "/recipes", label: "Công thức" },
  { href: "/categories", label: "Danh mục" },
  { href: "/dashboard", label: "Bảng điều khiển" },
] as const;

export function SiteHeader() {
  return (
    <header className="border-b border-black/10 dark:border-white/15 bg-white dark:bg-zinc-950">
      <div className="mx-auto flex max-w-[1920px] flex-wrap items-center gap-x-8 gap-y-3 px-[clamp(1rem,4vw,6rem)] py-3">
        <Link href="/" className="shrink-0 font-serif text-xl font-bold text-[#2e654b] dark:text-emerald-300">
          Culinary Blog
        </Link>
        <div className="order-3 w-full md:order-none md:w-72">
          <RecipeSearchForm compact />
        </div>
        <nav aria-label="Điều hướng chính" className="ml-auto flex flex-wrap items-center gap-4 text-sm">
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} className="font-medium text-[#17201b] dark:text-zinc-100 underline-offset-4 hover:text-[#2e654b] dark:hover:text-emerald-300 hover:underline">
              {link.label}
            </Link>
          ))}
          <UserMenu />
        </nav>
      </div>
    </header>
  );
}
