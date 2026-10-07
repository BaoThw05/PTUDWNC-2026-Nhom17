import { RecipeGridLoading } from "@/features/search/components/RecipeGridLoading";

export default function SearchLoading() {
  return (
    <div aria-busy="true" className="space-y-7">
      <div className="h-10 w-56 animate-pulse bg-black/5" />
      <div className="h-12 animate-pulse bg-black/[0.04]" />
      <RecipeGridLoading />
    </div>
  );
}