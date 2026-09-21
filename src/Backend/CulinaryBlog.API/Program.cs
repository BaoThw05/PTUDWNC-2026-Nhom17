using CulinaryBlog.API.Middlewares;
using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Recipes.Commands.CreateRecipe;
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

// Đăng ký DbContext qua interface (Application layer chỉ biết tới interface, không biết EF Core)
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<CulinaryBlogDbContext>());
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CulinaryBlogDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateRecipeCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Đăng ký FluentValidation - quét toàn bộ Validator trong Application layer
builder.Services.AddValidatorsFromAssembly(typeof(CreateRecipeCommand).Assembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // route mặc định: /scalar/v1
}

app.UseHttpsRedirection();

app.MapPost("/api/v1/recipes", async (CreateRecipeCommand command, IMediator mediator) =>
{
    var id = await mediator.Send(command);
    return Results.Created($"/api/v1/recipes/{id}", new { id });
})
.WithName("CreateRecipe");


app.Run();