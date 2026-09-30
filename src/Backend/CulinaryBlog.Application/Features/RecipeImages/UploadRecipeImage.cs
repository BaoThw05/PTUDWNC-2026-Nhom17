using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using MediatR;

namespace CulinaryBlog.Application.Features.RecipeImages;

public sealed record UploadRecipeImageCommand(Guid RecipeId, Stream Content, string? AltText) : IRequest<RecipeImageDto>;

public sealed class UploadRecipeImageCommandHandler(
    IAppDbContext db,
    IFileStorage storage,
    IBackgroundJobService backgroundJobs) : IRequestHandler<UploadRecipeImageCommand, RecipeImageDto>
{
    private const int MaxFileSize = 5 * 1024 * 1024;

    public async Task<RecipeImageDto> Handle(UploadRecipeImageCommand request, CancellationToken cancellationToken)
    {
        _ = db.Recipes.FirstOrDefault(recipe => recipe.Id == request.RecipeId)
            ?? throw new NotFoundException($"Không tìm thấy công thức có id '{request.RecipeId}'.", "RECIPE_NOT_FOUND");

        var content = await ReadContentAsync(request.Content, cancellationToken);
        var format = DetectFormat(content.GetBuffer().AsSpan(0, (int)content.Length))
            ?? throw new FileTypeNotAllowedException("Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP.", "FILE_TYPE_NOT_ALLOWED");

        var image = new RecipeImage
        {
            RecipeId = request.RecipeId,
            AltText = request.AltText?.Trim(),
            IsPrimary = !db.RecipeImages.Any(item => item.RecipeId == request.RecipeId),
            OrderIndex = db.RecipeImages
                .Where(item => item.RecipeId == request.RecipeId)
                .Select(item => (int?)item.OrderIndex)
                .Max() is var maxOrder && maxOrder.HasValue ? maxOrder.Value + 1 : 0
        };
        image.OriginalKey = $"recipes/{image.RecipeId}/{image.Id}/original.{format.Extension}";

        content.Position = 0;
        await storage.UploadAsync(content, image.OriginalKey, format.ContentType, cancellationToken);
        db.Add(image);
        await db.SaveChangesAsync(cancellationToken);

        backgroundJobs.Enqueue<IRecipeImageResizeJob>(job => job.ExecuteAsync(image.Id));

        return new RecipeImageDto(image.Id, storage.GetPublicUrl(image.OriginalKey), image.AltText, image.IsPrimary, image.OrderIndex);
    }

    private static async Task<MemoryStream> ReadContentAsync(Stream source, CancellationToken cancellationToken)
    {
        var target = new MemoryStream();
        await source.CopyToAsync(target, cancellationToken);
        if (target.Length > MaxFileSize)
        {
            throw new FileSizeExceededException("Kích thước ảnh tối đa là 5 MB.", "FILE_SIZE_EXCEEDED");
        }

        return target;
    }

    private static ImageFormat? DetectFormat(ReadOnlySpan<byte> content)
    {
        if (content.Length >= 3 && content[..3].SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF })) return new("jpg", "image/jpeg");
        if (content.Length >= 8 && content[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return new("png", "image/png");
        if (content.Length >= 12 && content[..4].SequenceEqual("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8)) return new("webp", "image/webp");
        return null;
    }

    private sealed record ImageFormat(string Extension, string ContentType);
}

public sealed record RecipeImageDto(Guid ImageId, string OriginalUrl, string? AltText, bool IsPrimary, int OrderIndex);
