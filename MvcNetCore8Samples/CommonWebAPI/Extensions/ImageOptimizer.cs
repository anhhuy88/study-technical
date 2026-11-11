using CommonWebAPI.Domain;
using CommonWebAPI.Models;
using ImageMagick;
using Microsoft.AspNetCore.StaticFiles;
using System.Buffers.Binary;

namespace CommonWebAPI.Extensions;

public static class ImageOptimizer
{
    public static bool IsImage(byte[] data, string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || IsSupportedSignature(data);
    }

    public static OptimizedImage Optimize(byte[] data, string fileName, FileType fileType)
    {
        try
        {
            if (!IsSupportedSignature(data))
            {
                throw new InvalidDataException("Only JPEG, PNG, GIF and WebP images are supported.");
            }

            if (IsAnimatedPng(data))
            {
                throw new InvalidDataException("Animated PNG (APNG) images are not supported.");
            }

            using var images = new MagickImageCollection(data);
            if (images.Count == 0)
            {
                throw new InvalidDataException("The image has no frames.");
            }

            var format = images[0].Format;
            var (contentType, extension) = format switch
            {
                MagickFormat.Jpeg => ("image/jpeg", ".jpg"),
                MagickFormat.Png => ("image/png", ".png"),
                MagickFormat.Gif => ("image/gif", ".gif"),
                MagickFormat.WebP => ("image/webp", ".webp"),
                _ => throw new InvalidDataException("Only JPEG, PNG, GIF and WebP images are supported.")
            };

            if (images.Any(image => image.Format != format))
            {
                throw new InvalidDataException("The image contains frames with different formats.");
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var fileContentType)
                || !string.Equals(fileContentType, contentType, StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.ChangeExtension(fileName, extension);
            }

            if (images.Count > 1)
            {
                images.Coalesce();
            }

            var mustRewrite = false;
            foreach (var image in images)
            {
                if (image.Orientation != Orientation.Undefined
                    && image.Orientation != Orientation.TopLeft)
                {
                    mustRewrite = true;
                }

                image.AutoOrient();
                if (image.Width > VARIABLES.IMAGE_MAX_DIMENSION || image.Height > VARIABLES.IMAGE_MAX_DIMENSION)
                {
                    mustRewrite = true;
                    image.Resize(VARIABLES.IMAGE_MAX_DIMENSION, VARIABLES.IMAGE_MAX_DIMENSION);
                    image.ResetPage();
                }

                image.RemoveProfile("exif");
                image.RemoveProfile("iptc");
                image.RemoveProfile("xmp");
                if (format == MagickFormat.Jpeg || format == MagickFormat.WebP)
                {
                    image.Quality = fileType == FileType.Logo
                        ? VARIABLES.IMAGE_LOGO_QUALITY
                        : VARIABLES.IMAGE_QUALITY;
                }
            }

            if (images.Count > 1 && format == MagickFormat.Gif)
            {
                images.Optimize();
            }

            var optimized = images.ToByteArray(format);
            return new OptimizedImage(!mustRewrite && optimized.Length >= data.Length ? data : optimized, contentType, fileName);
        }
        catch (MagickException ex)
        {
            throw new InvalidDataException("The image is invalid or could not be processed.", ex);
        }
    }

    private static bool IsSupportedSignature(byte[] data)
    {
        return data.Length >= 3 && data[0] == 0xff && data[1] == 0xd8 && data[2] == 0xff
            || data.Length >= 8 && data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            || data.Length >= 6 && (data.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || data.AsSpan(0, 6).SequenceEqual("GIF89a"u8))
            || data.Length >= 12 && data.AsSpan(0, 4).SequenceEqual("RIFF"u8) && data.AsSpan(8, 4).SequenceEqual("WEBP"u8);
    }

    private static bool IsAnimatedPng(byte[] data)
    {
        if (data.Length < 8 || !data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        {
            return false;
        }

        var offset = 8;
        while (data.Length - offset >= 12)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, 4));
            if (length > data.Length - offset - 12)
            {
                break;
            }

            var chunkType = data.AsSpan(offset + 4, 4);
            if (chunkType.SequenceEqual("acTL"u8))
            {
                return true;
            }

            if (chunkType.SequenceEqual("IDAT"u8))
            {
                break;
            }

            offset += (int)length + 12;
        }

        return false;
    }
}
