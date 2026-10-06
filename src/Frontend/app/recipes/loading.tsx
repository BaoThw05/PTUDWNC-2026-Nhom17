import { RecipeGridLoading } from "@/features/search/components/RecipeGridLoading";

export default function RecipesLoading() {
  return (
    <div aria-busy="true" className="space-y-7">
      <div className="h-10 w-56 animate-pulse bg-black/5" />
      <div className="h-24 animate-pulse border-y border-black/10 bg-black/[0.02]" />
      <RecipeGridLoading />
    </div>
  );
}