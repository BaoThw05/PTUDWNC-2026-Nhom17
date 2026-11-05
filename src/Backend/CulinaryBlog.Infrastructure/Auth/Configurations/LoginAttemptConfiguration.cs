using CulinaryBlog.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Auth.Configurations;

internal sealed class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    private const int EnumMaxLength = 20;

    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("LoginAttempts");

        builder.Property(attempt => attempt.Method).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(attempt => attempt.Result).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(attempt => attempt.IpAddress).HasMaxLength(LoginAttempt.IpAddressMaxLength);
        builder.Property(attempt => attempt.UserAgent).HasMaxLength(LoginAttempt.UserAgentMaxLength);

        builder.HasIndex(attempt => new { attempt.UserId, attempt.CreatedAt });
        builder.HasIndex(attempt => attempt.CreatedAt);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(attempt => attempt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
