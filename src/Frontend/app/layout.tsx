import type { Metadata } from "next";
import { Geist, Geist_Mono, Lora } from "next/font/google";
import "./globals.css";
import { SiteFooter } from "@/components/layout/SiteFooter";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { QueryProvider } from "@/components/providers/QueryProvider";
import { AuthSessionProvider } from "@/features/auth/components/AuthSessionProvider";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin", "vietnamese"],
});

const lora = Lora({
  variable: "--font-lora",
  subsets: ["latin", "vietnamese"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

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
      data-theme="dark"
      className={`${geistSans.variable} ${geistMono.variable} ${lora.variable} h-full antialiased`}
    >
      <body className="flex min-h-full flex-col">
        <AuthSessionProvider>
          <QueryProvider>
            <SiteHeader />
            <main className="mx-auto w-full max-w-[1920px] flex-1 px-[clamp(1rem,4vw,6rem)] py-10">
              {children}
            </main>
            <SiteFooter />
          </QueryProvider>
        </AuthSessionProvider>
      </body>
    </html>
  );
}
