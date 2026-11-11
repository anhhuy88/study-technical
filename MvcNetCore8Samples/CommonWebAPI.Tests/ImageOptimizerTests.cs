using CommonWebAPI.Domain;
using CommonWebAPI.Extensions;
using ImageMagick;
using Xunit;
using ImageOptimizer = CommonWebAPI.Extensions.ImageOptimizer;

namespace CommonWebAPI.Tests;

public class ImageOptimizerTests
{
    [Theory]
    [InlineData(MagickFormat.Jpeg, "image/jpeg", ".jpg")]
    [InlineData(MagickFormat.Png, "image/png", ".png")]
    [InlineData(MagickFormat.Gif, "image/gif", ".gif")]
    [InlineData(MagickFormat.WebP, "image/webp", ".webp")]
    public void LargeImageIsResizedAndKeepsItsFormat(MagickFormat format, string contentType, string extension)
    {
        using var original = new MagickImage(MagickColors.Red, 2000, 1000);
        var source = original.ToByteArray(format);

        var result = ImageOptimizer.Optimize(source, "photo" + extension, FileType.Normal);

        using var output = new MagickImage(result.Data);
        Assert.Equal(1600u, output.Width);
        Assert.Equal(800u, output.Height);
        Assert.Equal(format, output.Format);
        Assert.Equal(contentType, result.ContentType);
        Assert.Equal("photo" + extension, result.FileName);
    }

    [Fact]
    public void SmallImageIsNotEnlargedAndDoesNotGrow()
    {
        using var original = new MagickImage(MagickColors.Red, 80, 50);
        var source = original.ToByteArray(MagickFormat.Png);

        var result = ImageOptimizer.Optimize(source, "small.png", FileType.Normal);

        using var output = new MagickImage(result.Data);
        Assert.Equal(80u, output.Width);
        Assert.Equal(50u, output.Height);
        Assert.True(result.Data.Length <= source.Length);
    }

    [Fact]
    public void HighQualityJpegIsCompressedWithoutResizing()
    {
        using var original = new MagickImage("plasma:fractal", 800, 600) { Quality = 100 };
        var source = original.ToByteArray(MagickFormat.Jpeg);

        var result = ImageOptimizer.Optimize(source, "photo.jpg", FileType.Normal);

        using var output = new MagickImage(result.Data);
        Assert.Equal(800u, output.Width);
        Assert.Equal(600u, output.Height);
        Assert.True(result.Data.Length < source.Length);
    }

    [Fact]
    public void ExifOrientationIsAppliedBeforeRemovingMetadata()
    {
        using var original = new MagickImage(MagickColors.Red, 120, 80);
        var exif = new ExifProfile();
        exif.SetValue(ExifTag.Orientation, (ushort)6);
        original.SetProfile(exif);
        original.Orientation = Orientation.RightTop;

        var source = original.ToByteArray(MagickFormat.Jpeg);
        using var decodedOriginal = new MagickImage(source);
        Assert.Equal(Orientation.RightTop, decodedOriginal.Orientation);
        var result = ImageOptimizer.Optimize(source, "photo.jpg", FileType.Normal);

        using var output = new MagickImage(result.Data);
        Assert.Equal(80u, output.Width);
        Assert.Equal(120u, output.Height);
        Assert.True(output.Orientation is Orientation.Undefined or Orientation.TopLeft);
    }

    [Fact]
    public void TransparentLogoRemainsTransparent()
    {
        using var original = new MagickImage(MagickColors.Transparent, 2000, 1000);
        var result = ImageOptimizer.Optimize(original.ToByteArray(MagickFormat.Png), "logo.png", FileType.Logo);

        using var output = new MagickImage(result.Data);
        Assert.True(output.HasAlpha);
        Assert.Equal(1600u, output.Width);
    }

    [Theory]
    [InlineData(MagickFormat.Gif, ".gif")]
    [InlineData(MagickFormat.WebP, ".webp")]
    public void AnimatedImagesKeepTheirFramesAndTiming(MagickFormat format, string extension)
    {
        using var images = new MagickImageCollection();
        using var first = new MagickImage(MagickColors.Red, 1800, 900) { AnimationDelay = 12, AnimationIterations = 3 };
        using var second = new MagickImage(MagickColors.Blue, 1800, 900) { AnimationDelay = 25, AnimationIterations = 3 };
        images.Add(first);
        images.Add(second);
        var source = images.ToByteArray(format);

        var result = ImageOptimizer.Optimize(source, "animation" + extension, FileType.Normal);

        using var output = new MagickImageCollection(result.Data);
        output.Coalesce();
        Assert.Equal(2, output.Count);
        Assert.All(output, frame =>
        {
            Assert.Equal(1600u, frame.Width);
            Assert.Equal(800u, frame.Height);
        });
        Assert.Equal(12u, output[0].AnimationDelay);
        Assert.Equal(25u, output[1].AnimationDelay);
        Assert.Equal(3u, output[0].AnimationIterations);
        using var firstPixels = output[0].GetPixels();
        using var secondPixels = output[1].GetPixels();
        Assert.NotEqual(firstPixels.GetPixel(10, 10).ToColor(), secondPixels.GetPixel(10, 10).ToColor());
    }

    [Fact]
    public void MismatchedExtensionIsCorrected()
    {
        using var original = new MagickImage(MagickColors.Red, 10, 10);
        var result = ImageOptimizer.Optimize(original.ToByteArray(MagickFormat.Png), "image.jpg", FileType.Normal);

        Assert.Equal("image.png", result.FileName);
        Assert.Equal("image/png", result.ContentType);
    }

    [Fact]
    public void InvalidOrUnsupportedImageIsRejected()
    {
        Assert.Throws<InvalidDataException>(() => ImageOptimizer.Optimize("not an image"u8.ToArray(), "image.svg", FileType.Normal));
        Assert.Throws<InvalidDataException>(() => ImageOptimizer.Optimize(new byte[] { 0xff, 0xd8, 0xff, 0 }, "image.jpg", FileType.Normal));
    }

    [Fact]
    public void AnimatedPngIsRejectedInsteadOfLosingFrames()
    {
        using var images = new MagickImageCollection();
        using var first = new MagickImage(MagickColors.Red, 10, 10) { AnimationDelay = 12 };
        using var second = new MagickImage(MagickColors.Blue, 10, 10) { AnimationDelay = 25 };
        images.Add(first);
        images.Add(second);
        var source = images.ToByteArray(MagickFormat.APng);

        var error = Assert.Throws<InvalidDataException>(() => ImageOptimizer.Optimize(source, "animation.png", FileType.Normal));
        Assert.Contains("APNG", error.Message);
    }

    [Fact]
    public void ImageDetectionUsesMimeTypeOrSignature()
    {
        using var original = new MagickImage(MagickColors.Red, 10, 10);
        Assert.True(ImageOptimizer.IsImage(original.ToByteArray(MagickFormat.Png), "application/octet-stream"));
        Assert.True(ImageOptimizer.IsImage("svg"u8.ToArray(), "image/svg+xml"));
        Assert.False(ImageOptimizer.IsImage("plain text"u8.ToArray(), "text/plain"));
    }
}
