using WebPartDashboard.Models.Entities;
using TaskStatus = WebPartDashboard.Models.Entities.TaskStatus;
using TaskPriority = WebPartDashboard.Models.Entities.TaskPriority;

namespace WebPartDashboard.Services.DataProviders;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllTasksAsync(int? skip = null, int? take = null);
    Task<TaskItem?> GetTaskByIdAsync(int id);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task);
    Task<bool> DeleteTaskAsync(int id);
    Task<List<TaskItem>> GetTasksByStatusAsync(TaskStatus status);
    Task<List<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority);
    Task<int> GetTasksCountAsync();
    Task<int> GetCompletedTasksCountAsync();
}