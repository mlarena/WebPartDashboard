using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Data;
using WebPartDashboard.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Включаем поддержку старого поведения DateTime для PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// Настройка сессий (для хранения JWT)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Добавляем DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка JWT Аутентификации
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    
    // Извлекаем токен из сессии
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Если не авторизован, перенаправляем на страницу логина
            context.HandleResponse();
            context.Response.Redirect("/Auth/Login");
            return Task.CompletedTask;
        }
    };
});

// Регистрируем сервисы
builder.Services.AddScoped<IWebPartService, WebPartService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseSession(); // Важно: перед UseAuthentication

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllers();

// Создаем базу данных при запуске
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();