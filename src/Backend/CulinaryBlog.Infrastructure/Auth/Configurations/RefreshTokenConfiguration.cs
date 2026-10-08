using CulinaryBlog.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Auth.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    private const int Sha256HexLength = 64;
    private const int RevokedReasonMaxLength = 20;
    private const int IpAddressMaxLength = 45;

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.Property(token => token.TokenHash).HasMaxLength(Sha256HexLength).IsFixedLength().IsRequired();
        builder.Property(token => token.ReplacedByTokenHash).HasMaxLength(Sha256HexLength).IsFixedLength();
        builder.Property(token => token.RevokedReason).HasConversion<string>().HasMaxLength(RevokedReasonMaxLength);
        builder.Property(token => token.CreatedByIp).HasMaxLength(IpAddressMaxLength);

        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.FamilyId);
        builder.HasIndex(token => token.ExpiresAt);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
