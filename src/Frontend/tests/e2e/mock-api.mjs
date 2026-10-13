import { createServer } from "node:http";

const recipe = {
  id: "11111111-1111-4111-8111-111111111111",
  title: "Phở bò",
  slug: "pho-bo",
  description: "Nước dùng thơm, bánh phở mềm và thịt bò.",
  prepTimeMinutes: 30,
  cookTimeMinutes: 120,
  servings: 4,
  difficulty: "Medium",
  status: "Published",
  publishedAt: "2026-10-10T08:00:00Z",
  categoryId: "22222222-2222-4222-8222-222222222222",
  thumbnailUrl: null,
  relevanceScore: 0.98,
};

const category = {
  id: recipe.categoryId,
  name: "Món nước",
  slug: "mon-nuoc",
  description: "Các món nước",
  imageUrl: null,
  orderIndex: 1,
  recipeCount: 1,
};

function paged(items, page = 1, pageSize = 12) {
  return {
    items,
    page,
    pageSize,
    totalCount: items.length,
    totalPages: items.length ? 1 : 0,
    hasNextPage: false,
    hasPreviousPage: page > 1,
  };
}

const server = createServer((request, response) => {
  const url = new URL(request.url ?? "/", "http://127.0.0.1:5100");
  response.setHeader("content-type", "application/json; charset=utf-8");

  if (url.pathname === "/health") {
    response.writeHead(200).end(JSON.stringify({ status: "Healthy" }));
    return;
  }

  if (url.pathname === "/api/v1/categories") {
    response.writeHead(200).end(JSON.stringify([category]));
    return;
  }

  if (url.pathname === "/api/v1/recipes/search") {
    const query = (url.searchParams.get("q") ?? "").normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
    const items = query.includes("pho bo") ? [recipe] : [];
    response.writeHead(200).end(JSON.stringify(paged(items)));
    return;
  }

  if (url.pathname === "/api/v1/recipes") {
    const page = Number(url.searchParams.get("page") ?? 1);
    response.writeHead(200).end(JSON.stringify(paged([recipe], page)));
    return;
  }

  response.writeHead(404).end(JSON.stringify({ title: "Not Found", status: 404 }));
});

server.listen(5100, "127.0.0.1");
