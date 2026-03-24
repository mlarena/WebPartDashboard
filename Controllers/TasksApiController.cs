using Microsoft.AspNetCore.Mvc;
using WebPartDashboard.Models;
using WebPartDashboard.Services;

namespace WebPartDashboard.Controllers;

/// <summary>
/// API контроллер для управления задачами.
/// Обеспечивает CRUD операции, получение статистики и пагинацию.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksApiController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksApiController> _logger;

    public TasksApiController(ITaskService taskService, ILogger<TasksApiController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Получение списка задач с поддержкой пагинации.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks(int? skip = null, int? take = null)
    {
        try
        {
            var tasks = await _taskService.GetAllTasksAsync(skip, take);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении задач");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Получение конкретной задачи по ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();
                
            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении задачи {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Создание новой задачи.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
    {
        try
        {
            var created = await _taskService.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании задачи");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Обновление существующей задачи.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItem task)
    {
        try
        {
            if (id != task.Id)
                return BadRequest();
                
            var updated = await _taskService.UpdateTaskAsync(id, task);
            if (updated == null)
                return NotFound();
                
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении задачи {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Удаление задачи.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result)
                return NotFound();
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении задачи {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Получение статистики по задачам.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var total = await _taskService.GetTasksCountAsync();
            var completed = await _taskService.GetCompletedTasksCountAsync();
            
            return Ok(new
            {
                total,
                completed,
                inProgress = total - completed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
