using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Auth;

internal sealed class AuthDataSeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<AuthSeedOptions> options,
    TimeProvider timeProvider,
    ILogger<AuthDataSeeder> logger) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await EnsureSucceededAsync(roleManager.CreateAsync(new IdentityRole<Guid>(role)), role);
            }
        }

        var seed = options.Value;

        if (string.IsNullOrEmpty(seed.AdminPassword))
        {
            logger.LogInformation("Seed:Auth:AdminPassword is not set; skipping the admin account");
        }
        else
        {
            await EnsureUserAsync(seed.AdminEmail, "Admin", seed.AdminPassword, [Roles.Admin, Roles.Author]);
        }

        if (string.IsNullOrEmpty(seed.AuthorPassword))
        {
            logger.LogInformation("Seed:Auth:AuthorPassword is not set; skipping author accounts");
            return;
        }

        for (var index = 0; index < seed.AuthorEmails.Length; index++)
        {
            await EnsureUserAsync(seed.AuthorEmails[index], $"Author {index + 1}", seed.AuthorPassword, [Roles.Author]);
        }
    }

    private async Task EnsureUserAsync(string email, string fullName, string password, string[] roles)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        await EnsureSucceededAsync(userManager.CreateAsync(user, password), email);
        await EnsureSucceededAsync(userManager.AddToRolesAsync(user, roles), email);
        logger.LogInformation("Seeded user {Email} with roles {Roles}", email, roles);
    }

    private static async Task EnsureSucceededAsync(Task<IdentityResult> operation, string subject)
    {
        var result = await operation;
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Seeding '{subject}' failed: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
