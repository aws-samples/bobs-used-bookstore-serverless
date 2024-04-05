namespace BookInventory.Service;

public interface IImageService
{
    public Task<bool> IsSafeAsync(string bucket, string image);
    public Task SaveImageAsync(string bucket, string image);
}