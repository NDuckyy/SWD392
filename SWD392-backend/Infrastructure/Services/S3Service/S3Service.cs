using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace SWD392_backend.Infrastructure.Services.S3Service
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service()
        {
            // Get from env
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION");
            var bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");

            if (string.IsNullOrEmpty(accessKey) || 
                string.IsNullOrEmpty(secretKey) || 
                string.IsNullOrEmpty(region) ||
                string.IsNullOrEmpty(bucketName))
            {
                throw new Exception("ENV is not set");
            }

            // Create S3 Client
            _bucketName = bucketName;
            _s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(region));
        }

        public async Task DeleteFileAsync(List<string> urls)
        {
            List<string> keys = new List<string>();

            foreach (var url in urls)
            {
                keys.Add(new Uri(url).AbsolutePath.TrimStart('/'));
            }

            var request = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = keys.Select(k => new KeyVersion { Key = k }).ToList()
            };

            await _s3Client.DeleteObjectsAsync(request);
        }

        public string GeneratePreSignedURL(string key, string contentType, int expireMintues = 15)
        {
            Console.WriteLine(contentType);
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
                Expires = DateTime.UtcNow.AddMinutes(expireMintues),
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}
