using WebPartDashboard.Models;
using TaskStatus = WebPartDashboard.Models.TaskStatus;
using TaskPriority = WebPartDashboard.Models.TaskPriority;

namespace WebPartDashboard.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllTasksAsync();
    Task<TaskItem?> GetTaskByIdAsync(int id);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task);
    Task<bool> DeleteTaskAsync(int id);
    Task<List<TaskItem>> GetTasksByStatusAsync(TaskStatus status);
    Task<List<TaskItem>> GetTasksByPriorityAsync(TaskPriority priority);
    Task<int> GetTasksCountAsync();
    Task<int> GetCompletedTasksCountAsync();
}