using ErayBarbekuSomine.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    var mappings = new Dictionary<string, string>
    {
        { "duzcepheli", "DuzCepheliSomine" },
        { "kosesomine", "KoseSomine" },
        { "ltipisomine", "LTipiSomine" },
        { "truvabarbeku", "TruvaBarbeku" },
        { "truvacifttarafli", "TruvaCiftTarafli" },
        { "truvatektaraflı", "TruvaTekTarafli" },
        { "utipi", "UTipiSomine" }
    };

    foreach (var map in mappings)
    {
        var folderDir = Path.Combine(env.WebRootPath, "images", map.Key);
        if (Directory.Exists(folderDir))
        {
            var files = Directory.GetFiles(folderDir);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var filePath = $"/images/{map.Key}/{fileName}";

                if (!context.Images.Any(i => i.FilePath == filePath))
                {
                    context.Images.Add(new Image
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        Category = map.Value,
                        UploadDate = DateTime.Now
                    });
                }
            }
        }
    }
    context.SaveChanges();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
