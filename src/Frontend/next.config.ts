import type { NextConfig } from "next";

// Địa chỉ backend mà server Next.js gọi tới; trình duyệt chỉ gọi /api/* cùng origin.
const apiInternalUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  async rewrites() {
    return {
      beforeFiles: [],
      // /health của backend nằm ngoài /api/v1 nên cần rewrite riêng.
      afterFiles: [{ source: "/api/health", destination: `${apiInternalUrl}/health` }],
      // Để ở fallback: chỉ áp dụng khi không có route nào của Next.js khớp (ví dụ /api/auth/* của Auth.js).
      fallback: [{ source: "/api/:path*", destination: `${apiInternalUrl}/api/:path*` }],
    };
  },
};

export default nextConfig;
