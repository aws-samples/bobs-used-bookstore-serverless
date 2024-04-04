using ImageMagick;

namespace BookInventory.Service.Utility
{
    public class ImageResizeService : IImageResizeService
    {
        private const int BookCoverImageWidth = 400;
        private const int BookCoverImageHeight = 600;

        public async Task<Stream> ResizeImageAsync(Stream image)
        {
            using var magickImage = new MagickImage(image);

            if (magickImage.BaseWidth == BookCoverImageWidth && magickImage.BaseHeight == BookCoverImageHeight) return image;

            var size = new MagickGeometry(BookCoverImageWidth, BookCoverImageHeight) { IgnoreAspectRatio = false };

            magickImage.Resize(size);

            var result = new MemoryStream();

            await magickImage.WriteAsync(result);

            result.Position = 0;

            return result;
        }
    }
}