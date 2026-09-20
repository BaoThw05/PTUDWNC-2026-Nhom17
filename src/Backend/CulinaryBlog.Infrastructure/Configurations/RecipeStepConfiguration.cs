using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.Property(s => s.Description).IsRequired().HasMaxLength(2000);
        builder.Property(s => s.Title).HasMaxLength(200);

        builder.Property(s => s.Version)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        builder.HasQueryFilter(s => !s.Recipe.IsDeleted);
    }
}