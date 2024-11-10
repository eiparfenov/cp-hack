using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Api;

public static class Images
{
    public static RouteGroupBuilder MapImages(this RouteGroupBuilder builder)
    {
        builder.MapPost("create_package", async ([FromServices] TimeProvider timeProvider, [FromServices] ApplicationDbContext db) =>
        {
            var package = new Package()
            {
                Time = timeProvider.GetUtcNow(),
            };
            await db.Packages.AddAsync(package);
            await db.SaveChangesAsync();
            return Results.Json(new { PackageId = package.Id });
        });
        builder.MapPost("upload_image", async ([FromBody] UploadImageRequest request, [FromServices] TimeProvider timeProvider, [FromServices] ApplicationDbContext db, [FromServices] IMlService mlService, [FromServices] IImagesToS3Service s3Service) =>
        {
            var imageTitle = $"{request.PackageId}/{request.ImagePath}/{request.ImageTitle}";

            var imageBytes = Convert.FromBase64String(request.ImageBase64);
            using var imageStream = new MemoryStream(imageBytes);
            var imageObject = System.Drawing.Image.FromStream(imageStream);
            imageStream.Seek(0, SeekOrigin.Begin);
            
            var url = await s3Service.PutImageToS3(imageStream, imageTitle);
            var mlResult = await mlService.PerformMlAsync(url, imageTitle);

            var image = new Image()
            {
                PackageId = request.PackageId,
                Path = request.ImagePath,
                Title = request.ImageTitle,
                Height = imageObject.Height,
                Width = imageObject.Width,
                Url = url,
                MlResult = mlResult
            };
            await db.Images.AddAsync(image);
            await db.SaveChangesAsync();
            return Results.Json(new UploadImageResponse()
            {
                ImagePath = request.ImagePath,
                ImageTitle = request.ImageTitle,
                MlResponse = mlResult
            });
        });
        return builder;
    }

    private class UploadImageRequest
    {
        public Guid PackageId { get; set; }
        public string ImageTitle { get; set; }
        public string ImagePath { get; set; }
        public string ImageBase64 { get; set; }
    }
    private class UploadImageResponse
    {
        public string ImageTitle { get; set; }
        public string ImagePath { get; set; }
        public MlResult MlResponse { get; set; }
    } 
}