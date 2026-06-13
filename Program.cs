using UrlShortner.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddNpgsqlDataSource("url-shortener");

builder.AddRedisDistributedCache("redis");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddScoped<UrlShorteningService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });
}

app.MapPost("shorten", async (string url, UrlShorteningService urlShorteningService) =>
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("Invalid URL format");
    }

    var shortCode = await urlShorteningService.ShortenUrl(url);

    return Results.Ok(new { shortCode });
});

app.MapGet("{shortCode}", async (string shortCode, UrlShorteningService urlShorteningService) =>
{
    var originalUrl = await urlShorteningService.GetOriginalUrl(shortCode);

    return originalUrl is null ? Results.NotFound() : Results.Redirect(originalUrl);
});

app.MapGet("urls", async (UrlShorteningService urlShorteningService) =>
{
    var urls = await urlShorteningService.GetAllUrls();

    return urls is null ? Results.NotFound() : Results.Ok(urls);
});

app.UseHttpsRedirection();

app.Run();
