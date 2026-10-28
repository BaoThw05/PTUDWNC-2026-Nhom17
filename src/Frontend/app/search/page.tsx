import type { Metadata } from "next";
import { SearchScreen } from "@/features/search/components/SearchScreen";

export const metadata: Metadata = { title: "Tìm kiếm" };

type SearchParams = Record<string, string | string[] | undefined>;

export const dynamic = "force-dynamic";

export default async function SearchPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  return <SearchScreen searchParams={await searchParams} />;
}
