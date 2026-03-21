using WebPartDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IWebPartService, WebPartService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Выводим информацию о запуске
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("=== Application Started ===");
    using (var scope = app.Services.CreateScope())
    {
        var service = scope.ServiceProvider.GetRequiredService<IWebPartService>();
        var webParts = service.GetUserWebPartsAsync(1).Result;
        Console.WriteLine($"User 1 has {webParts.Count} webparts at startup");
    }
});

app.Run();