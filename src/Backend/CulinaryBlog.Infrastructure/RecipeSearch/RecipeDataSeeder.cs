using Bogus;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.RecipeSearch;

internal sealed class RecipeDataSeeder(
    AppDbContext db,
    IFileStorage fileStorage,
    UserManager<ApplicationUser> userManager,
    IOptions<AuthSeedOptions> authSeedOptions,
    IOptions<RecipeSeedOptions> recipeSeedOptions,
    TimeProvider timeProvider,
    ILogger<RecipeDataSeeder> logger) : IDataSeeder
{
    private const int MaximumRecipeCount = 10_000;

    private static readonly string[] DescriptionOpenings =
    [
        "Công thức dễ làm cho bữa cơm gia đình",
        "Món ngon đậm đà hương vị Việt",
        "Gợi ý hấp dẫn cho thực đơn cuối tuần",
        "Cách nấu tròn vị với nguyên liệu quen thuộc",
        "Món ăn thơm ngon, phù hợp để thưởng thức cùng gia đình"
    ];

    private static readonly string[] DescriptionDetails =
    [
        "có thể gia giảm gia vị theo khẩu vị",
        "nên chọn nguyên liệu tươi để món ăn giữ được hương vị tự nhiên",
        "thưởng thức ngay khi còn nóng để cảm nhận trọn vị",
        "chuẩn bị sẵn các nguyên liệu để quá trình chế biến thuận tiện hơn",
        "dùng kèm rau thơm và nước chấm phù hợp"
    ];

    private static readonly RecipeDish[] Dishes =
    [
        new("Phở bò", "mon-chinh", RecipeStyle.PhoBo),
        new("Phở gà", "mon-chinh", RecipeStyle.PhoGa),
        new("Bún bò Huế", "mon-chinh", RecipeStyle.BunBo),
        new("Bún riêu cua", "mon-chinh", RecipeStyle.BunRieu),
        new("Bún chả Hà Nội", "mon-chinh", RecipeStyle.RiceMeat),
        new("Bún thịt nướng", "mon-chinh", RecipeStyle.RiceMeat),
        new("Mì Quảng", "mon-chinh", RecipeStyle.SeafoodNoodles),
        new("Hủ tiếu Nam Vang", "mon-chinh", RecipeStyle.SeafoodNoodles),
        new("Bánh canh cua", "mon-chinh", RecipeStyle.BunRieu),
        new("Miến gà", "mon-chinh", RecipeStyle.PhoGa),
        new("Cơm tấm sườn nướng", "mon-chinh", RecipeStyle.RiceMeat),
        new("Cơm gà Hội An", "mon-chinh", RecipeStyle.RiceMeat),
        new("Cá kho tộ", "mon-chinh", RecipeStyle.Fish),
        new("Thịt kho trứng", "mon-chinh", RecipeStyle.RiceMeat),
        new("Bò lúc lắc", "mon-chinh", RecipeStyle.RiceMeat),
        new("Gà kho gừng", "mon-chinh", RecipeStyle.Chicken),
        new("Gà nướng mật ong", "mon-chinh", RecipeStyle.Chicken),
        new("Sườn xào chua ngọt", "mon-chinh", RecipeStyle.RiceMeat),
        new("Bò kho", "mon-chinh", RecipeStyle.RiceMeat),
        new("Cá basa kho tiêu", "mon-chinh", RecipeStyle.Fish),
        new("Canh chua cá lóc", "canh-va-sup", RecipeStyle.Soup),
        new("Canh bí đỏ nấu tôm", "canh-va-sup", RecipeStyle.Soup),
        new("Canh rau ngót thịt bằm", "canh-va-sup", RecipeStyle.Soup),
        new("Cháo gà", "canh-va-sup", RecipeStyle.PhoGa),
        new("Lẩu Thái", "canh-va-sup", RecipeStyle.SeafoodNoodles),
        new("Gỏi cuốn tôm thịt", "mon-khai-vi", RecipeStyle.Appetizer),
        new("Nem rán", "mon-khai-vi", RecipeStyle.Appetizer),
        new("Gỏi ngó sen tôm thịt", "mon-khai-vi", RecipeStyle.Appetizer),
        new("Gỏi xoài xanh", "mon-khai-vi", RecipeStyle.Appetizer),
        new("Bánh xèo miền Tây", "mon-khai-vi", RecipeStyle.Appetizer),
        new("Rau muống xào tỏi", "mon-phu", RecipeStyle.Vegetable),
        new("Đậu hũ sốt cà chua", "mon-chay", RecipeStyle.Vegetable),
        new("Nấm kho tiêu", "mon-chay", RecipeStyle.Vegetable),
        new("Cà tím nướng mỡ hành", "mon-phu", RecipeStyle.Vegetable),
        new("Đậu que xào tỏi", "mon-chay", RecipeStyle.Vegetable),
        new("Chè ba màu", "mon-trang-mieng", RecipeStyle.Dessert),
        new("Chè đậu xanh", "mon-trang-mieng", RecipeStyle.Dessert),
        new("Sữa chua nếp cẩm", "mon-trang-mieng", RecipeStyle.Dessert),
        new("Bánh flan", "mon-trang-mieng", RecipeStyle.Dessert),
        new("Chè khúc bạch", "mon-trang-mieng", RecipeStyle.Dessert),
        new("Trà tắc", "do-uong", RecipeStyle.Drink),
        new("Sinh tố bơ", "do-uong", RecipeStyle.Drink),
        new("Nước sâm", "do-uong", RecipeStyle.Drink),
        new("Bánh mì Việt Nam", "banh", RecipeStyle.Baked),
        new("Bánh da lợn", "banh", RecipeStyle.Baked),
        new("Bánh bò nướng", "banh", RecipeStyle.Baked),
        new("Bánh chuối hấp", "banh", RecipeStyle.Baked),
        new("Xôi gà", "mon-chinh", RecipeStyle.RiceMeat),
        new("Trứng chiên thịt", "mon-phu", RecipeStyle.RiceMeat),
        new("Bắp xào tép", "mon-phu", RecipeStyle.Vegetable)
    ];

    private static readonly IReadOnlyDictionary<RecipeStyle, RecipeProfile> Profiles =
        new Dictionary<RecipeStyle, RecipeProfile>
        {
            [RecipeStyle.PhoBo] = Profile(
                [
                    Ingredient("Bánh phở", "gói", 1, 2),
                    Ingredient("Thịt bò nạm", "g", 250, 400),
                    Ingredient("Xương ống bò", "g", 500, 800),
                    Ingredient("Hành tây", "củ", 1, 2),
                    Ingredient("Gừng", "g", 20, 40)
                ],
                [
                    Ingredient("Hoa hồi", "cánh", 2, 4),
                    Ingredient("Quế thanh", "thanh", 1, 2),
                    Ingredient("Thảo quả", "quả", 1, 2),
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Rau húng quế", "nắm", 1, 2),
                    Ingredient("Giá đỗ", "g", 100, 200),
                    Ingredient("Nước mắm", "ml", 15, 30)
                ],
                [
                    Step("Ninh nước dùng", "Nướng thơm gừng và hành tây, chần sạch xương rồi ninh với gia vị hồi quế. Hớt bọt thường xuyên để nước dùng trong.", 90, 150),
                    Step("Sơ chế thịt bò", "Rửa sạch thịt bò, thái lát mỏng vừa ăn; rửa rau thơm và giá đỗ, để ráo.", 10, 20),
                    Step("Chuẩn bị bánh phở", "Trụng bánh phở qua nước sôi, chia vào tô cùng hành lá và rau thơm.", 3, 6),
                    Step("Hoàn thành món phở", "Xếp thịt bò lên trên, chan nước dùng đang sôi và nêm nếm lại trước khi dùng.", 5, 10)
                ],
                350, 750, 20, 55),
            [RecipeStyle.PhoGa] = Profile(
                [
                    Ingredient("Thịt gà ta", "g", 500, 800),
                    Ingredient("Bánh phở", "gói", 1, 2),
                    Ingredient("Xương gà", "g", 300, 500),
                    Ingredient("Hành tây", "củ", 1, 2),
                    Ingredient("Gừng", "g", 20, 40)
                ],
                [
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Rau mùi", "nắm", 1, 2),
                    Ingredient("Lá chanh", "lá", 3, 5),
                    Ingredient("Giá đỗ", "g", 100, 200),
                    Ingredient("Nước mắm", "ml", 15, 30),
                    Ingredient("Muối", "thìa cà phê", 1, 2)
                ],
                [
                    Step("Sơ chế gà", "Làm sạch gà và xương, chà nhẹ với muối rồi rửa lại. Nướng thơm hành tây và gừng.", 10, 20),
                    Step("Nấu nước dùng", "Cho gà, xương, hành và gừng vào nồi nước, nấu nhỏ lửa đến khi gà chín mềm. Vớt gà ra trước để thịt không bị bở.", 40, 70),
                    Step("Chuẩn bị thịt và bánh phở", "Để gà nguội bớt rồi xé hoặc chặt miếng vừa ăn; trụng bánh phở và xếp vào tô.", 10, 15),
                    Step("Hoàn thành món phở", "Nêm nước dùng với nước mắm, chan vào tô và thêm hành lá, rau mùi cùng lá chanh.", 5, 10)
                ],
                300, 680, 20, 50),
            [RecipeStyle.BunBo] = Profile(
                [
                    Ingredient("Bún tươi", "g", 400, 600),
                    Ingredient("Bắp bò", "g", 250, 400),
                    Ingredient("Giò heo", "g", 300, 500),
                    Ingredient("Sả", "cây", 3, 6),
                    Ingredient("Xương ống", "g", 500, 800)
                ],
                [
                    Ingredient("Mắm ruốc", "thìa canh", 1, 2),
                    Ingredient("Ớt", "quả", 1, 3),
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Bắp chuối", "g", 100, 200),
                    Ingredient("Rau thơm", "nắm", 1, 2),
                    Ingredient("Dầu điều", "ml", 10, 20)
                ],
                [
                    Step("Sơ chế thịt và xương", "Chần xương và giò heo qua nước sôi, rửa sạch. Buộc gọn bắp bò để thịt giữ hình dáng khi nấu.", 15, 25),
                    Step("Nấu nước dùng", "Ninh xương cùng sả đập dập, cho bắp bò và giò heo vào nấu đến khi mềm rồi vớt ra thái lát.", 60, 100),
                    Step("Nêm nước lèo", "Hòa mắm ruốc với nước, lọc bỏ cặn rồi cho vào nồi. Thêm dầu điều và điều chỉnh gia vị vừa ăn.", 10, 15),
                    Step("Trình bày tô bún", "Trụng bún, xếp thịt và giò lên trên, chan nước lèo nóng rồi dùng cùng rau sống.", 5, 10)
                ],
                400, 800, 22, 60),
            [RecipeStyle.BunRieu] = Profile(
                [
                    Ingredient("Bún tươi", "g", 400, 600),
                    Ingredient("Cua đồng xay", "g", 250, 400),
                    Ingredient("Cà chua", "quả", 3, 5),
                    Ingredient("Đậu hũ", "miếng", 2, 4),
                    Ingredient("Hành tím", "củ", 2, 4)
                ],
                [
                    Ingredient("Trứng gà", "quả", 1, 2),
                    Ingredient("Mắm tôm", "thìa cà phê", 1, 2),
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Rau muống bào", "g", 100, 200),
                    Ingredient("Giấm bỗng", "ml", 15, 30),
                    Ingredient("Nước mắm", "ml", 15, 30)
                ],
                [
                    Step("Lọc cua", "Hòa cua đồng xay với nước, bóp đều rồi lọc qua rây để lấy phần nước cua mịn.", 10, 15),
                    Step("Nấu riêu cua", "Đun nước cua ở lửa vừa, khuấy nhẹ đến khi riêu nổi thành mảng thì vớt ra để riêng.", 15, 25),
                    Step("Nấu nước dùng", "Phi thơm hành tím, xào cà chua và đậu hũ rồi cho vào nồi. Nêm mắm tôm, giấm bỗng và nước mắm.", 15, 25),
                    Step("Hoàn thành món bún", "Trụng bún, chan nước dùng cùng riêu cua, thêm hành lá và dùng với rau sống.", 5, 10)
                ],
                320, 700, 15, 40),
            [RecipeStyle.SeafoodNoodles] = Profile(
                [
                    Ingredient("Mì Quảng hoặc hủ tiếu", "g", 400, 600),
                    Ingredient("Tôm tươi", "g", 200, 350),
                    Ingredient("Thịt heo", "g", 200, 350),
                    Ingredient("Hành tím", "củ", 2, 4),
                    Ingredient("Nước dùng", "ml", 800, 1200)
                ],
                [
                    Ingredient("Trứng cút", "quả", 4, 8),
                    Ingredient("Đậu phộng rang", "g", 30, 60),
                    Ingredient("Bánh tráng nướng", "miếng", 1, 2),
                    Ingredient("Rau thơm", "nắm", 1, 2),
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Nước mắm", "ml", 15, 30)
                ],
                [
                    Step("Sơ chế nguyên liệu", "Rửa sạch tôm và thịt, cắt miếng vừa ăn. Rửa rau thơm, để ráo và chuẩn bị các loại đồ ăn kèm.", 10, 20),
                    Step("Xào phần nhân", "Phi thơm hành tím, cho thịt và tôm vào xào săn, nêm nước mắm và gia vị vừa ăn.", 10, 15),
                    Step("Nấu nước dùng", "Thêm nước dùng vào nồi nhân, nấu nhỏ lửa để các nguyên liệu thấm vị. Có thể cho trứng cút vào ở bước này.", 15, 25),
                    Step("Hoàn thành tô mì", "Trụng sợi mì, xếp nhân và rau thơm lên trên, chan nước dùng rồi rắc đậu phộng rang.", 5, 10)
                ],
                350, 780, 18, 45),
            [RecipeStyle.RiceMeat] = Profile(
                [
                    Ingredient("Gạo tẻ", "g", 250, 400),
                    Ingredient("Thịt heo hoặc thịt gà", "g", 250, 450),
                    Ingredient("Tỏi", "tép", 3, 6),
                    Ingredient("Hành tím", "củ", 2, 4),
                    Ingredient("Nước mắm", "ml", 15, 30)
                ],
                [
                    Ingredient("Trứng gà", "quả", 1, 3),
                    Ingredient("Dưa leo", "quả", 1, 2),
                    Ingredient("Cà chua", "quả", 1, 2),
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Đường", "g", 10, 25),
                    Ingredient("Tiêu xay", "g", 2, 5)
                ],
                [
                    Step("Sơ chế và ướp thịt", "Rửa sạch thịt, cắt miếng vừa ăn rồi ướp với hành tím, tỏi, nước mắm và tiêu trong khoảng 15 phút.", 15, 25),
                    Step("Nấu cơm", "Vo gạo nhẹ tay, thêm lượng nước phù hợp và nấu đến khi cơm chín dẻo.", 25, 40),
                    Step("Chế biến món mặn", "Làm nóng chảo hoặc nồi, cho thịt đã ướp vào chế biến đến khi chín mềm và thấm gia vị.", 15, 30),
                    Step("Hoàn thành món ăn", "Dọn cơm cùng phần thịt, rau ăn kèm và nước mắm pha; rắc hành lá trước khi dùng.", 5, 10)
                ],
                320, 850, 15, 55),
            [RecipeStyle.Fish] = Profile(
                [
                    Ingredient("Cá basa hoặc cá lóc", "g", 400, 700),
                    Ingredient("Nước mắm", "ml", 20, 35),
                    Ingredient("Hành tím", "củ", 2, 4),
                    Ingredient("Tỏi", "tép", 3, 5),
                    Ingredient("Tiêu xay", "g", 2, 5)
                ],
                [
                    Ingredient("Thịt ba chỉ", "g", 100, 200),
                    Ingredient("Nước dừa tươi", "ml", 150, 300),
                    Ingredient("Ớt", "quả", 1, 3),
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Đường", "g", 10, 20),
                    Ingredient("Nước màu", "ml", 5, 10)
                ],
                [
                    Step("Sơ chế cá", "Làm sạch cá, cắt khúc vừa ăn, thấm khô rồi ướp với nước mắm, tiêu và hành tỏi băm.", 15, 25),
                    Step("Thắng nước màu", "Đun đường ở lửa nhỏ đến khi chuyển màu cánh gián, thêm chút nước nóng để tạo nước màu.", 5, 10),
                    Step("Kho cá", "Xếp cá vào nồi, thêm nước màu và nước dừa. Kho nhỏ lửa đến khi cá chín và nước sốt sánh lại.", 30, 50),
                    Step("Nêm nếm và dọn món", "Nêm lại gia vị, thêm ớt và hành lá rồi tắt bếp. Dùng nóng với cơm trắng.", 5, 10)
                ],
                250, 650, 18, 45),
            [RecipeStyle.Chicken] = Profile(
                [
                    Ingredient("Thịt gà", "g", 500, 800),
                    Ingredient("Gừng", "g", 20, 40),
                    Ingredient("Tỏi", "tép", 3, 6),
                    Ingredient("Nước mắm", "ml", 15, 30),
                    Ingredient("Hành tím", "củ", 2, 4)
                ],
                [
                    Ingredient("Mật ong", "thìa canh", 1, 3),
                    Ingredient("Sả", "cây", 2, 4),
                    Ingredient("Ớt", "quả", 1, 2),
                    Ingredient("Hành lá", "nhánh", 3, 5),
                    Ingredient("Tiêu xay", "g", 2, 5),
                    Ingredient("Dầu ăn", "ml", 10, 20)
                ],
                [
                    Step("Sơ chế gà", "Rửa sạch gà, để ráo rồi chặt miếng vừa ăn. Băm nhỏ gừng, tỏi và hành tím.", 10, 15),
                    Step("Ướp thịt gà", "Trộn gà với nước mắm, gừng, tỏi và các gia vị; để thấm trong khoảng 20 phút.", 20, 30),
                    Step("Nấu gà", "Áp chảo hoặc kho gà ở lửa vừa đến khi thịt săn, sau đó hạ lửa và nấu chín đều.", 25, 40),
                    Step("Hoàn thiện món ăn", "Nêm nếm lại, thêm hành lá và trình bày ra đĩa. Dùng nóng cùng cơm.", 5, 10)
                ],
                280, 720, 20, 55),
            [RecipeStyle.Soup] = Profile(
                [
                    Ingredient("Cá lóc hoặc tôm tươi", "g", 250, 400),
                    Ingredient("Cà chua", "quả", 2, 4),
                    Ingredient("Đậu bắp", "g", 100, 200),
                    Ingredient("Thơm", "miếng", 2, 4),
                    Ingredient("Me chua", "g", 20, 40)
                ],
                [
                    Ingredient("Bạc hà", "cây", 1, 2),
                    Ingredient("Giá đỗ", "g", 100, 200),
                    Ingredient("Rau om", "nắm", 1, 2),
                    Ingredient("Ngò gai", "nắm", 1, 2),
                    Ingredient("Nước mắm", "ml", 15, 30),
                    Ingredient("Đường", "g", 10, 25)
                ],
                [
                    Step("Sơ chế nguyên liệu", "Làm sạch cá hoặc tôm, cắt khúc vừa ăn. Rửa rau củ, cắt cà chua, thơm và đậu bắp.", 10, 20),
                    Step("Nấu nước me", "Đun me với nước nóng, dằm nhẹ rồi lọc lấy nước cốt. Đun sôi nước dùng và nêm vị chua ngọt vừa ăn.", 10, 15),
                    Step("Nấu canh", "Cho cá hoặc tôm vào nồi, nấu chín rồi lần lượt thêm thơm, cà chua, đậu bắp và các loại rau củ.", 15, 25),
                    Step("Hoàn thiện nồi canh", "Nêm nước mắm, thêm rau om và ngò gai rồi tắt bếp. Dùng nóng với cơm.", 5, 10)
                ],
                150, 420, 10, 35),
            [RecipeStyle.Appetizer] = Profile(
                [
                    Ingredient("Tôm tươi", "g", 150, 250),
                    Ingredient("Thịt heo", "g", 150, 250),
                    Ingredient("Rau xà lách", "lá", 6, 12),
                    Ingredient("Bánh tráng", "cái", 6, 12),
                    Ingredient("Rau thơm", "nắm", 1, 2)
                ],
                [
                    Ingredient("Bún tươi", "g", 100, 200),
                    Ingredient("Cà rốt", "củ", 1, 2),
                    Ingredient("Đậu phộng rang", "g", 20, 40),
                    Ingredient("Nước mắm", "ml", 15, 30),
                    Ingredient("Đường", "g", 10, 20),
                    Ingredient("Tỏi", "tép", 2, 4)
                ],
                [
                    Step("Sơ chế nguyên liệu", "Rửa sạch tôm, thịt và rau sống. Gọt vỏ, bào sợi cà rốt nếu dùng; chuẩn bị nước chấm.", 10, 20),
                    Step("Luộc phần nhân", "Luộc chín tôm và thịt với chút muối, vớt ra để nguội rồi thái hoặc bóc vỏ tùy món.", 10, 20),
                    Step("Chuẩn bị rau và bánh tráng", "Trụng bún nếu cần, để ráo. Làm mềm bánh tráng vừa đủ để cuốn hoặc chuẩn bị nhân để chiên.", 5, 10),
                    Step("Cuốn hoặc chiên món ăn", "Xếp nhân và rau vào bánh tráng, cuốn chặt tay; nếu là món chiên thì chiên vàng đều trước khi dùng.", 10, 20)
                ],
                180, 500, 8, 30),
            [RecipeStyle.Vegetable] = Profile(
                [
                    Ingredient("Rau củ theo mùa", "g", 300, 500),
                    Ingredient("Tỏi", "tép", 3, 6),
                    Ingredient("Đậu hũ hoặc nấm", "g", 150, 300),
                    Ingredient("Hành lá", "nhánh", 2, 4),
                    Ingredient("Dầu ăn", "ml", 10, 20)
                ],
                [
                    Ingredient("Nước tương", "ml", 15, 30),
                    Ingredient("Nước mắm chay", "ml", 10, 20),
                    Ingredient("Tiêu xay", "g", 2, 5),
                    Ingredient("Ớt", "quả", 1, 2),
                    Ingredient("Đường", "g", 5, 15),
                    Ingredient("Đậu phộng rang", "g", 20, 40)
                ],
                [
                    Step("Sơ chế rau củ", "Rửa sạch rau củ, cắt miếng vừa ăn. Băm tỏi và chuẩn bị gia vị.", 10, 15),
                    Step("Phi thơm gia vị", "Làm nóng dầu, phi tỏi đến khi dậy mùi; cho nấm hoặc đậu hũ vào đảo nhẹ.", 5, 10),
                    Step("Chế biến rau củ", "Thêm rau củ vào chảo, đảo nhanh trên lửa vừa để rau chín tới mà vẫn giữ màu tươi.", 8, 15),
                    Step("Nêm nếm và trình bày", "Thêm nước tương hoặc nước mắm chay, nêm tiêu vừa ăn rồi dọn ra đĩa.", 3, 5)
                ],
                100, 420, 4, 25),
            [RecipeStyle.Dessert] = Profile(
                [
                    Ingredient("Đậu xanh hoặc nếp cẩm", "g", 150, 250),
                    Ingredient("Nước cốt dừa", "ml", 150, 300),
                    Ingredient("Đường", "g", 50, 100),
                    Ingredient("Sữa tươi", "ml", 200, 400),
                    Ingredient("Bột rau câu", "g", 5, 10)
                ],
                [
                    Ingredient("Lá dứa", "lá", 2, 4),
                    Ingredient("Đậu đỏ", "g", 80, 150),
                    Ingredient("Trân châu", "g", 50, 100),
                    Ingredient("Vanilla", "ml", 2, 5),
                    Ingredient("Muối", "thìa cà phê", 1, 2),
                    Ingredient("Đá viên", "g", 100, 200)
                ],
                [
                    Step("Sơ chế nguyên liệu", "Vo sạch đậu hoặc nếp, ngâm mềm nếu cần. Chuẩn bị lá dứa và các nguyên liệu ăn kèm.", 10, 20),
                    Step("Nấu phần chè", "Nấu đậu hoặc nếp với nước đến khi mềm, thêm đường và chút muối để cân bằng vị.", 25, 45),
                    Step("Chuẩn bị nước cốt dừa", "Đun nước cốt dừa với lá dứa ở lửa nhỏ, khuấy đều và tắt bếp khi vừa sánh.", 8, 15),
                    Step("Hoàn thành món chè", "Múc chè ra ly hoặc chén, thêm nước cốt dừa và topping. Làm lạnh hoặc dùng cùng đá.", 5, 10)
                ],
                180, 520, 3, 15),
            [RecipeStyle.Drink] = Profile(
                [
                    Ingredient("Trái cây tươi", "g", 200, 400),
                    Ingredient("Nước lọc", "ml", 300, 600),
                    Ingredient("Đường", "g", 20, 60),
                    Ingredient("Chanh hoặc tắc", "quả", 2, 5),
                    Ingredient("Đá viên", "g", 100, 250)
                ],
                [
                    Ingredient("Sữa đặc", "ml", 15, 40),
                    Ingredient("Lá dứa", "lá", 2, 4),
                    Ingredient("Gừng", "g", 10, 20),
                    Ingredient("Hạt chia", "g", 5, 10),
                    Ingredient("Muối", "g", 1, 3),
                    Ingredient("Mật ong", "ml", 10, 20)
                ],
                [
                    Step("Sơ chế nguyên liệu", "Rửa sạch trái cây và các nguyên liệu. Gọt vỏ hoặc bỏ hạt nếu cần, vắt lấy nước cốt.", 5, 10),
                    Step("Pha hoặc xay thức uống", "Cho nguyên liệu vào máy xay hoặc bình pha, thêm nước và đường theo khẩu vị.", 3, 8),
                    Step("Điều chỉnh hương vị", "Khuấy hoặc xay đến khi hỗn hợp hòa quyện, nếm thử và điều chỉnh độ ngọt.", 2, 5),
                    Step("Hoàn thành ly nước", "Rót ra ly, thêm đá viên và trang trí bằng lát trái cây hoặc lá thơm.", 2, 5)
                ],
                30, 320, 0, 8),
            [RecipeStyle.Baked] = Profile(
                [
                    Ingredient("Bột mì hoặc bột gạo", "g", 200, 400),
                    Ingredient("Trứng gà", "quả", 2, 4),
                    Ingredient("Nước cốt dừa", "ml", 100, 250),
                    Ingredient("Đường", "g", 40, 100),
                    Ingredient("Bột nở", "g", 3, 8)
                ],
                [
                    Ingredient("Chuối chín", "quả", 2, 4),
                    Ingredient("Lá dứa", "lá", 2, 4),
                    Ingredient("Bơ lạt", "g", 30, 60),
                    Ingredient("Sữa tươi", "ml", 100, 200),
                    Ingredient("Muối", "g", 1, 3),
                    Ingredient("Mè rang", "g", 10, 20)
                ],
                [
                    Step("Chuẩn bị nguyên liệu", "Rây bột, làm nóng lò hoặc chuẩn bị xửng hấp. Đánh tan trứng và chuẩn bị khuôn.", 10, 15),
                    Step("Trộn hỗn hợp bột", "Trộn bột với đường và các nguyên liệu lỏng đến khi hỗn hợp mịn, không còn vón cục.", 8, 12),
                    Step("Nướng hoặc hấp bánh", "Đổ bột vào khuôn đã chuẩn bị, nướng hoặc hấp đến khi bánh chín và có mùi thơm.", 25, 50),
                    Step("Để nguội và thưởng thức", "Lấy bánh ra khỏi khuôn, để nguội bớt rồi cắt miếng vừa ăn.", 10, 20)
                ],
                180, 600, 4, 18)
        };

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var targetCount = recipeSeedOptions.Value.Count;
        if (targetCount is < 1 or > MaximumRecipeCount)
        {
            throw new InvalidOperationException(
                $"Seed:Recipes:Count must be between 1 and {MaximumRecipeCount}.");
        }

        var authors = await EnsureSampleAuthorsAsync(cancellationToken);
        if (authors.Count == 0)
        {
            logger.LogWarning("Recipe seed needs author accounts; configure Seed:Auth:AuthorPassword");
            return;
        }

        var categories = await db.Categories
            .OrderBy(category => category.OrderIndex)
            .ToListAsync(cancellationToken);
        if (categories.Count == 0)
        {
            logger.LogWarning("Recipe seed needs at least one category; category seed was not applied");
            return;
        }

        var faker = new Faker();
        var now = timeProvider.GetUtcNow();
        var existingRecipes = await db.Recipes.IgnoreQueryFilters()
            .Where(recipe => recipe.Slug.StartsWith("sample-"))
            .Include(recipe => recipe.Steps)
            .Include(recipe => recipe.Ingredients)
            .ToListAsync(cancellationToken);
        var recipesByIndex = existingRecipes
            .Select(recipe => (Recipe: recipe, Index: GetSampleIndex(recipe.Slug)))
            .Where(item => item.Index.HasValue && item.Index.Value <= targetCount)
            .GroupBy(item => item.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First().Recipe);
        var categoriesBySlug = categories.ToDictionary(category => category.Slug, StringComparer.OrdinalIgnoreCase);
        var recipes = new Recipe[targetCount];
        var newRecipes = new List<Recipe>();

        for (var index = 0; index < recipes.Length; index++)
        {
            var dish = Dishes[index % Dishes.Length];
            var category = categoriesBySlug.TryGetValue(dish.CategorySlug, out var matchingCategory)
                ? matchingCategory
                : categories[index % categories.Count];

            if (!recipesByIndex.TryGetValue(index + 1, out var recipe))
            {
                recipe = CreateRecipe(index, dish, authors[index % authors.Count], category.Id, faker, now);
                newRecipes.Add(recipe);
            }
            else
            {
                recipe.Title = dish.Name;
                recipe.Slug = $"sample-{index + 1:00}-{SlugHelper.GenerateSlug(dish.Name)}";
                recipe.CategoryId = category.Id;
                EnsureRecipeDetails(recipe, dish, faker, now);
            }

            recipes[index] = recipe;
        }

        await db.Recipes.AddRangeAsync(newRecipes, cancellationToken);

        var sampleRecipeIds = recipes.Select(recipe => recipe.Id).ToArray();
        var recipesWithPrimaryImage = await db.RecipeImages.IgnoreQueryFilters()
            .Where(image => sampleRecipeIds.Contains(image.RecipeId) && image.IsPrimary)
            .Select(image => image.RecipeId)
            .ToHashSetAsync(cancellationToken);
        var imageIndexes = Enumerable.Range(0, recipes.Length)
            .Where(index => !recipesWithPrimaryImage.Contains(recipes[index].Id))
            .ToArray();
        var availableArtwork = new HashSet<int>();

        foreach (var variant in imageIndexes.Select(index => index % 3).Distinct())
        {
            var key = SampleImageKey(variant);
            try
            {
                await using var stream = new MemoryStream(SampleRecipeArtwork.CreatePng(variant));
                await fileStorage.UploadAsync(stream, key, "image/png", cancellationToken);
                availableArtwork.Add(variant);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Could not upload sample recipe artwork {StorageKey}", key);
            }
        }

        foreach (var index in imageIndexes)
        {
            var variant = index % 3;
            if (!availableArtwork.Contains(variant))
            {
                continue;
            }

            var recipe = recipes[index];
            db.RecipeImages.Add(new RecipeImage
            {
                Id = Guid.CreateVersion7(),
                RecipeId = recipe.Id,
                OriginalKey = SampleImageKey(variant),
                ThumbnailKey = SampleImageKey(variant),
                AltText = $"Ảnh minh họa món {recipe.Title}",
                IsPrimary = true,
                OrderIndex = 0,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Ensured {RecipeCount} sample recipes for {AuthorCount} authors and {CategoryCount} categories",
            recipes.Length,
            authors.Count,
            categories.Count);
    }

    private async Task<IReadOnlyList<ApplicationUser>> EnsureSampleAuthorsAsync(CancellationToken cancellationToken)
    {
        var seedOptions = authSeedOptions.Value;
        var emails = seedOptions.AuthorEmails.Take(5).ToList();
        while (emails.Count < 5)
        {
            emails.Add($"author{emails.Count + 1}@culinaryblog.test");
        }

        var authors = new List<ApplicationUser>(5);
        foreach (var email in emails.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var author = await userManager.FindByEmailAsync(email);
            if (author is null)
            {
                if (string.IsNullOrWhiteSpace(seedOptions.AuthorPassword))
                {
                    logger.LogWarning("Cannot seed author {Email} because Seed:Auth:AuthorPassword is not configured", email);
                    continue;
                }

                author = new ApplicationUser
                {
                    Id = Guid.CreateVersion7(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = $"Sample Author {authors.Count + 1}",
                    CreatedAt = timeProvider.GetUtcNow()
                };

                var createResult = await userManager.CreateAsync(author, seedOptions.AuthorPassword);
                EnsureSucceeded(createResult, email);
            }

            if (!await userManager.IsInRoleAsync(author, Roles.Author))
            {
                var roleResult = await userManager.AddToRoleAsync(author, Roles.Author);
                EnsureSucceeded(roleResult, email);
            }

            authors.Add(author);
        }

        return authors;
    }

    private static Recipe CreateRecipe(
        int index,
        RecipeDish dish,
        ApplicationUser author,
        Guid categoryId,
        Faker faker,
        DateTimeOffset now)
    {
        var createdAt = now.AddDays(-faker.Random.Int(1, 180)).AddMinutes(-index);
        var status = GetStatus(index);
        var recipe = new Recipe
        {
            Id = Guid.CreateVersion7(),
            Title = dish.Name,
            Slug = $"sample-{index + 1:00}-{SlugHelper.GenerateSlug(dish.Name)}",
            Description = $"{faker.PickRandom(DescriptionOpenings)}: {dish.Name.ToLowerInvariant()}, {faker.PickRandom(DescriptionDetails)}.",
            PrepTimeMinutes = faker.Random.Int(5, 60),
            CookTimeMinutes = faker.Random.Int(10, 150),
            Servings = faker.Random.Int(1, 8),
            Difficulty = (Difficulty)faker.Random.Int(0, 2),
            Status = status,
            PublishedAt = status == RecipeStatus.Published ? createdAt.AddHours(faker.Random.Int(1, 24)) : null,
            AuthorId = author.Id.ToString(),
            CategoryId = categoryId,
            IsDeleted = false,
            CreatedAt = createdAt
        };

        AddRecipeDetails(recipe, dish, faker, createdAt);
        return recipe;
    }

    private void EnsureRecipeDetails(Recipe recipe, RecipeDish dish, Faker faker, DateTimeOffset now)
    {
        if (recipe.Ingredients.Count >= 5 && recipe.Steps.Count >= 3 && recipe.Nutrition is not null)
        {
            return;
        }

        foreach (var ingredient in recipe.Ingredients.ToArray())
        {
            recipe.Ingredients.Remove(ingredient);
        }

        foreach (var step in recipe.Steps.ToArray())
        {
            recipe.Steps.Remove(step);
        }

        AddRecipeDetails(recipe, dish, faker, now);

        // BaseEntity luôn gán sẵn Id, nên nếu chỉ gắn vào collection của Recipe đang được track,
        // EF sẽ coi bước/nguyên liệu mới là bản ghi cũ -> UPDATE 0 dòng -> DbUpdateConcurrencyException.
        // Đánh dấu Added tường minh để EF sinh INSERT.
        db.AddRange(recipe.Ingredients);
        db.AddRange(recipe.Steps);
    }

    private static void AddRecipeDetails(Recipe recipe, RecipeDish dish, Faker faker, DateTimeOffset createdAt)
    {
        var profile = Profiles[dish.Style];
        var ingredients = profile.RequiredIngredients.ToList();
        var optionalCount = faker.Random.Int(1, Math.Min(3, profile.OptionalIngredients.Length));
        while (ingredients.Count < profile.RequiredIngredients.Length + optionalCount)
        {
            var candidate = faker.PickRandom(profile.OptionalIngredients);
            if (!ingredients.Contains(candidate))
            {
                ingredients.Add(candidate);
            }
        }

        recipe.Ingredients = ingredients.Select((ingredient, orderIndex) => new RecipeIngredient
        {
            Id = Guid.CreateVersion7(),
            Name = ingredient.Name,
            Quantity = faker.Random.Int(ingredient.MinimumQuantity, ingredient.MaximumQuantity),
            Unit = ingredient.Unit,
            OrderIndex = orderIndex,
            CreatedAt = createdAt
        }).ToList();

        recipe.Steps = profile.Steps.Select((step, index) => new RecipeStep
        {
            Id = Guid.CreateVersion7(),
            StepNumber = index + 1,
            Title = step.Title,
            Description = step.Description,
            DurationMinutes = faker.Random.Int(step.MinimumDurationMinutes, step.MaximumDurationMinutes),
            CreatedAt = createdAt
        }).ToList();

        recipe.Nutrition ??= new RecipeNutrition();
        recipe.Nutrition.Calories = faker.Random.Int(profile.MinimumCalories, profile.MaximumCalories);
        recipe.Nutrition.ProteinGrams = faker.Random.Int(profile.MinimumProtein, profile.MaximumProtein);
        recipe.Nutrition.FatGrams = faker.Random.Int(3, 40);
        recipe.Nutrition.CarbsGrams = faker.Random.Int(10, 100);
        recipe.Nutrition.FiberGrams = faker.Random.Int(1, 15);
        recipe.Nutrition.SugarGrams = faker.Random.Int(1, 30);
    }

    private static RecipeStatus GetStatus(int index) => (index % 10) switch
    {
        < 7 => RecipeStatus.Published,
        < 9 => RecipeStatus.Draft,
        _ => RecipeStatus.Archived
    };

    private static RecipeProfile Profile(
        IngredientSeed[] requiredIngredients,
        IngredientSeed[] optionalIngredients,
        RecipeStepSeed[] steps,
        int minimumCalories,
        int maximumCalories,
        int minimumProtein,
        int maximumProtein) =>
        new(requiredIngredients, optionalIngredients, steps, minimumCalories, maximumCalories, minimumProtein, maximumProtein);

    private static IngredientSeed Ingredient(string name, string unit, int minimumQuantity, int maximumQuantity) =>
        new(name, unit, minimumQuantity, maximumQuantity);

    private static RecipeStepSeed Step(string title, string description, int minimumDurationMinutes, int maximumDurationMinutes) =>
        new(title, description, minimumDurationMinutes, maximumDurationMinutes);

    private static int? GetSampleIndex(string slug)
    {
        var parts = slug.Split('-', 3);
        return parts.Length >= 2 && parts[0] == "sample" && int.TryParse(parts[1], out var number)
            ? number
            : null;
    }

    private static string SampleImageKey(int variant) => $"seed/recipes/sample-dish-{variant + 1:00}.png";

    private static void EnsureSucceeded(IdentityResult result, string subject)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Recipe data seeding for '{subject}' failed: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }

    private enum RecipeStyle
    {
        PhoBo,
        PhoGa,
        BunBo,
        BunRieu,
        SeafoodNoodles,
        RiceMeat,
        Fish,
        Chicken,
        Soup,
        Appetizer,
        Vegetable,
        Dessert,
        Drink,
        Baked
    }

    private sealed record RecipeDish(string Name, string CategorySlug, RecipeStyle Style);

    private sealed record RecipeProfile(
        IngredientSeed[] RequiredIngredients,
        IngredientSeed[] OptionalIngredients,
        RecipeStepSeed[] Steps,
        int MinimumCalories,
        int MaximumCalories,
        int MinimumProtein,
        int MaximumProtein);

    private sealed record IngredientSeed(string Name, string Unit, int MinimumQuantity, int MaximumQuantity);

    private sealed record RecipeStepSeed(
        string Title,
        string Description,
        int MinimumDurationMinutes,
        int MaximumDurationMinutes);
}
