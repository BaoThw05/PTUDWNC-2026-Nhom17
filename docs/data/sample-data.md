# Dữ liệu mẫu tham khảo Vào Bếp

- Nguồn: [Điện máy XANH – Vào bếp](https://www.dienmayxanh.com/vao-bep), đối chiếu ngày 20/11/2026 qua chỉ mục công khai của trang.
- Trang gốc không phản hồi trong lần truy cập này; danh mục và tên bài dưới đây lấy từ kết quả chỉ mục tìm kiếm của URL trên.
- Chỉ dùng tên danh mục và tên món làm dữ liệu nhận diện. Mô tả, nguyên liệu, định lượng và hướng dẫn seed trong ứng dụng được biên soạn độc lập; không sao chép bài viết hoặc hình ảnh.

## Danh mục

Trang nguồn hiển thị các nhóm như: Món canh, Món bánh, Món chè, Món nước, Món kem, Món nướng, Món cháo, Món xào, Món kho, Món chiên, Món hấp, Món lẩu, Món gỏi - salad, Món từ gà, Món từ bò, Món chay, Ăn vặt, Ngày lễ Tết, Thức uống, Sinh tố, Trà sữa, Nước ép, Món tráng miệng, Món khô - mắm và Món cuốn - trộn.

Database giữ tám danh mục ban đầu của TV3 và bổ sung mười danh mục từ nhóm món trên trang nguồn: Món hấp, Món nướng, Món xào, Món kho, Món chiên, Món lẩu, Món cháo, Món chè, Gỏi - salad và Món cuốn - trộn. Tổng cộng 18 danh mục. Seeder dùng các category này trực tiếp, không rơi về nhóm dự phòng.

Recipe seeder tạo đúng 50 món, tất cả có nguyên liệu và định lượng cố định, bước chế biến, thời gian, khẩu phần và ảnh minh họa PNG do dự án tự sinh. Nutrition để trống vì chưa có số liệu đáng tin cậy cho từng món. Seed lặp sẽ chuẩn hóa recipe cũ, đồng thời xóa các recipe `sample-*` có index ngoài 1–50 để loại dữ liệu test cũ. Không xóa recipe do người dùng tạo.

## Tên công thức được đưa vào seed

Các tên sau lấy từ phần “Món ngon mới nhất”/các kết quả recipe công khai của trang nguồn và được dùng làm mẫu tên; nội dung recipe trong ứng dụng không phải bản chép công thức từ nguồn.

| Tên mẫu | Danh mục ứng dụng |
|---|---|
| Đậu bắp xào thịt bò | Món phụ |
| Chuối nếp nướng lá dứa | Bánh |
| Súp Tom Yum Thái Lan hải sản | Canh và súp |
| Chim câu xúc bánh đa | Món khai vị |
| Sinh tố dâu chuối | Đồ uống |
| Gỏi gà măng cụt | Món khai vị |
| Cá hấp nước tương | Món chính |
| Chả tôm thịt chiên | Món khai vị |
| Canh chua cá ngừ | Canh và súp |
| Sinh tố xoài chuối | Đồ uống |
| Cơm chiên muối ớt xanh | Món chính |
| Cơm chiên phô mai | Món chính |
| Bánh ram ít Huế | Bánh |
| Chè đậu xanh nha đam | Món chè |
| Dừa tắc | Đồ uống |
| Trà măng cụt chanh dây | Đồ uống |
| Canh cá nấu rau cần | Canh và súp |
| Canh rong biển nấu trứng | Canh và súp |
| Bánh từ sữa bột | Bánh |
| Bánh đậu xanh trà xanh | Bánh |
| Cánh gà chiên giòn | Món chiên |
| Bánh vừng bằng nồi chiên không dầu | Bánh |
| Mì xào giòn chay rau cải | Món xào |
| Thịt kho trám xanh | Món kho |
| Thịt băm kho trứng cút | Món kho |
| Cá trắm kho khế | Món kho |
| Cá kho dưa chua | Món kho |
| Bò kho truyền thống | Món kho |
| Cá chim kho nghệ | Món kho |
| Thịt kho nước dừa trứng cút | Món kho |
| Nấm kho nước tương | Món kho |
| Salad ngũ sắc | Gỏi - salad |
| Salad táo với tôm | Gỏi - salad |
| Salad rau củ trộn sốt mayonnaise | Gỏi - salad |
| Salad xà lách trộn mayonnaise | Gỏi - salad |
| Salad trứng | Gỏi - salad |
| Cá ngân nấu canh chua | Canh và súp |
| Canh chua cá lóc | Canh và súp |
| Phở bò | Món chính |
| Bánh cuốn | Món cuốn - trộn |
| Bún thang | Món chính |
| Bánh tráng nước dừa | Bánh |
| Bánh tráng dừa mè nướng | Bánh |
| Bánh đúc nước cốt dừa | Bánh |
| Bánh trôi nước | Món chè |
| Bánh bèo ngọt nước cốt dừa | Món chè |
| Bánh tráng kẹp Đà Nẵng | Bánh |
| Cơm cháy từ cơm nguội | Món chính |
| Cơm thập cẩm | Món chính |
| Cơm trộn rong biển | Món chay |
