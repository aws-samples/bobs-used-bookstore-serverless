namespace BookInventory.Service.Utility
{
    public interface IImageResizeService
    {
        Task<Stream> ResizeImageAsync(Stream image);
    }
}