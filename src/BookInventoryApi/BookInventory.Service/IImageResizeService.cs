namespace BookInventory.Service
{
    public interface IImageResizeService
    {
        Task<Stream> ResizeImageAsync(Stream image);
    }
}