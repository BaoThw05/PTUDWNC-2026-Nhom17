using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Slug).IsRequired().HasMaxLength(220);

        // ADR-0002: xmin ánh xạ vào Version, EF Core tự đọc/ghi giá trị này mỗi lần SELECT/UPDATE
        builder.Property(r => r.Version)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        // ADR-0001: partial unique index - chỉ áp dụng cho bản ghi CHƯA bị xóa
        builder.HasIndex(r => r.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // ADR-0001: Global Query Filter - mọi query mặc định bỏ qua recipe đã xóa mềm
        builder.HasQueryFilter(r => !r.IsDeleted);

        // Owned Entity - map chung bảng Recipes
        builder.OwnsOne(r => r.Nutrition, n =>
        {
            n.Property(x => x.Calories).HasColumnName("Nutrition_Calories");
            n.Property(x => x.ProteinGrams).HasColumnName("Nutrition_ProteinGrams");
            n.Property(x => x.FatGrams).HasColumnName("Nutrition_FatGrams");
            n.Property(x => x.CarbsGrams).HasColumnName("Nutrition_CarbsGrams");
            n.Property(x => x.FiberGrams).HasColumnName("Nutrition_FiberGrams");
            n.Property(x => x.SugarGrams).HasColumnName("Nutrition_SugarGrams");
        });

        // Cascade delete: xóa Recipe (thật) thì xóa luôn Steps, Ingredients
        builder.HasMany(r => r.Steps)
            .WithOne(s => s.Recipe)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}