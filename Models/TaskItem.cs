using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPartDashboard.Models;

public class TaskItem
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Название обязательно")]
    [Display(Name = "Название задачи")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Display(Name = "Описание")]
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Display(Name = "Статус")]
    public TaskStatus Status { get; set; } = TaskStatus.New;
    
    [Display(Name = "Приоритет")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Срок выполнения")]
    public DateTime? DueDate { get; set; }
    
    [Display(Name = "Ответственный")]
    [StringLength(100)]
    public string? AssignedTo { get; set; }
    
    [Display(Name = "Выполнено")]
    public bool IsCompleted { get; set; }
}

public enum TaskStatus
{
    [Display(Name = "Новая")]
    New,
    
    [Display(Name = "В работе")]
    InProgress,
    
    [Display(Name = "На проверке")]
    Review,
    
    [Display(Name = "Завершена")]
    Completed,
    
    [Display(Name = "Отложена")]
    Cancelled
}

public enum TaskPriority
{
    [Display(Name = "Низкий")]
    Low,
    
    [Display(Name = "Средний")]
    Medium,
    
    [Display(Name = "Высокий")]
    High,
    
    [Display(Name = "Критический")]
    Critical
}