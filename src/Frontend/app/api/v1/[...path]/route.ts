import type { NextRequest } from "next/server";
import { auth } from "@/features/auth/auth";

// Chuyển tiếp /api/v1/* từ trình duyệt tới backend và gắn access token lấy từ cookie phiên ngay trên server,
// nên token không bao giờ xuống trình duyệt. auth() làm mới token khi sắp hết hạn và ghi lại cookie phiên.
const apiInternalUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5000";

// Không gửi cookie phiên Auth.js sang backend; các header còn lại (X-Forwarded-For, User-Agent…) giữ nguyên.
const DROPPED_REQUEST_HEADERS = ["host", "cookie", "authorization", "connection", "content-length"];
// fetch đã giải nén body nên độ dài/kiểu nén gốc không còn đúng.
const DROPPED_RESPONSE_HEADERS = ["content-encoding", "content-length", "transfer-encoding", "connection"];

const forward = auth(async (request) => {
  const headers = new Headers(request.headers);
  DROPPED_REQUEST_HEADERS.forEach((name) => headers.delete(name));

  const session = request.auth;
  if (session?.accessToken && !session.error) {
    headers.set("Authorization", `Bearer ${session.accessToken}`);
  }

  const hasBody = request.method !== "GET" && request.method !== "HEAD";
  const upstream = await fetch(`${apiInternalUrl}${request.nextUrl.pathname}${request.nextUrl.search}`, {
    method: request.method,
    headers,
    body: hasBody ? request.body : undefined,
    // Bắt buộc khi body là stream (upload ảnh).
    ...(hasBody ? { duplex: "half" } : {}),
    redirect: "manual",
    cache: "no-store",
  } as RequestInit);

  const responseHeaders = new Headers(upstream.headers);
  DROPPED_RESPONSE_HEADERS.forEach((name) => responseHeaders.delete(name));
  return new Response(upstream.body, { status: upstream.status, headers: responseHeaders });
});

type Handler = (request: NextRequest) => Promise<Response>;
const handler = forward as unknown as Handler;

export { handler as GET, handler as POST, handler as PUT, handler as PATCH, handler as DELETE };
