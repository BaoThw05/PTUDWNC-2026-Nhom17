using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CulinaryBlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký DbContext qua interface (Application layer chỉ biết tới interface, không biết EF Core)
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<CulinaryBlogDbContext>());

// Đăng ký MediatR - quét toàn bộ Command/Query Handler trong Application layer
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateRecipeCommand).Assembly));

// Đăng ký FluentValidation - quét toàn bộ Validator trong Application layer
builder.Services.AddValidatorsFromAssembly(typeof(CreateRecipeCommand).Assembly);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // route mặc định: /scalar/v1
}

app.UseHttpsRedirection();

// Endpoint tạm để test CreateRecipeCommand
app.MapPost("/api/v1/recipes", async (CreateRecipeCommand command, IMediator mediator) =>
{
    var id = await mediator.Send(command);
    return Results.Created($"/api/v1/recipes/{id}", new { id });
})
.WithName("CreateRecipe");

app.UseMiddleware<CulinaryBlog.API.Middlewares.GlobalExceptionMiddleware>();

app.Run();