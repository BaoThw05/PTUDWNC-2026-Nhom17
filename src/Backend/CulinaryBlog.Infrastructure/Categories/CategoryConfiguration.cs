using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Categories;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    private static readonly DateTimeOffset SeededAt = new(2026, 9, 25, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(category => category.Name)
            .IsRequired()
            .HasColumnType("citext")
            .HasMaxLength(100);
        builder.Property(category => category.Slug).IsRequired().HasMaxLength(120);
        builder.Property(category => category.Description).HasColumnType("text");
        builder.Property(category => category.ImageUrl).HasMaxLength(500);
        builder.Property(category => category.OrderIndex).HasDefaultValue(0);

        builder.HasIndex(category => category.Name).IsUnique();
        builder.HasIndex(category => category.Slug).IsUnique();

        builder.HasMany<Recipe>()
            .WithOne()
            .HasForeignKey(recipe => recipe.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            CreateSeed("0b89ac27-e8d0-4ee2-9e4b-1c0f1a52c001", "Món khai vị", "mon-khai-vi", "Các món nhẹ dùng trước bữa chính.", 1),
            CreateSeed("1c89ac27-e8d0-4ee2-9e4b-1c0f1a52c002", "Món chính", "mon-chinh", "Các món ăn chính cho bữa cơm hằng ngày.", 2),
            CreateSeed("2d89ac27-e8d0-4ee2-9e4b-1c0f1a52c003", "Món phụ", "mon-phu", "Các món ăn kèm cho bữa ăn thêm trọn vẹn.", 3),
            CreateSeed("3e89ac27-e8d0-4ee2-9e4b-1c0f1a52c004", "Canh và súp", "canh-va-sup", "Các món canh và súp nóng hổi.", 4),
            CreateSeed("4f89ac27-e8d0-4ee2-9e4b-1c0f1a52c005", "Món chay", "mon-chay", "Các công thức thuần chay và chay.", 5),
            CreateSeed("5a89ac27-e8d0-4ee2-9e4b-1c0f1a52c006", "Món tráng miệng", "mon-trang-mieng", "Các món ngọt dùng sau bữa ăn.", 6),
            CreateSeed("6b89ac27-e8d0-4ee2-9e4b-1c0f1a52c007", "Đồ uống", "do-uong", "Các loại thức uống giải khát.", 7),
            CreateSeed("7c89ac27-e8d0-4ee2-9e4b-1c0f1a52c008", "Bánh", "banh", "Các công thức bánh mặn và bánh ngọt.", 8));
    }

    private static Category CreateSeed(string id, string name, string slug, string description, int orderIndex) => new()
    {
        Id = Guid.Parse(id),
        Name = name,
        Slug = slug,
        Description = description,
        OrderIndex = orderIndex,
        CreatedAt = SeededAt
    };
}
