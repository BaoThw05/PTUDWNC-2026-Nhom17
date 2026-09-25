using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Features.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CulinaryBlog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        // Thông báo validation luôn bằng tiếng Anh, không theo ngôn ngữ của máy chủ; frontend hiển thị theo code.
        ValidatorOptions.Global.LanguageManager.Enabled = false;
        services.TryAddSingleton(TimeProvider.System);

        services.AddAuthFeature();

        services.AddScoped<Features.Recipes.IRecipeAuthorizationHandler, Features.Recipes.RecipeAuthorizationHandler>();

        return services;
    }
}
