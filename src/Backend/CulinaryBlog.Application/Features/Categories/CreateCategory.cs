using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

public sealed record CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int OrderIndex { get; init; }
}

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().Length(2, 100);
        RuleFor(command => command.Description).MaximumLength(2000);
        RuleFor(command => command.ImageUrl).MaximumLength(500);
        RuleFor(command => command.OrderIndex).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateCategoryCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (db.Categories.Any(category => category.Name.ToUpper() == name.ToUpper()))
        {
            throw new ConflictException("Tên danh mục đã tồn tại.", CategoryErrorCodes.CategoryNameExists);
        }

        var category = new Category
        {
            Name = name,
            Slug = CategorySlugHelper.GenerateUniqueSlug(db, name),
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            OrderIndex = request.OrderIndex
        };

        db.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Slug, category.Description, category.ImageUrl, 0);
    }
}
