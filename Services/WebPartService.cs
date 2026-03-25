using Newtonsoft.Json;
using WebPartDashboard.Models;
using Microsoft.Extensions.Logging;

namespace WebPartDashboard.Services;

/// <summary>
/// Сервис для управления конфигурацией и данными веб-частей.
/// Использует In-memory хранилище для демонстрационных целей.
/// </summary>
public class WebPartService : IWebPartService
{
    // Хранилище веб-частей в памяти: Dictionary<UserId, List<WebPart>>
    private static readonly Dictionary<int, List<WebPart>> _userWebParts = new();
    private static int _nextId = 1;
    private static readonly object _lock = new object();
    private readonly ILogger<WebPartService> _logger;

    public WebPartService(ILogger<WebPartService> logger)
    {
        _logger = logger;
        
        lock (_lock)
        {
            // Инициализация демо-данных при первом запуске
            if (_userWebParts.Count == 0)
            {
                InitializeDemoData();
            }
        }
    }

    /// <summary>
    /// Создание начального набора веб-частей для демонстрации.
    /// </summary>
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
        _logger.LogInformation("Демо-данные инициализированы: {Count} веб-частей", demoWebParts.Count);
    }

    /// <summary>
    /// Получение всех веб-частей конкретного пользователя.
    /// </summary>
    public Task<List<WebPart>> GetUserWebPartsAsync(int userId)
    {
        lock (_lock)
        {
            if (!_userWebParts.ContainsKey(userId))
                _userWebParts[userId] = new List<WebPart>();
            
            return Task.FromResult(_userWebParts[userId].ToList());
        }
    }

    /// <summary>
    /// Добавление новой веб-части с параметрами по умолчанию.
    /// </summary>
    public Task<WebPart> AddWebPartAsync(int userId, WebPartType type, string title)
    {
        lock (_lock)
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
                Width = (type == WebPartType.Tasks || type == WebPartType.Monitoring) ? 12 : 6,
                Height = (type == WebPartType.Tasks || type == WebPartType.Monitoring) ? 5 : 4,
                Settings = new Dictionary<string, object>(),
                Data = GetDefaultData(type)
            };
            _userWebParts[userId].Add(webPart);
            _logger.LogInformation("Добавлена веб-часть: {Title} (ID: {Id})", webPart.Title, webPart.Id);
            
            return Task.FromResult(webPart);
        }
    }

    /// <summary>
    /// Возвращает заголовок по умолчанию для типа веб-части.
    /// </summary>
    private string GetDefaultTitle(WebPartType type)
    {
        return type switch
        {
            WebPartType.DataTable => "Таблица данных",
            WebPartType.Chart => "График",
            WebPartType.Informer => "Информер",
            WebPartType.Tasks => "Управление задачами",
            WebPartType.Monitoring => "Посты мониторинга",
            _ => "Новая веб-часть"
        };
    }
    /// <summary>
    /// Генерирует демонстрационные данные в формате JSON для разных типов веб-частей.
    /// </summary>
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
                type = "bar",
                labels = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн" },
                data = new[] { 45, 62, 78, 85, 92, 88 },
                backgroundColor = "rgba(54, 162, 235, 0.5)",
                borderColor = "rgba(54, 162, 235, 1)"
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
            
            WebPartType.Tasks => JsonConvert.SerializeObject(new { Type = "tasks" }),
            WebPartType.Monitoring => JsonConvert.SerializeObject(new { Type = "monitoring" }),
            _ => "{}"
        };
    }
    /// <summary>
    /// Обновление параметров веб-части (заголовок, позиция, размер).
    /// </summary>
    public Task UpdateWebPartAsync(WebPart webPart)
    {
        lock (_lock)
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
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Удаление веб-части из списка пользователя.
    /// </summary>
    public Task RemoveWebPartAsync(int webPartId)
    {
        lock (_lock)
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
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Десериализация данных веб-части для отправки на клиент.
    /// </summary>
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