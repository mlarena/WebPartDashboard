using Microsoft.AspNetCore.Mvc;
using WebPartDashboard.Services.DataProviders;

namespace WebPartDashboard.Controllers.WebParts;

public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasksWebPart()
    {
        try
        {
            var tasks = await _taskService.GetAllTasksAsync(0, 10);
            return PartialView("~/Views/Shared/WebParts/_Tasks.cshtml", tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке веб-части задач");
            return Content("<div class='alert alert-danger'>Ошибка загрузки задач</div>");
        }
    }
}
