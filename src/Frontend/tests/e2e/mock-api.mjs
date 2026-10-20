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
    const items = query.includes("pho bo") ? [recipe] : [];
    response.writeHead(200).end(JSON.stringify(paged(items)));
    return;
  }

  if (url.pathname === "/api/v1/recipes") {
    const page = Number(url.searchParams.get("page") ?? 1);
    response.writeHead(200).end(JSON.stringify(paged([recipe], page)));
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
