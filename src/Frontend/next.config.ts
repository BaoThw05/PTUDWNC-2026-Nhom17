import type { NextConfig } from "next";

// Địa chỉ backend mà server Next.js gọi tới; trình duyệt chỉ gọi /api/* cùng origin.
const apiInternalUrl = process.env.API_INTERNAL_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  output: "standalone",
  async rewrites() {
    return [
      // /health của backend nằm ngoài /api/v1 nên cần rewrite riêng.
      { source: "/api/health", destination: `${apiInternalUrl}/health` },
      { source: "/api/:path*", destination: `${apiInternalUrl}/api/:path*` },
    ];
  },
};

export default nextConfig;
