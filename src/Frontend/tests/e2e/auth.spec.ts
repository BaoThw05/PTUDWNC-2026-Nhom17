import { expect, test } from "@playwright/test";

// Việc 1.22 (TV1): luồng đăng ký + đăng nhập trên giao diện, backend là mock-api.mjs.
const account = { email: "e2e@culinaryblog.test", password: "E2e@12345", fullName: "Người Dùng E2E" };

test("đăng nhập đúng thì về trang được yêu cầu và hiện tên người dùng", async ({ page }) => {
  await page.goto("/auth/login?callbackUrl=%2F");

  await page.getByLabel("Email").fill(account.email);
  await page.getByLabel("Mật khẩu").fill(account.password);
  await page.getByRole("button", { name: "Đăng nhập" }).click();

  await expect(page).toHaveURL(/\/$/);
  await expect(page.getByRole("link", { name: account.fullName })).toBeVisible();
});

test("đăng nhập sai mật khẩu thì hiện thông báo lỗi chung", async ({ page }) => {
  await page.goto("/auth/login");

  await page.getByLabel("Email").fill(account.email);
  await page.getByLabel("Mật khẩu").fill("Sai@12345");
  await page.getByRole("button", { name: "Đăng nhập" }).click();

  await expect(page.getByText("Email hoặc mật khẩu không đúng.")).toBeVisible();
  await expect(page).toHaveURL(/\/auth\/login/);
});

test("đăng ký xong tự đăng nhập", async ({ page }) => {
  const suffix = Date.now().toString(36);
  await page.goto("/auth/register?callbackUrl=%2F");

  await page.getByLabel("Họ tên").fill("Người Mới");
  await page.getByLabel("Email").fill(`new-${suffix}@culinaryblog.test`);
  await page.getByLabel("Tên đăng nhập").fill(`new${suffix}`);
  await page.getByLabel("Mật khẩu", { exact: true }).fill("Moi@12345");
  await page.getByLabel("Nhập lại mật khẩu").fill("Moi@12345");
  await page.getByRole("button", { name: "Đăng ký" }).click();

  await expect(page).toHaveURL(/\/$/);
  await expect(page.getByRole("link", { name: "Người Mới" })).toBeVisible();
});

test("đăng ký bằng email đã có thì báo lỗi cạnh ô email", async ({ page }) => {
  await page.goto("/auth/register");

  await page.getByLabel("Họ tên").fill("Trùng Email");
  await page.getByLabel("Email").fill(account.email);
  await page.getByLabel("Tên đăng nhập").fill("trungemail");
  await page.getByLabel("Mật khẩu", { exact: true }).fill("Moi@12345");
  await page.getByLabel("Nhập lại mật khẩu").fill("Moi@12345");
  await page.getByRole("button", { name: "Đăng ký" }).click();

  await expect(page.getByText("Email này đã được đăng ký.")).toBeVisible();
});
