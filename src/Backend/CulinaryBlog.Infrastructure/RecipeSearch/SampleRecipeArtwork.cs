using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace CulinaryBlog.Infrastructure.RecipeSearch;

internal static class SampleRecipeArtwork
{
    private const int Width = 320;
    private const int Height = 220;
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly (byte Red, byte Green, byte Blue)[] BrothColors =
    [
        (191, 112, 58),
        (150, 104, 62),
        (207, 144, 76)
    ];

    public static byte[] CreatePng(int variant)
    {
        var pixels = new byte[Width * Height * 4];
        var broth = BrothColors[Math.Abs(variant % BrothColors.Length)];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Paint(pixels, x, y, 244, 238, 226);

                if (InsideEllipse(x, y, 160, 112, 148, 84))
                {
                    Paint(pixels, x, y, 211, 199, 179);
                }

                if (InsideEllipse(x, y, 160, 108, 140, 76))
                {
                    Paint(pixels, x, y, 255, 253, 246);
                }

                if (InsideEllipse(x, y, 160, 108, 108, 52))
                {
                    Paint(pixels, x, y, broth.Red, broth.Green, broth.Blue);
                }
            }
        }

        for (var strand = 0; strand < 7; strand++)
        {
            for (var x = 92; x < 228; x++)
            {
                var y = 91 + strand * 6 + (int)Math.Round(Math.Sin((x + strand * 17) / 15d) * 5);
                if (InsideEllipse(x, y, 160, 108, 103, 48))
                {
                    FillEllipse(pixels, x, y, 2, 1, 247, 218, 166);
                }
            }
        }

        FillEllipse(pixels, 126, 93, 17, 8, 113, 61, 46);
        FillEllipse(pixels, 178, 125, 20, 9, 132, 73, 49);
        FillEllipse(pixels, 192, 91, 14, 7, 109, 57, 43);
        FillEllipse(pixels, 146, 129, 15, 7, 226, 190, 119);

        FillEllipse(pixels, 111, 117, 5, 4, 77, 130, 75);
        FillEllipse(pixels, 153, 84, 5, 4, 88, 145, 80);
        FillEllipse(pixels, 207, 113, 5, 4, 69, 122, 68);
        FillEllipse(pixels, 164, 139, 4, 4, 224, 82, 57);
        FillEllipse(pixels, 136, 105, 3, 3, 251, 236, 202);

        var scanlines = new byte[Height * (Width * 4 + 1)];
        for (var y = 0; y < Height; y++)
        {
            var rowOffset = y * (Width * 4 + 1);
            scanlines[rowOffset] = 0;
            Buffer.BlockCopy(pixels, y * Width * 4, scanlines, rowOffset + 1, Width * 4);
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(scanlines);
        }

        using var png = new MemoryStream();
        png.Write(Signature);

        var header = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), Width);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), Height);
        header[8] = 8;
        header[9] = 6;
        WriteChunk(png, "IHDR", header);
        WriteChunk(png, "IDAT", compressed.ToArray());
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static void FillEllipse(
        byte[] pixels,
        int centerX,
        int centerY,
        int radiusX,
        int radiusY,
        byte red,
        byte green,
        byte blue)
    {
        for (var y = centerY - radiusY; y <= centerY + radiusY; y++)
        {
            for (var x = centerX - radiusX; x <= centerX + radiusX; x++)
            {
                if (InsideEllipse(x, y, centerX, centerY, radiusX, radiusY))
                {
                    Paint(pixels, x, y, red, green, blue);
                }
            }
        }
    }

    private static bool InsideEllipse(int x, int y, int centerX, int centerY, int radiusX, int radiusY)
    {
        var horizontal = (x - centerX) / (double)radiusX;
        var vertical = (y - centerY) / (double)radiusY;
        return horizontal * horizontal + vertical * vertical <= 1;
    }

    private static void Paint(byte[] pixels, int x, int y, byte red, byte green, byte blue)
    {
        var offset = (y * Width + x) * 4;
        pixels[offset] = red;
        pixels[offset + 1] = green;
        pixels[offset + 2] = blue;
        pixels[offset + 3] = byte.MaxValue;
    }

    private static void WriteChunk(Stream output, string name, byte[] data)
    {
        var type = Encoding.ASCII.GetBytes(name);
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        output.Write(length);
        output.Write(type);
        output.Write(data);

        var checksum = 0xffffffffu;
        foreach (var value in type)
        {
            checksum = UpdateChecksum(checksum, value);
        }

        foreach (var value in data)
        {
            checksum = UpdateChecksum(checksum, value);
        }

        Span<byte> checksumBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksumBytes, ~checksum);
        output.Write(checksumBytes);
    }

    private static uint UpdateChecksum(uint checksum, byte value)
    {
        checksum ^= value;
        for (var bit = 0; bit < 8; bit++)
        {
            checksum = (checksum & 1) == 1
                ? 0xedb88320u ^ (checksum >> 1)
                : checksum >> 1;
        }

        return checksum;
    }
}