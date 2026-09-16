using CulinaryBlog.API.Endpoints;
using Microsoft.OpenApi;

namespace CulinaryBlog.API.OpenApi;

internal static class OpenApiExtensions
{
    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer((document, context, _) =>
        {
            document.Info.Title = "Culinary Blog API";

            // Khai báo nhóm của mọi module để Scalar liệt kê kể cả khi module chưa có endpoint.
            document.Tags ??= new HashSet<OpenApiTag>();
            foreach (var module in context.ApplicationServices.GetServices<IEndpointModule>())
            {
                document.Tags.Add(new OpenApiTag { Name = module.Tag, Description = module.Description });
            }

            return Task.CompletedTask;
        }));

        return services;
    }
}
