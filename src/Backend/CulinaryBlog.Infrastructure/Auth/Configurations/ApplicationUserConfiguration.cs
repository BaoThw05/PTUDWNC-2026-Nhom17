using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Auth.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    private const int DisplayNameMaxLength = 100;
    private const int AvatarUrlMaxLength = 2048;

    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(user => user.DisplayName).HasMaxLength(DisplayNameMaxLength).IsRequired();
        builder.Property(user => user.AvatarUrl).HasMaxLength(AvatarUrlMaxLength);
        builder.Property(user => user.IsActive).HasDefaultValue(true);
    }
}
