using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Data;
using WebPartDashboard.Models.Entities;
using TaskStatus = WebPartDashboard.Models.Entities.TaskStatus;
using TaskPriority = WebPartDashboard.Models.Entities.TaskPriority;

namespace WebPartDashboard.Services.DataProviders;

/// <summary>
/// Сервис для работы с задачами в базе данных PostgreSQL.
/// Реализует бизнес-логику CRUD операций и фильтрации.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получение списка всех задач с поддержкой серверной пагинации.
    /// </summary>
    public async Task<List<TaskItem>> GetAllTasksAsync(int? skip = null, int? take = null)
    {
        var query = _context.Tasks.OrderByDescending(t => t.CreatedAt).AsQueryable();
        
        if (skip.HasValue)
            query = query.Skip(skip.Value);
            
        if (take.HasValue)
            query = query.Take(take.Value);
            
        return await query.ToListAsync();
    }

    /// <summary>
    /// Поиск задачи по её уникальному идентификатору.
    /// </summary>
    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    /// <summary>
    /// Создание новой задачи. Автоматически устанавливает дату создания.
    /// </summary>
    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.Now;
        if (task.DueDate.HasValue)
        {
            task.DueDate = task.DueDate.Value.ToLocalTime();
        }
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Создана задача: {Title} (ID: {Id})", task.Title, task.Id);
        return task;
    }

    /// <summary>
    /// Обновление данных существующей задачи.
    /// </summary>
    public async Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task)
    {
        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
            return null;
            
        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.Priority = task.Priority;
        
        if (task.DueDate.HasValue)
        {
            existingTask.DueDate = task.DueDate.Value.ToLocalTime();
        }
        else
        {
            existingTask.DueDate = null;
        }
        
        existingTask.AssignedTo = task.AssignedTo;
        existingTask.IsCompleted = task.IsCompleted;
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Обновлена задача: {Title} (ID: {Id})", task.Title, id);
        return existingTask;
    }

    /// <summary>
    /// Удаление задачи из базы данных.
    /// </summary>
    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return false;
            
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Удалена задача: {Title} (ID: {Id})", task.Title, id);
        return true;
    }

    /// <summary>
    /// Фильтрация задач по статусу.
    /// </summary>
    public async Task<List<TaskItem>> GetTasksByStatusAsync(TaskStatus status)
    {
        return await _context.Tasks
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Фильтрация задач по приоритету.
    /// </summary>
    public async Task<List<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority)
    {
        return await _context.Tasks
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Получение общего количества задач в базе.
    /// </summary>
    public async Task<int> GetTasksCountAsync()
    {
        return await _context.Tasks.CountAsync();
    }

    /// <summary>
    /// Получение количества выполненных задач.
    /// </summary>
    public async Task<int> GetCompletedTasksCountAsync()
    {
        return await _context.Tasks.CountAsync(t => t.IsCompleted);
    }
}
