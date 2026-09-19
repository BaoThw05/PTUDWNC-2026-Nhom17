import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/PlaceholderPage";

export const metadata: Metadata = { title: "Hồ sơ cá nhân" };

export default function ProfilePage() {
  return (
    <PlaceholderPage
      title="Hồ sơ cá nhân"
      owner="TV1"
      requirements="FR-AUTH-006, 007"
    />
  );
}
