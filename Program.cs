using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using VideoManagementApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register VideoService as scoped service following Dependency Injection pattern
builder.Services.AddScoped<IVideoService, VideoService>();

// Configure Kestrel server options for 200MB upload limit (only for API endpoints)
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 209715200; // 200 MB in bytes
});

// Configure IIS server options for 200MB upload limit (when hosting on IIS)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 209715200; // 200 MB in bytes
});

// Configure form options with increased limits for multipart form data
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 209715200; // 200 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enable static files middleware to serve videos, CSS, JS, images
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configure default route to Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure media directory exists on application startup
var mediaPath = Path.Combine(app.Environment.WebRootPath, "media");
if (!Directory.Exists(mediaPath))
{
    Directory.CreateDirectory(mediaPath);
}

app.Run();
