import { expect, test } from "@playwright/test";

// Việc 2.23 (TV2): E2E Playwright luồng tạo recipe -> publish -> xem trang chi tiết.
test.describe("Luồng công thức nấu ăn (TV2 - Task 2.23)", () => {
  test("tạo công thức mới qua wizard -> xuất bản -> xem chi tiết công thức", async ({ page }) => {
    // 1. Truy cập trang Wizard tạo công thức
    await page.goto("/dashboard/recipes/new");

    // Bước 1: Thông tin cơ bản & dinh dưỡng
    const title = "Bún bò Huế gia truyền E2E";
    const description = "Món bún bò Huế đậm đà thơm ngon chuẩn vị cố đô.";

    await page.getByPlaceholder("Ví dụ: Bún chả Hà Nội nướng than hoa").fill(title);
    await page.getByPlaceholder("Giới thiệu đôi nét về hương vị, nguồn gốc món ăn...").fill(description);

    // Bấm Tiếp tục sang Bước 2
    await page.getByRole("button", { name: "Tiếp tục →" }).click();

    // Bước 2: Nguyên liệu
    await page.getByPlaceholder("Tên nguyên liệu (ví dụ: Thịt bò)").fill("Bắp bò hoa");
    await page.getByPlaceholder("Số lượng").fill("500");
    await page.getByPlaceholder("Đơn vị (g, ml, quả...)").fill("g");

    // Bấm Tiếp tục sang Bước 3
    await page.getByRole("button", { name: "Tiếp tục →" }).click();

    // Bước 3: Các bước thực hiện
    await page.getByPlaceholder("Mô tả chi tiết cách thực hiện bước này...").fill(
      "Hầm xương bò và bắp bò cùng sả trong 2 tiếng cho ngọt nước."
    );

    // Bấm Tiếp tục sang Bước 4 (Xem lại & Lưu bài)
    await page.getByRole("button", { name: "Tiếp tục →" }).click();

    // Bước 4: Kiểm tra thông tin hiển thị lại chính xác
    await expect(page.getByText(title)).toBeVisible();

    // Nhấn Xuất bản ngay
    await page.getByRole("button", { name: "Xuất bản ngay" }).click();

    // Sau khi xuất bản, wizard điều hướng về /dashboard/recipes
    await expect(page).toHaveURL(/\/dashboard\/recipes/);

    // 2. Chuyển sang xem trang chi tiết công khai theo slug
    // Slug được tạo tự động: "bun-bo-hue-gia-truyen-e2e"
    await page.goto("/recipes/bun-bo-hue-gia-truyen-e2e");

    // Kiểm tra trang chi tiết hiển thị đầy đủ tiêu đề, mô tả và nguyên liệu, bước làm
    await expect(page.getByRole("heading", { level: 1, name: title })).toBeVisible();
    await expect(page.getByText(description)).toBeVisible();
    await expect(page.getByText("Bắp bò hoa")).toBeVisible();
    await expect(page.getByText("Hầm xương bò và bắp bò cùng sả trong 2 tiếng cho ngọt nước.")).toBeVisible();
  });
});
