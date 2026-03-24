using Microsoft.AspNetCore.Mvc;
using WebPartDashboard.Models;
using WebPartDashboard.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace WebPartDashboard.Controllers;

/// <summary>
/// Основной контроллер дашборда.
/// Отвечает за отображение главной страницы и управление конфигурацией веб-частей.
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly IWebPartService _webPartService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IWebPartService webPartService, ILogger<DashboardController> logger)
    {
        _webPartService = webPartService;
        _logger = logger;
    }

    /// <summary>
    /// Главная страница дашборда. Загружает список веб-частей текущего пользователя.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var webParts = await _webPartService.GetUserWebPartsAsync(userId);
        
        var model = new DashboardViewModel
        {
            WebParts = webParts,
            AvailableWebParts = Enum.GetValues<WebPartType>().ToList()
        };
        
        return View(model);
    }

    /// <summary>
    /// Добавление новой веб-части на дашборд.
    /// Возвращает частичное представление карточки веб-части.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddWebPart(WebPartType type, string title)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webPart = await _webPartService.AddWebPartAsync(userId, type, title);
            
            return PartialView("_WebPart", webPart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении веб-части");
            return BadRequest("Ошибка при добавлении веб-части");
        }
    }

    /// <summary>
    /// Удаление веб-части из хранилища.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RemoveWebPart(int webPartId)
    {
        try
        {
            await _webPartService.RemoveWebPartAsync(webPartId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении веб-части {Id}", webPartId);
            return BadRequest("Ошибка при удалении веб-части");
        }
    }

    /// <summary>
    /// Обновление координат (X, Y) веб-части при перетаскивании.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateWebPartPosition(int webPartId, int x, int y)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            
            if (webPart != null)
            {
                webPart.PositionX = x;
                webPart.PositionY = y;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении позиции веб-части {Id}", webPartId);
            return BadRequest("Ошибка при обновлении позиции");
        }
    }

    /// <summary>
    /// Получение данных для веб-части (JSON).
    /// Используется для графиков, таблиц и информеров.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWebPartData(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == id);
            
            if (webPart == null)
                return NotFound(new { error = $"WebPart {id} not found" });
            
            var data = await _webPartService.GetWebPartDataAsync(webPart);
            return Json(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении данных веб-части {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Обновление заголовка веб-части.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateWebPartTitle(int webPartId, string title)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            
            if (webPart != null)
            {
                webPart.Title = title;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении заголовка веб-части {Id}", webPartId);
            return BadRequest("Ошибка при обновлении заголовка");
        }
    }

    /// <summary>
    /// Обновление размеров (ширина, высота) веб-части.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateWebPartSize(int webPartId, int width, int height)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            
            if (webPart != null)
            {
                webPart.Width = width;
                webPart.Height = height;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении размера веб-части {Id}", webPartId);
            return BadRequest("Ошибка при обновлении размера");
        }
    }

    /// <summary>
    /// Заглушка для получения ID текущего пользователя.
    /// В реальном приложении берется из Claims.
    /// </summary>
    private int GetCurrentUserId()
    {
        return 1;
    }
}

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var webParts = await _webPartService.GetUserWebPartsAsync(userId);
        
        var model = new DashboardViewModel
        {
            WebParts = webParts,
            AvailableWebParts = Enum.GetValues<WebPartType>().ToList()
        };
        
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddWebPart(WebPartType type, string title)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webPart = await _webPartService.AddWebPartAsync(userId, type, title);
            
            return PartialView("_WebPart", webPart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении веб-части");
            return BadRequest("Ошибка при добавлении веб-части");
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveWebPart(int webPartId)
    {
        try
        {
            await _webPartService.RemoveWebPartAsync(webPartId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении веб-части {Id}", webPartId);
            return BadRequest("Ошибка при удалении веб-части");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWebPartPosition(int webPartId, int x, int y)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            
            if (webPart != null)
            {
                webPart.PositionX = x;
                webPart.PositionY = y;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении позиции веб-части {Id}", webPartId);
            return BadRequest("Ошибка при обновлении позиции");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetWebPartData(int id)
    {
        try
        {
            Console.WriteLine($"=== GetWebPartData called ===");
            Console.WriteLine($"WebPartId: {id}");
            
            var userId = GetCurrentUserId();
            Console.WriteLine($"UserId: {userId}");
            
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            Console.WriteLine($"Total webparts for user: {webParts.Count}");
            
            foreach (var wp in webParts)
            {
                Console.WriteLine($"  - ID: {wp.Id}, Title: {wp.Title}, Type: {wp.Type}");
            }
            
            var webPart = webParts.FirstOrDefault(w => w.Id == id);
            
            if (webPart == null)
            {
                Console.WriteLine($"ERROR: WebPart {id} not found!");
                return NotFound(new { error = $"WebPart {id} not found" });
            }
            
            Console.WriteLine($"Found webpart: {webPart.Title}");
            
            var data = await _webPartService.GetWebPartDataAsync(webPart);
            Console.WriteLine($"Data retrieved successfully");
            
            return Json(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWebPartTitle(int webPartId, string title)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            
            if (webPart != null)
            {
                webPart.Title = title;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении заголовка веб-части {Id}", webPartId);
            return BadRequest("Ошибка при обновлении заголовка");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWebPartSize(int webPartId, int width, int height)
    {
        try
        {
            var userId = GetCurrentUserId();
            var webParts = await _webPartService.GetUserWebPartsAsync(userId);
            var webPart = webParts.FirstOrDefault(w => w.Id == webPartId);
            
            if (webPart != null)
            {
                webPart.Width = width;
                webPart.Height = height;
                await _webPartService.UpdateWebPartAsync(webPart);
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении размера веб-части {Id}", webPartId);
            return BadRequest("Ошибка при обновлении размера");
        }
    }

    private int GetCurrentUserId()
    {
        return 1;
    }
}