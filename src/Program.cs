using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
var authScheme = CookieAuthenticationDefaults.AuthenticationScheme;
builder.Services.AddAuthentication(authScheme).AddCookie(authScheme, options =>
{
    // options.Cookie.IsEssential = true;
    // options.Cookie.HttpOnly = true;
    // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // options.Cookie.SameSite = SameSiteMode.None;
    options.LoginPath = new PathString("/Account/Index");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
// Set up custom content types - associating file extension to MIME type
var provider = new FileExtensionContentTypeProvider();
// Add new mappings
provider.Mappings[".less"] = "text/less";
// Replace an existing mapping
// provider.Mappings[".rtf"] = "application/x-msdownload";
// Remove MP4 videos.
//provider.Mappings.Remove(".mp4");

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
app.UseStaticFiles();
app.UseRouting();

// 留意一下顺序，顺序不对会导致Authorize未登录
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseStaticHttpContext();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
