import { NextResponse } from "next/server";
import { auth } from "@/features/auth/auth";

const PROTECTED_PREFIXES = ["/dashboard", "/profile"];

function isProtected(pathname: string): boolean {
  return PROTECTED_PREFIXES.some((prefix) => pathname === prefix || pathname.startsWith(`${prefix}/`));
}

// Chạy trên mọi trang để Auth.js kịp làm mới token và ghi lại cookie trước khi trang render;
// chỉ chuyển hướng với các trang cần đăng nhập.
export default auth((request) => {
  const { pathname, search } = request.nextUrl;
  const session = request.auth;

  if (isProtected(pathname) && (!session || session.error)) {
    const loginUrl = new URL("/auth/login", request.nextUrl.origin);
    loginUrl.searchParams.set("callbackUrl", `${pathname}${search}`);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
});

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\.[\\w]+$).*)"],
};
