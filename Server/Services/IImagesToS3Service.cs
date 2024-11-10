using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Server.Configuration;

namespace Server.Services;

public interface IImagesToS3Service
{
    Task<string> PutImageToS3(Stream image, string key);
}

public class ImagesToS3Service(AmazonS3Client client, IOptions<S3Options> options) : IImagesToS3Service
{
    public async Task<string> PutImageToS3(Stream image, string key)
    {
        var pubObjectRequest = new PutObjectRequest()
        {
            BucketName = options.Value.BucketName,
            Key = key,
            InputStream = image,
            CannedACL = S3CannedACL.PublicRead
        };
        var pubObjectResponse = await client.PutObjectAsync(pubObjectRequest);
        return $"{options.Value.ServiceUrl}/{options.Value.BucketName}/{key}";
    }
}

