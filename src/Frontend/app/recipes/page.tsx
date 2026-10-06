import type { Metadata } from "next";
import { RecipesScreen } from "@/features/search/components/RecipesScreen";

export const metadata: Metadata = { title: "Công thức" };

type SearchParams = Record<string, string | string[] | undefined>;

export const dynamic = "force-dynamic";

export default async function RecipesPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  return <RecipesScreen searchParams={await searchParams} />;
}
