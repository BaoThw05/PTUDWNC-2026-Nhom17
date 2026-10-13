import { expect, test } from "@playwright/test";

test("publishes a sitemap with recipes, categories, and static pages", async ({ request }) => {
  const response = await request.get("/sitemap.xml");

  expect(response.ok()).toBeTruthy();
  const xml = await response.text();
  expect(xml).toContain("/recipes");
  expect(xml).toMatch(/<loc>[^<]+\/recipes\/[^<]+<\/loc>/);
  expect(xml).toMatch(/<loc>[^<]+\/categories\/[^<]+<\/loc>/);
  expect(xml).toContain("<lastmod>");
  expect(xml).toContain("<changefreq>");
  expect(xml).toContain("<priority>");
});

test("allows crawlers and points them to the sitemap", async ({ request }) => {
  const response = await request.get("/robots.txt");

  expect(response.ok()).toBeTruthy();
  const robots = await response.text();
  expect(robots).toContain("User-Agent: *");
  expect(robots).toContain("Allow: /");
  const sitemapLine = robots.match(/^Sitemap:\s+(\S+)$/m);
  expect(sitemapLine).not.toBeNull();
  expect(new URL(sitemapLine![1]).pathname).toBe("/sitemap.xml");
});
