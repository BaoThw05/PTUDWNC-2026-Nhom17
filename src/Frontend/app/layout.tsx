import type { Metadata } from "next";
import "./globals.css";
import { SiteFooter } from "@/components/layout/SiteFooter";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { QueryProvider } from "@/components/providers/QueryProvider";
import { AuthSessionProvider } from "@/features/auth/components/AuthSessionProvider";

export const metadata: Metadata = {
  title: {
    default: "Culinary Blog",
    template: "%s · Culinary Blog",
  },
  description: "Nền tảng chia sẻ công thức nấu ăn",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="vi"
      className="h-full antialiased"
    >
      <body className="flex min-h-full flex-col">
        <AuthSessionProvider>
          <QueryProvider>
            <SiteHeader />
            <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-10">
              {children}
            </main>
            <SiteFooter />
          </QueryProvider>
        </AuthSessionProvider>
      </body>
    </html>
  );
}
