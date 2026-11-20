import { writeFile } from "node:fs/promises";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { chromium } from "../../src/Frontend/node_modules/playwright/index.mjs";

const origin = process.env.DEMO_ORIGIN ?? "http://localhost";
const evidenceDirectory = fileURLToPath(new URL("../../docs/evidence/tv4/", import.meta.url));
const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 1 });
const results = [];

for (const [name, path] of [
  ["home", "/"],
  ["category", "/categories/mon-chinh"],
  ["recipe-pho-bo", "/recipes/sample-39-pho-bo"],
  ["recipe-chuoi-nep", "/recipes/sample-02-chuoi-nep-nuong-la-dua"],
  ["recipe-dua-tac", "/recipes/sample-15-dua-tac"],
]) {
  const response = await page.goto(new URL(path, origin).href, { waitUntil: "networkidle" });
  const metadata = await page.evaluate(() => {
    const structuredData = [...document.querySelectorAll('script[type="application/ld+json"]')]
      .map((node) => JSON.parse(node.textContent ?? "null"));
    return {
      title: document.title,
      description: document.querySelector('meta[name="description"]')?.content ?? null,
      canonical: document.querySelector('link[rel="canonical"]')?.href ?? null,
      openGraphTitle: document.querySelector('meta[property="og:title"]')?.content ?? null,
      openGraphImage: document.querySelector('meta[property="og:image"]')?.content ?? null,
      twitterCard: document.querySelector('meta[name="twitter:card"]')?.content ?? null,
      imageCount: document.querySelectorAll('img[src*="/media/"]').length,
      loadedImageCount: [...document.querySelectorAll('img[src*="/media/"]')]
        .filter((image) => image.complete && image.naturalWidth > 0).length,
      structuredData,
    };
  });
  if (name === "home" || name === "category" || name === "recipe-pho-bo") {
    await page.screenshot({ path: join(evidenceDirectory, `${name}.png`), fullPage: true });
  }
  if (name.startsWith("recipe-")) {
    const recipe = metadata.structuredData.find((item) => item?.["@type"] === "Recipe");
    if (recipe) {
      const html = `<!doctype html><html lang="vi"><head><meta charset="utf-8"><script type="application/ld+json">${JSON.stringify(recipe).replace(/</g, "\\u003c")}</script></head><body></body></html>`;
      await writeFile(join(evidenceDirectory, `${name}-rich-results.html`), html);
    }
  }
  results.push({ name, path, status: response?.status() ?? null, ...metadata });
}

await writeFile(join(evidenceDirectory, "local-smoke.json"), `${JSON.stringify(results, null, 2)}\n`);
await browser.close();

for (const result of results) {
  console.log(`${result.name}: HTTP ${result.status}; title=${Boolean(result.title)}; canonical=${Boolean(result.canonical)}; og:image=${Boolean(result.openGraphImage)}; twitter=${result.twitterCard ?? "missing"}; loaded-images=${result.loadedImageCount}/${result.imageCount}`);
}
