import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import { CategoryIndexPage } from "@/features/categories/components/CategoryIndexPage";
import { CategoryDetailPage, loadCategory } from "@/features/categories/components/CategoryDetailPage";
import { buildPageMetadata } from "@/features/search/seo/metadata";

export const revalidate = 3600;

export function generateStaticParams() {
  return [];
}

type Route = { slug: string; page: number } | null;

function parseRoute(segments: string[] | undefined): Route {
  if (!segments?.length) return null;
  if (segments.length === 1) return { slug: segments[0], page: 1 };
  if (segments.length !== 3 || segments[1] !== "page" || !/^[1-9]\d*$/.test(segments[2])) notFound();
  const page = Number(segments[2]);
  if (!Number.isSafeInteger(page) || page > 2_147_483_647) notFound();
  return { slug: segments[0], page };
}

export async function generateMetadata({ params }: PageProps<"/categories/[[...segments]]">): Promise<Metadata> {
  const route = parseRoute((await params).segments);
  if (!route) {
    return buildPageMetadata({
      title: "Danh mục món ăn",
      description: "Khám phá công thức nấu ăn theo từng danh mục.",
      path: "/categories",
    });
  }
  const { category } = await loadCategory(route.slug, 1);
  return buildPageMetadata({
    title: category.name,
    description: category.description ?? `Khám phá công thức thuộc danh mục ${category.name}.`,
    path: `/categories/${encodeURIComponent(route.slug)}`,
  });
}

export default async function CategoriesPage({ params }: PageProps<"/categories/[[...segments]]">) {
  const route = parseRoute((await params).segments);
  if (!route) return <CategoryIndexPage />;
  if (route.page === 1 && (await params).segments?.length === 3) {
    redirect(`/categories/${encodeURIComponent(route.slug)}`);
  }
  return <CategoryDetailPage slug={route.slug} page={route.page} />;
}
