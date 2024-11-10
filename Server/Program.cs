using System.Drawing;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server;
using Server.Api;
using Server.Configuration;
using Server.Services;
using Server.Services.Grpc;
using Server.Services.Initialize;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<S3Options>(builder.Configuration.GetSection(nameof(S3Options)));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddGrpc();
builder.Services.AddSingleton(s =>
{
    var opts = s.GetRequiredService<IOptions<S3Options>>().Value;
    return new AmazonS3Client(opts.AccessKeyId, opts.SecretAccessKey, new AmazonS3Config()
    {
        ServiceURL = opts.ServiceUrl,
        ForcePathStyle = true
    });
});
builder.Services.AddScoped<IImagesToS3Service, ImagesToS3Service>();
builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb"));
    optionsBuilder.UseSnakeCaseNamingConvention();
});
builder.Services.AddHttpClient<IMlService, MlService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration.GetConnectionString("MlService")!);
});
builder.Services.AddHostedService<MigrateDb<ApplicationDbContext>>();
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapPost("/test/postImageToS3", async ([FromServices] IImagesToS3Service imagesToS3Service, [FromServices] IMlService mlService, [FromBody] TestImagesRequest request) =>
{
    var byteArray = Convert.FromBase64String(request.ImageBase64);
    using var stream = new MemoryStream(byteArray);
    var image = Image.FromStream(stream);
    stream.Seek(0, SeekOrigin.Begin);
    Console.WriteLine($"Image: {image.Width}x{image.Height}");
    var link = await imagesToS3Service.PutImageToS3(stream, request.ImageKey);
    var mlResult = await mlService.PerformMlAsync(link, request.ImageKey);

    return Results.Ok(mlResult);
});
app.MapGroup("images").MapImages(); 
app.MapGroup("archive").MapArchive();
app.MapGrpcService<ImagesService>();

app.Run();

class TestImagesRequest
{
    public string ImageBase64 { get; set; } = default!;
    public string ImageKey { get; set; } = default!;
}