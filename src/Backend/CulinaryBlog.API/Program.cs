using CulinaryBlog.API.Middlewares;
using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Application.Recipes.Commands.DeleteRecipe;
using CulinaryBlog.Application.Recipes.Commands.PublishRecipe;
using CulinaryBlog.Application.Recipes.Commands.RestoreRecipe;
using CulinaryBlog.Application.Recipes.Commands.UnpublishRecipe;
using CulinaryBlog.Application.Recipes.Commands.UpdateRecipe;
using CulinaryBlog.Application.Recipes.Queries.GetRecipeById;
using CulinaryBlog.Infrastructure.Data;
using CulinaryBlog.Infrastructure.Interceptors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AuditInterceptor>();

builder.Services.AddDbContext<CulinaryBlogDbContext>((sp, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<CulinaryBlogDbContext>());
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CulinaryBlogDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateRecipeCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(CreateRecipeCommand).Assembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapPost("/api/v1/recipes", async (CreateRecipeCommand command, IMediator mediator) =>
{
    var id = await mediator.Send(command);
    return Results.Created($"/api/v1/recipes/{id}", new { id });
})
.WithName("CreateRecipe");

app.MapGet("/api/v1/recipes/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var recipe = await mediator.Send(new GetRecipeByIdQuery(id));
    return Results.Ok(recipe);
})
.WithName("GetRecipeById");

app.MapPut("/api/v1/recipes/{id:guid}", async (Guid id, UpdateRecipeCommand command, IMediator mediator) =>
{
    await mediator.Send(command with { Id = id });
    return Results.NoContent();
})
.WithName("UpdateRecipe");

app.MapDelete("/api/v1/recipes/{id:guid}", async (Guid id, IMediator mediator) =>
{
    await mediator.Send(new DeleteRecipeCommand(id));
    return Results.NoContent();
})
.WithName("DeleteRecipe");

app.MapPost("/api/v1/recipes/{id:guid}/publish", async (Guid id, IMediator mediator) =>
{
    await mediator.Send(new PublishRecipeCommand(id));
    return Results.NoContent();
})
.WithName("PublishRecipe");

app.MapPost("/api/v1/recipes/{id:guid}/unpublish", async (Guid id, IMediator mediator) =>
{
    await mediator.Send(new UnpublishRecipeCommand(id));
    return Results.NoContent();
})
.WithName("UnpublishRecipe");

app.MapPost("/api/v1/recipes/{id:guid}/restore", async (Guid id, IMediator mediator) =>
{
    await mediator.Send(new RestoreRecipeCommand(id));
    return Results.NoContent();
})
.WithName("RestoreRecipe");

app.Run();