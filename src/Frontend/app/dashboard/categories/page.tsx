import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { requireSession } from "@/features/auth/server";
import { CategoryAdmin } from "@/features/categories/components/CategoryAdmin";

export const metadata: Metadata = { title: "Quản lý danh mục" };

export default async function ManageCategoriesPage() {
  const session = await requireSession("/dashboard/categories");
  if (!session.user.roles.includes("Admin")) redirect("/dashboard");

  return <CategoryAdmin />;
}
