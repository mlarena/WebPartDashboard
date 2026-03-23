using Newtonsoft.Json;
using WebPartDashboard.Models;
using Microsoft.Extensions.Logging;

namespace WebPartDashboard.Services;

public class WebPartService : IWebPartService
{
    private readonly Dictionary<int, List<WebPart>> _userWebParts = new();
    private int _nextId = 1;
    private readonly ILogger<WebPartService> _logger;

    public WebPartService(ILogger<WebPartService> logger)
    {
        _logger = logger;
        
        // Добавляем демо-данные для первого пользователя
        InitializeDemoData();
        
        _logger.LogInformation($"WebPartService initialized. Users: {_userWebParts.Count}");
    }

    private void InitializeDemoData()
    {
        var demoWebParts = new List<WebPart>
        {
            new()
            {
                Id = _nextId++,
                Title = "Активные проекты",
                Type = WebPartType.DataTable,
                PositionX = 0,
                PositionY = 0,
                Width = 4,
                Height = 3,
                Data = GetDefaultData(WebPartType.DataTable)
            },
            new()
            {
                Id = _nextId++,
                Title = "Статистика выполнения",
                Type = WebPartType.Chart,
                PositionX = 4,
                PositionY = 0,
                Width = 4,
                Height = 3,
                Data = GetDefaultData(WebPartType.Chart)
            },
            new()
            {
                Id = _nextId++,
                Title = "Системные уведомления",
                Type = WebPartType.Informer,
                PositionX = 0,
                PositionY = 3,
                Width = 8,
                Height = 2,
                Data = GetDefaultData(WebPartType.Informer)
            }
        };
        
        _userWebParts[1] = demoWebParts;
        _logger.LogInformation($"Demo data initialized with {demoWebParts.Count} webparts");
        
        foreach (var wp in demoWebParts)
        {
            _logger.LogInformation($"  WebPart ID: {wp.Id}, Title: {wp.Title}, Type: {wp.Type}");
        }
    }

    public Task<List<WebPart>> GetUserWebPartsAsync(int userId)
    {
        if (!_userWebParts.ContainsKey(userId))
        {
            _logger.LogWarning($"User {userId} not found, creating empty list");
            _userWebParts[userId] = new List<WebPart>();
        }
        
        return Task.FromResult(_userWebParts[userId]);
    }

    public Task<WebPart> AddWebPartAsync(int userId, WebPartType type, string title)
    {
        if (!_userWebParts.ContainsKey(userId))
            _userWebParts[userId] = new List<WebPart>();
        
        var webPart = new WebPart
        {
            Id = _nextId++,
            Title = string.IsNullOrEmpty(title) ? GetDefaultTitle(type) : title,
            Type = type,
            PositionX = 0,
            PositionY = _userWebParts[userId].Count,
            Width = 4,
            Height = 3,
            Settings = new Dictionary<string, object>(),
            Data = GetDefaultData(type)
        };

        _userWebParts[userId].Add(webPart);
        
        _logger.LogInformation("Добавлена новая веб-часть: {Title} (ID: {Id})", webPart.Title, webPart.Id);
        
        return Task.FromResult(webPart);
    }

    private string GetDefaultTitle(WebPartType type)
    {
        return type switch
        {
            WebPartType.DataTable => "Таблица данных",
            WebPartType.Chart => "График",
            WebPartType.Informer => "Информер",
            _ => "Новая веб-часть"
        };
    }

    private string GetDefaultData(WebPartType type)
{
    return type switch
    {
        WebPartType.DataTable => JsonConvert.SerializeObject(new
        {
            Columns = new[] { "ID", "Название", "Статус", "Дата создания", "Ответственный" },
            Rows = new[]
            {
                new[] { "1", "Разработка дашборда", "В работе", "2024-01-10", "Иванов И." },
                new[] { "2", "Тестирование модуля", "Завершен", "2024-01-15", "Петров П." },
                new[] { "3", "Внедрение системы", "Планируется", "2024-02-01", "Сидоров С." },
                new[] { "4", "Обучение персонала", "В работе", "2024-01-20", "Кузнецова А." },
                new[] { "5", "Написание документации", "Отложено", "2024-01-25", "Волков В." }
            }
        }),
        
        WebPartType.Chart => JsonConvert.SerializeObject(new
        {
            Type = "bar",
            Labels = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн" },
            Data = new[] { 45, 62, 78, 85, 92, 88 },
            BackgroundColor = "rgba(54, 162, 235, 0.5)",
            BorderColor = "rgba(54, 162, 235, 1)"
        }),
        
        WebPartType.Informer => JsonConvert.SerializeObject(new
        {
            Message = "Добро пожаловать в систему управления проектами!",
            Type = "info",
            LastUpdate = DateTime.Now,
            Details = new[]
            {
                "У вас 3 новых уведомления",
                "2 задачи требуют внимания",
                "Система работает стабильно"
            }
        }),
        
        WebPartType.Tasks => JsonConvert.SerializeObject(new
        {
            Type = "tasks",
            Message = "Tasks webpart"
        }),
        
        _ => "{}"
    };
}

    public Task UpdateWebPartAsync(WebPart webPart)
    {
        foreach (var userWebParts in _userWebParts.Values)
        {
            var existing = userWebParts.FirstOrDefault(w => w.Id == webPart.Id);
            if (existing != null)
            {
                existing.Title = webPart.Title;
                existing.PositionX = webPart.PositionX;
                existing.PositionY = webPart.PositionY;
                existing.Width = webPart.Width;
                existing.Height = webPart.Height;
                existing.Settings = webPart.Settings;
                existing.Data = webPart.Data;
                break;
            }
        }
        
        return Task.CompletedTask;
    }

    public Task RemoveWebPartAsync(int webPartId)
    {
        foreach (var userWebParts in _userWebParts.Values)
        {
            var webPart = userWebParts.FirstOrDefault(w => w.Id == webPartId);
            if (webPart != null)
            {
                userWebParts.Remove(webPart);
                _logger.LogInformation("Удалена веб-часть с ID: {Id}", webPartId);
                break;
            }
        }
        
        return Task.CompletedTask;
    }

    public Task<object> GetWebPartDataAsync(WebPart webPart)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<object>(webPart.Data);
            return Task.FromResult(data ?? new object());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при десериализации данных веб-части {Id}", webPart.Id);
            return Task.FromResult<object>(new { Error = "Ошибка загрузки данных" });
        }
    }
}