import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Đăng ký" };

export default function RegisterPage() {
  return (
    <PlaceholderPage
      title="Đăng ký"
      owner="TV1"
      requirements="FR-AUTH-001"
    />
  );
}
