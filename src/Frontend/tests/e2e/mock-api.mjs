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

let createdRecipe = null;

function paged(items, page = 1, pageSize = 12, totalCount = items.length) {
  return {
    items,
    page,
    pageNumber: page,
    pageSize,
    totalCount,
    totalPages: totalCount ? Math.ceil(totalCount / pageSize) : 0,
    hasNextPage: page * pageSize < totalCount,
    hasPreviousPage: page > 1,
  };
}

async function readJsonBody(request) {
  const chunks = [];
  for await (const chunk of request) {
    chunks.push(chunk);
  }
  return JSON.parse(Buffer.concat(chunks).toString("utf8") || "{}");
}

function sendJson(response, status, body) {
  response.writeHead(status).end(JSON.stringify(body));
}

const server = createServer(async (request, response) => {
  const url = new URL(request.url ?? "/", "http://127.0.0.1:5100");
  response.setHeader("content-type", "application/json; charset=utf-8");

  if (url.pathname === "/health") {
    response.writeHead(200).end(JSON.stringify({ status: "Healthy" }));
    return;
  }

  if (request.method === "POST" && url.pathname === "/api/v1/auth/login") {
    sendJson(response, 200, {
      accessToken: "e2e-access-token",
      accessTokenExpiresAt: "2030-01-01T00:00:00Z",
      refreshToken: "e2e-refresh-token",
      refreshTokenExpiresAt: "2030-01-08T00:00:00Z",
      user: {
        id: "33333333-3333-4333-8333-333333333333",
        email: "e2e@example.com",
        userName: "e2e-user",
        fullName: "E2E User",
        avatarUrl: null,
        roles: ["User"],
        createdAt: "2026-01-01T00:00:00Z",
        emailConfirmed: true,
      },
    });
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

  if (request.method === "GET" && url.pathname === "/api/v1/recipes") {
    const page = Number(url.searchParams.get("page") ?? 1);
    const pageSize = Number(url.searchParams.get("pageSize") ?? 12);
    const publishedRecipes = [recipe, ...(createdRecipe?.status === "Published" ? [createdRecipe] : [])]
      .sort((left, right) => Date.parse(right.publishedAt ?? "") - Date.parse(left.publishedAt ?? ""));
    const start = (page - 1) * pageSize;
    response.writeHead(200).end(JSON.stringify(
      paged(publishedRecipes.slice(start, start + pageSize), page, pageSize, publishedRecipes.length),
    ));
    return;
  }

  if (request.method === "POST" && url.pathname === "/api/v1/recipes") {
    const body = await readJsonBody(request);
    createdRecipe = {
      id: "44444444-4444-4444-8444-444444444444",
      title: body.title,
      slug: "bun-bo-hue-gia-truyen-e2e",
      description: body.description,
      prepTimeMinutes: body.prepTimeMinutes,
      cookTimeMinutes: body.cookTimeMinutes,
      servings: body.servings,
      difficulty: body.difficulty,
      status: "Draft",
      publishedAt: null,
      authorId: "33333333-3333-4333-8333-333333333333",
      categoryId: body.categoryId ?? category.id,
      version: 1,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      nutrition: body.nutrition,
      ingredients: (body.ingredients ?? []).map((ingredient, index) => ({
        id: `55555555-5555-4555-8555-${String(index + 1).padStart(12, "0")}`,
        name: ingredient.name,
        quantity: ingredient.quantity,
        unit: ingredient.unit,
        orderIndex: index + 1,
      })),
      steps: (body.steps ?? []).map((step, index) => ({
        id: `66666666-6666-4666-8666-${String(index + 1).padStart(12, "0")}`,
        stepNumber: index + 1,
        title: step.title ?? `Bước ${index + 1}`,
        description: step.description,
        durationMinutes: step.durationMinutes,
      })),
    };
    sendJson(response, 201, { id: createdRecipe.id });
    return;
  }

  if (request.method === "POST" && /^\/api\/v1\/recipes\/[^/]+\/publish$/.test(url.pathname)) {
    if (createdRecipe) {
      createdRecipe.status = "Published";
      createdRecipe.publishedAt = new Date().toISOString();
      createdRecipe.updatedAt = createdRecipe.publishedAt;
    }
    response.writeHead(204).end();
    return;
  }

  if (request.method === "GET" && url.pathname === "/api/v1/me/recipes") {
    response.writeHead(200).end(JSON.stringify(paged(createdRecipe ? [createdRecipe] : [])));
    return;
  }

  if (request.method === "GET" && /^\/api\/v1\/recipes\/[^/]+\/images$/.test(url.pathname)) {
    response.writeHead(200).end(JSON.stringify([]));
    return;
  }

  if (request.method === "GET" && url.pathname.startsWith("/api/v1/recipes/")) {
    const slug = decodeURIComponent(url.pathname.slice("/api/v1/recipes/".length));
    if (createdRecipe && createdRecipe.slug === slug) {
      sendJson(response, 200, createdRecipe);
      return;
    }
  }

  response.writeHead(404).end(JSON.stringify({ title: "Not Found", status: 404 }));
});

server.listen(5100, "127.0.0.1");
