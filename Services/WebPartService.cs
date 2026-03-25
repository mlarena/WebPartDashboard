using Newtonsoft.Json;
using WebPartDashboard.Models;
using WebPartDashboard.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebPartDashboard.Services;

/// <summary>
/// Сервис для управления конфигурацией и данными веб-частей.
/// Теперь использует базу данных PostgreSQL для хранения настроек пользователей.
/// </summary>
public class WebPartService : IWebPartService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WebPartService> _logger;

    public WebPartService(ApplicationDbContext context, ILogger<WebPartService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<WebPart>> GetUserWebPartsAsync(int userId)
    {
        var webParts = await _context.WebParts
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.PositionY)
            .ThenBy(w => w.PositionX)
            .ToListAsync();

        // Если у пользователя нет веб-частей, создаем дефолтный набор
        if (!webParts.Any())
        {
            await InitializeDefaultWebPartsAsync(userId);
            webParts = await _context.WebParts.Where(w => w.UserId == userId).ToListAsync();
        }

        return webParts;
    }

    private async Task InitializeDefaultWebPartsAsync(int userId)
    {
        var defaults = new List<WebPart>
        {
            new() { UserId = userId, Title = "Активные проекты", Type = WebPartType.DataTable, PositionX = 0, PositionY = 0, Width = 6, Height = 4, Data = GetDefaultData(WebPartType.DataTable) },
            new() { UserId = userId, Title = "Статистика", Type = WebPartType.Chart, PositionX = 6, PositionY = 0, Width = 6, Height = 4, Data = GetDefaultData(WebPartType.Chart) },
            new() { UserId = userId, Title = "Задачи", Type = WebPartType.Tasks, PositionX = 0, PositionY = 4, Width = 12, Height = 5, Data = GetDefaultData(WebPartType.Tasks) }
        };

        _context.WebParts.AddRange(defaults);
        await _context.SaveChangesAsync();
    }

    public async Task<WebPart> AddWebPartAsync(int userId, WebPartType type, string title)
    {
        var webPart = new WebPart
        {
            UserId = userId,
            Title = string.IsNullOrEmpty(title) ? GetDefaultTitle(type) : title,
            Type = type,
            PositionX = 0,
            PositionY = 100, // В конец
            Width = (type == WebPartType.Tasks || type == WebPartType.Monitoring) ? 12 : 6,
            Height = (type == WebPartType.Tasks || type == WebPartType.Monitoring) ? 5 : 4,
            Data = GetDefaultData(type)
        };

        _context.WebParts.Add(webPart);
        await _context.SaveChangesAsync();
        return webPart;
    }

    public async Task UpdateWebPartAsync(WebPart webPart)
    {
        var existing = await _context.WebParts.FindAsync(webPart.Id);
        if (existing != null)
        {
            existing.Title = webPart.Title;
            existing.PositionX = webPart.PositionX;
            existing.PositionY = webPart.PositionY;
            existing.Width = webPart.Width;
            existing.Height = webPart.Height;
            existing.Settings = webPart.Settings;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveWebPartAsync(int webPartId)
    {
        var webPart = await _context.WebParts.FindAsync(webPartId);
        if (webPart != null)
        {
            _context.WebParts.Remove(webPart);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<object> GetWebPartDataAsync(WebPart webPart)
    {
        try
        {
            return JsonConvert.DeserializeObject<object>(webPart.Data) ?? new object();
        }
        catch
        {
            return new { Error = "Ошибка данных" };
        }
    }

    private string GetDefaultTitle(WebPartType type) => type switch {
        WebPartType.DataTable => "Таблица данных",
        WebPartType.Chart => "График",
        WebPartType.Informer => "Информер",
        WebPartType.Tasks => "Задачи",
        WebPartType.Monitoring => "Мониторинг",
        _ => "Новая панель"
    };

    private string GetDefaultData(WebPartType type) => type switch {
        WebPartType.DataTable => JsonConvert.SerializeObject(new { Columns = new[] { "ID", "Имя" }, Rows = new[] { new[] { "1", "Демо" } } }),
        WebPartType.Chart => JsonConvert.SerializeObject(new { type = "bar", labels = new[] { "А", "Б" }, data = new[] { 10, 20 } }),
        WebPartType.Informer => JsonConvert.SerializeObject(new { message = "Система готова", type = "info" }),
        _ => "{}"
    };
}
