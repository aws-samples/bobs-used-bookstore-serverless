namespace BookInventory.Service;

public interface IImageService
{
    public Task<bool> IsSafeAsync(string bucket, string image);
    public Task MoveImageToPublish(string bucket, string image);
}