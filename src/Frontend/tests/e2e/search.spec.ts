import { expect, test } from "@playwright/test";

test("searches published recipes and keeps the query in the URL", async ({ page }) => {
  await page.goto("/search");

  const searchBox = page.getByRole("searchbox", { name: "Tìm công thức" });
  await searchBox.fill("pho bo");
  await page.getByRole("button", { name: "Tìm", exact: true }).click();

  await expect(page).toHaveURL(/\/search\?q=pho\+bo$/);
  await expect(page.getByRole("heading", { name: "Phở bò" })).toBeVisible();
});

test("shows suggestions when a search has no matching recipes", async ({ page }) => {
  await page.goto("/search?q=mon-khong-co");

  await expect(page.getByRole("heading", { name: "Chưa tìm thấy công thức phù hợp" })).toBeVisible();
  await expect(page.getByRole("link", { name: "pho bo" })).toHaveAttribute("href", "/search?q=pho%20bo");
});
