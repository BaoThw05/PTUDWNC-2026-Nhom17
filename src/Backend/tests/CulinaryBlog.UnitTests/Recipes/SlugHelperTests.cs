using CulinaryBlog.Application.Features.Recipes;

namespace CulinaryBlog.UnitTests.Recipes;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Phở bò Hà Nội", "pho-bo-ha-noi")]
    [InlineData("Đậu hũ chiên sả ớt", "dau-hu-chien-sa-ot")]
    [InlineData("Bánh xèo miền Tây", "banh-xeo-mien-tay")]
    [InlineData("Gỏi cuốn tôm thịt & nước chấm", "goi-cuon-tom-thit-nuoc-cham")]
    [InlineData("Canh chua cá lóc ĐỒNG", "canh-chua-ca-loc-dong")]
    public void GenerateSlug_ShouldConvertVietnameseCharactersCorrectly(string input, string expected)
    {
        var slug = SlugHelper.GenerateSlug(input);

        Assert.Equal(expected, slug);
    }

    [Fact]
    public void GenerateSlug_ShouldForbidSearchSlug()
    {
        var slug = SlugHelper.GenerateSlug("Search");

        Assert.Equal("search-recipe", slug);
    }

    [Theory]
    [InlineData("  Món ăn ngon  ", "mon-an-ngon")]
    [InlineData("Món---ngon---lắm", "mon-ngon-lam")]
    [InlineData("Món ăn @#$%^&* ngon", "mon-an-ngon")]
    public void GenerateSlug_ShouldTrimAndCleanHyphens(string input, string expected)
    {
        var slug = SlugHelper.GenerateSlug(input);

        Assert.Equal(expected, slug);
    }
}
