using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

public sealed record UpdateCategoryCommand : IRequest<CategoryDto>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int OrderIndex { get; init; }
}

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().Length(2, 100);
        RuleFor(command => command.Description).MaximumLength(2000);
        RuleFor(command => command.ImageUrl).MaximumLength(500);
        RuleFor(command => command.OrderIndex).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCategoryCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = db.Categories.FirstOrDefault(item => item.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy danh mục có id '{request.Id}'.",
                CategoryErrorCodes.CategoryNotFound);

        var name = request.Name.Trim();
        if (db.Categories.Any(item => item.Id != category.Id && item.Name.ToUpper() == name.ToUpper()))
        {
            throw new ConflictException("Tên danh mục đã tồn tại.", CategoryErrorCodes.CategoryNameExists);
        }

        category.Name = name;
        category.Description = request.Description?.Trim();
        category.ImageUrl = request.ImageUrl?.Trim();
        category.OrderIndex = request.OrderIndex;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.Slug, category.Description, category.ImageUrl, 0);
    }
}
