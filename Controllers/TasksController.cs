using Microsoft.AspNetCore.Mvc;
using WebPartDashboard.Services;

namespace WebPartDashboard.Controllers;

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
            var tasks = await _taskService.GetAllTasksAsync();
            return PartialView("_TasksWebPart", tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке веб-части задач");
            return Content("<div class='alert alert-danger'>Ошибка загрузки задач</div>");
        }
    }
}