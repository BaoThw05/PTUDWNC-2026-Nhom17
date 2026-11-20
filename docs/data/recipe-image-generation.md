# Ảnh mẫu cho 50 công thức

- `src/Backend/CulinaryBlog.Infrastructure/RecipeSearch/seed-images/recipe-01.jpg` đến `recipe-50.jpg` là ảnh món ăn được tạo riêng bằng Cloudflare Workers AI, model `@cf/black-forest-labs/flux-1-schnell`.
- Nội dung prompt tương ứng theo thứ tự khai báo món trong `RecipeDataSeeder.cs` nằm tại `recipe-image-prompts.jsonl`.
- Các ảnh là nội dung tổng hợp bằng AI, không lấy ảnh có sẵn từ website/photographer; vì vậy không có attribution ảnh stock.
- Seeder upload một ảnh JPEG riêng cho mỗi recipe Published, với key `seed/recipes/sample-dish-{NN}.jpg`.
- Để tạo lại ảnh, đặt `CLOUDFLARE_ACCOUNT_ID` và `CLOUDFLARE_API_TOKEN` trong môi trường, sau đó chạy `scripts/generate-cloudflare-recipe-images.ps1`. Script bỏ qua ảnh đã tồn tại để có thể tiếp tục sau khi bị ngắt.
