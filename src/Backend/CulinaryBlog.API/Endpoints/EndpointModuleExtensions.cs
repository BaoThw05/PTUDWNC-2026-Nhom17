namespace CulinaryBlog.API.Endpoints;

internal static class EndpointModuleExtensions
{
    public const string ApiPrefix = "/api/v1";

    public static IServiceCollection AddEndpointModules(this IServiceCollection services)
    {
        var moduleTypes = typeof(IEndpointModule).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsAssignableTo(typeof(IEndpointModule)));

        foreach (var moduleType in moduleTypes)
        {
            services.AddSingleton(typeof(IEndpointModule), moduleType);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup(ApiPrefix);

        foreach (var module in app.ServiceProvider.GetServices<IEndpointModule>())
        {
            module.MapEndpoints(api);
        }

        return app;
    }
}
