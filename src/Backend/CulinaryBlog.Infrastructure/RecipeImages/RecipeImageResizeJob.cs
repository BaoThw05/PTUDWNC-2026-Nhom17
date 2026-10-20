using CulinaryBlog.Application.Abstractions;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace CulinaryBlog.Infrastructure.RecipeImages;

public sealed class RecipeImageResizeJob(
    IAppDbContext db,
    IFileStorage storage,
    ILogger<RecipeImageResizeJob> logger) : IRecipeImageResizeJob
{
    private const int ThumbnailSize = 300;
    private const int MediumMaxWidth = 800;

    public async Task ExecuteAsync(Guid imageId)
    {
        var image = db.RecipeImages.FirstOrDefault(item => item.Id == imageId);
        if (image is null || image.IsProcessingFailed || (image.ThumbnailKey is not null && image.MediumKey is not null))
        {
            return;
        }

        try
        {
            await using var original = await storage.OpenReadAsync(image.OriginalKey);
            using var codec = SKCodec.Create(original)
                ?? throw new ImageDecodeException("Không thể giải mã ảnh gốc.");
            using var source = SKBitmap.Decode(codec)
                ?? throw new ImageDecodeException("Không thể giải mã ảnh gốc.");

            var prefix = image.OriginalKey[..image.OriginalKey.LastIndexOf('/')];
            var thumbnailKey = $"{prefix}/thumb.webp";
            var mediumKey = $"{prefix}/medium.webp";

            using var thumbnail = CreateThumbnail(source);
            using var medium = CreateMedium(source);
            await UploadWebpAsync(thumbnail, thumbnailKey);
            await UploadWebpAsync(medium, mediumKey);

            image.ThumbnailKey = thumbnailKey;
            image.MediumKey = mediumKey;
            image.IsProcessingFailed = false;
            image.ProcessingError = null;
            image.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }
        catch (ImageDecodeException exception)
        {
            image.IsProcessingFailed = true;
            image.ProcessingError = exception.Message;
            image.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            logger.LogWarning(exception, "Cannot decode recipe image {ImageId}; resize job will not retry.", imageId);
        }
    }

    private async Task UploadWebpAsync(SKBitmap bitmap, string key)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Webp, quality: 85);
        if (data is null)
        {
            throw new InvalidOperationException("Không thể mã hóa biến thể WebP.");
        }

        await using var content = new MemoryStream();
        data.SaveTo(content);
        content.Position = 0;
        await storage.UploadAsync(content, key, "image/webp");
    }

    private static SKBitmap CreateThumbnail(SKBitmap source)
    {
        var side = Math.Min(source.Width, source.Height);
        var sourceRect = new SKRect(
            (source.Width - side) / 2f,
            (source.Height - side) / 2f,
            (source.Width + side) / 2f,
            (source.Height + side) / 2f);
        return Resize(source, sourceRect, ThumbnailSize, ThumbnailSize);
    }

    private static SKBitmap CreateMedium(SKBitmap source)
    {
        var width = Math.Min(source.Width, MediumMaxWidth);
        var height = Math.Max(1, (int)Math.Round(source.Height * (width / (double)source.Width)));
        return Resize(source, new SKRect(0, 0, source.Width, source.Height), width, height);
    }

    private static SKBitmap Resize(SKBitmap source, SKRect sourceRect, int width, int height)
    {
        var target = new SKBitmap(width, height, source.ColorType, source.AlphaType);
        using var canvas = new SKCanvas(target);
        canvas.Clear(SKColors.Transparent);
        using var paint = new SKPaint { IsAntialias = true };
        canvas.DrawBitmap(source, sourceRect, new SKRect(0, 0, width, height), paint);
        return target;
    }

    private sealed class ImageDecodeException(string message) : Exception(message);
}
