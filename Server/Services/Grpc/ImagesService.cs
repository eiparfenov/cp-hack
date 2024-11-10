using System.Text.Json;
using Grpc.Core;
using Server.Models;

namespace Server.Services.Grpc;

public class ImagesService(ApplicationDbContext db, IImagesToS3Service s3Service, IMlService mlService, TimeProvider timeProvider): ImageService.ImageServiceBase
{
    public override async Task PerformImages(IAsyncStreamReader<PerformImagesRequest> requestStream, IServerStreamWriter<PerformImagesResponse> responseStream, ServerCallContext context)
    {
        var package = new Package()
        {
            Id = Guid.NewGuid(),
            Images = new List<Image>(),
            Time = timeProvider.GetUtcNow()
        };
        await foreach (var imageRequest in requestStream.ReadAllAsync())
        {
            var imageTitle = $"{package.Id}/{imageRequest.ImagePath}/{imageRequest.ImageTitle}";

            var imageBytes = Convert.FromBase64String(imageRequest.Base64Image);
            using var imageStream = new MemoryStream(imageBytes);
            var imageObject = System.Drawing.Image.FromStream(imageStream);
            imageStream.Seek(0, SeekOrigin.Begin);
            
            var url = await s3Service.PutImageToS3(imageStream, imageTitle);
            var mlResult = await mlService.PerformMlAsync(url, imageTitle);

            package.Images.Add(new Image()
            {
                Path = imageRequest.ImagePath,
                Title = imageRequest.ImageTitle,
                Height = imageObject.Height,
                Width = imageObject.Width,
                Url = url,
                MlResult = mlResult
            });
            await responseStream.WriteAsync(new PerformImagesResponse()
            {
                ImagePath = imageRequest.ImagePath,
                ImageTitle = imageRequest.ImageTitle,
                JsonFromMl = JsonSerializer.Serialize(mlResult),
            });
        }
        await db.Packages.AddRangeAsync(package);
        await db.SaveChangesAsync();
    }
}