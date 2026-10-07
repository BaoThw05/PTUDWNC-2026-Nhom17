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

  // Giữ các liên kết phân trang dạng ?page= cũ; URL theo path có cache ISR riêng.
  const categoryMatch = pathname.match(/^\/categories\/([^/]+)\/?$/);
  const legacyPage = request.nextUrl.searchParams.get("page");
  if (categoryMatch && legacyPage && /^[1-9]\d*$/.test(legacyPage)) {
    const page = Number(legacyPage);
    if (Number.isSafeInteger(page) && page <= 2_147_483_647) {
      const target = new URL(
        page === 1 ? `/categories/${categoryMatch[1]}` : `/categories/${categoryMatch[1]}/page/${page}`,
        request.nextUrl.origin,
      );
      return NextResponse.redirect(target);
    }
  }

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
