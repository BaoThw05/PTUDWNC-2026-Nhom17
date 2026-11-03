import { createServer } from "node:http";

function generateSlug(text) {
  return text
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
}

const defaultRecipe = {
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
  ingredients: [
    { id: "ing-1", name: "Bánh phở", quantity: 500, unit: "g" },
    { id: "ing-2", name: "Thịt bò tái", quantity: 300, unit: "g" },
  ],
  steps: [
    { id: "step-1", stepOrder: 1, title: "Nấu nước dùng", description: "Hầm xương ống với gừng hành nướng.", durationMinutes: 90 },
    { id: "step-2", stepOrder: 2, title: "Trần bánh phở", description: "Trần bánh phở qua nước sôi rồi xếp vào tô.", durationMinutes: 5 },
  ],
};

const category = {
  id: defaultRecipe.categoryId,
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

const recipesStore = new Map([
  [defaultRecipe.id, defaultRecipe],
  [defaultRecipe.slug, defaultRecipe],
]);

// Tài khoản giả cho E2E auth (TV1); chỉ sống trong bộ nhớ của mock server.
const users = new Map([
  ["e2e@culinaryblog.test", { fullName: "Người Dùng E2E", userName: "e2euser", password: "E2e@12345" }],
]);

function authResponse(email) {
  const user = users.get(email);
  const now = Date.now();
  return {
    accessToken: `e2e-access-${user.userName}`,
    accessTokenExpiresAt: new Date(now + 15 * 60_000).toISOString(),
    refreshToken: `e2e-refresh-${user.userName}`,
    refreshTokenExpiresAt: new Date(now + 7 * 86_400_000).toISOString(),
    user: {
      id: "33333333-3333-4333-8333-333333333333",
      email,
      userName: user.userName,
      fullName: user.fullName,
      avatarUrl: null,
      roles: ["Author"],
      createdAt: new Date(now).toISOString(),
    },
  };
}

function problem(response, status, code, title) {
  response.writeHead(status).end(JSON.stringify({ title, status, code }));
}

async function readJson(request) {
  let body = "";
  for await (const chunk of request) body += chunk;
  return body ? JSON.parse(body) : {};
}

const server = createServer(async (request, response) => {
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
    const items = query.includes("pho bo") ? [defaultRecipe] : [];
    response.writeHead(200).end(JSON.stringify(paged(items)));
    return;
  }

  // Danh sách công thức cá nhân (TV2 - Task 2.16 & 2.23)
  if (url.pathname === "/api/v1/me/recipes") {
    const all = Array.from(new Set(recipesStore.values()));
    response.writeHead(200).end(JSON.stringify(paged(all)));
    return;
  }

  // Xuất bản công thức (TV2 - Task 2.11 & 2.23)
  const publishMatch = url.pathname.match(/^\/api\/v1\/recipes\/([^/]+)\/publish$/);
  if (request.method === "POST" && publishMatch) {
    const id = publishMatch[1];
    const r = recipesStore.get(id);
    if (r) {
      r.status = "Published";
      r.publishedAt = new Date().toISOString();
      recipesStore.set(r.slug, r);
    }
    response.writeHead(204).end();
    return;
  }

  // Chi tiết công thức theo ID hoặc Slug (TV2 - Task 2.09, 2.12, 2.17 & 2.23)
  const singleRecipeMatch = url.pathname.match(/^\/api\/v1\/recipes\/([^/]+)$/);
  if (request.method === "GET" && singleRecipeMatch) {
    const key = decodeURIComponent(singleRecipeMatch[1]);
    const r = recipesStore.get(key);
    if (r) {
      response.writeHead(200).end(JSON.stringify(r));
      return;
    }
  }

  // Tạo công thức mới (TV2 - Task 2.08, 2.18 & 2.23)
  if (request.method === "POST" && url.pathname === "/api/v1/recipes") {
    const body = await readJson(request);
    const id = "e2e-rcp-" + Date.now();
    const slug = generateSlug(body.title || "mon-an-moi");
    const newRecipe = {
      id,
      title: body.title,
      slug,
      description: body.description,
      prepTimeMinutes: body.prepTimeMinutes || 15,
      cookTimeMinutes: body.cookTimeMinutes || 30,
      servings: body.servings || 2,
      difficulty: body.difficulty === 0 ? "Easy" : body.difficulty === 1 ? "Medium" : "Hard",
      status: "Draft",
      publishedAt: null,
      categoryId: body.categoryId || category.id,
      thumbnailUrl: null,
      ingredients: (body.ingredients || []).map((ing, i) => ({ id: `ing-${i + 1}`, ...ing })),
      steps: (body.steps || []).map((st, i) => ({ id: `step-${i + 1}`, stepOrder: i + 1, ...st })),
    };
    recipesStore.set(id, newRecipe);
    recipesStore.set(slug, newRecipe);
    response.writeHead(201).end(JSON.stringify({ id }));
    return;
  }

  if (url.pathname === "/api/v1/recipes") {
    const page = Number(url.searchParams.get("page") ?? 1);
    const all = Array.from(new Set(recipesStore.values()));
    response.writeHead(200).end(JSON.stringify(paged(all, page)));
    return;
  }

  if (request.method === "POST" && url.pathname === "/api/v1/auth/register") {
    const { fullName, email, userName, password } = await readJson(request);
    if (users.has(email)) {
      problem(response, 409, "AUTH_EMAIL_EXISTS", "Email already exists");
      return;
    }
    users.set(email, { fullName, userName, password });
    response.writeHead(201).end(JSON.stringify(authResponse(email)));
    return;
  }

  if (request.method === "POST" && url.pathname === "/api/v1/auth/login") {
    const { email, password } = await readJson(request);
    if (users.get(email)?.password !== password) {
      problem(response, 401, "AUTH_INVALID_CREDENTIALS", "Invalid credentials");
      return;
    }
    response.writeHead(200).end(JSON.stringify(authResponse(email)));
    return;
  }

  if (request.method === "POST" && url.pathname === "/api/v1/auth/logout") {
    response.writeHead(204).end();
    return;
  }

  response.writeHead(404).end(JSON.stringify({ title: "Not Found", status: 404 }));
});

server.listen(5100, "127.0.0.1");
