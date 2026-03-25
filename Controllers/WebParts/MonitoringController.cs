using Microsoft.AspNetCore.Mvc;
using WebPartDashboard.Services.DataProviders;

namespace WebPartDashboard.Controllers.WebParts;

public class MonitoringController : Controller
{
    private readonly IMonitoringService _monitoringService;

    public MonitoringController(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    public async Task<IActionResult> GetMonitoringWebPart()
    {
        try 
        {
            var posts = await _monitoringService.GetAllPostsAsync();
            return PartialView("~/Views/Shared/WebParts/_Monitoring.cshtml", posts);
        }
        catch (Exception ex)
        {
            return Content($"<div class='alert alert-danger'>Ошибка сервера: {ex.Message}</div>");
        }
    }
}
