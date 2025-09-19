namespace SWD392_backend.Infrastructure.Services.S3Service
{
    public interface IS3Service
    {
        string GeneratePreSignedURL(string key, string contentType, int expireMintues = 15);
        Task DeleteFileAsync(List<string> urls);
    }
}
