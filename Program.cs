using Gameshop_Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using CloudinaryDotNet;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);
var cloudinaryAccount = new Account(
    builder.Configuration["dzicj4dci"],
    builder.Configuration["979858885916716"],
    builder.Configuration["lhH4qrG44s7KvXJEMbPv9cOE1RQ"]
);
// เพิ่ม Cloudinary เป็น Singleton Service
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "gameshop API",
        Version = "v1"
    });
});

// **เพิ่ม CORS **
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "https://gameshop-api-production.up.railway.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseCors("AllowAngular");

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello gameshop");
app.MapGet("/User", async (AppDbContext db) =>
{
    return await db.Users.ToListAsync();
});

app.MapControllers();
app.Run();