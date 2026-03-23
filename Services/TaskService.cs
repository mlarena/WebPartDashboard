using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Data;
using WebPartDashboard.Models;
using TaskStatus = WebPartDashboard.Models.TaskStatus;
using TaskPriority = WebPartDashboard.Models.TaskPriority;

namespace WebPartDashboard.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        return await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Создана задача: {Title} (ID: {Id})", task.Title, task.Id);
        return task;
    }

    public async Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task)
    {
        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
            return null;
            
        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.AssignedTo = task.AssignedTo;
        existingTask.IsCompleted = task.IsCompleted;
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Обновлена задача: {Title} (ID: {Id})", task.Title, id);
        return existingTask;
    }

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

    public async Task<List<TaskItem>> GetTasksByStatusAsync(TaskStatus status)
    {
        return await _context.Tasks
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority)
    {
        return await _context.Tasks
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTasksCountAsync()
    {
        return await _context.Tasks.CountAsync();
    }

    public async Task<int> GetCompletedTasksCountAsync()
    {
        return await _context.Tasks.CountAsync(t => t.IsCompleted);
    }
}