import type { NextRequest } from "next/server";
import { handlers } from "@/features/auth/auth";

// Trình duyệt đọc phiên qua /api/auth/session (useSession, update): bỏ accessToken khỏi JSON trả về để token
// chỉ nằm trong cookie HttpOnly; server vẫn lấy được qua auth() và route /api/v1/* tự gắn token.
async function withoutAccessToken(response: Response): Promise<Response> {
  if (!response.ok || !response.headers.get("content-type")?.includes("json")) {
    return response;
  }

  const session = (await response.json()) as Record<string, unknown> | null;
  if (session) {
    delete session.accessToken;
  }

  const headers = new Headers(response.headers);
  headers.delete("content-length");
  return new Response(JSON.stringify(session), { status: response.status, headers });
}

function isSessionRequest(request: NextRequest): boolean {
  return request.nextUrl.pathname.endsWith("/session");
}

export async function GET(request: NextRequest) {
  const response = await handlers.GET(request);
  return isSessionRequest(request) ? withoutAccessToken(response) : response;
}

export async function POST(request: NextRequest) {
  const response = await handlers.POST(request);
  return isSessionRequest(request) ? withoutAccessToken(response) : response;
}
