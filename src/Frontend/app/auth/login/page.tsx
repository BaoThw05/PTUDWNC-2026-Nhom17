import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Đăng nhập" };

export default function LoginPage() {
  return (
    <PlaceholderPage
      title="Đăng nhập"
      owner="TV1"
      requirements="FR-AUTH-002, 003"
    />
  );
}
