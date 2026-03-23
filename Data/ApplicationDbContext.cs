using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Models;
using TaskStatus = WebPartDashboard.Models.TaskStatus;
using TaskPriority = WebPartDashboard.Models.TaskPriority;

namespace WebPartDashboard.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Настройка индексов
        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.Status);
            
        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.AssignedTo);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();            
        // Добавляем демо-данные с фиксированными значениями
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Создать веб-часть для задач",
                Description = "Реализовать CRUD функционал для управления задачами",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                DueDate = new DateTime(2024, 1, 18, 10, 0, 0, DateTimeKind.Utc),
                AssignedTo = "Разработчик",
                IsCompleted = false
            },
            new TaskItem
            {
                Id = 2,
                Title = "Настроить базу данных PostgreSQL",
                Description = "Установить и настроить подключение к PostgreSQL",
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Medium,
                CreatedAt = new DateTime(2024, 1, 13, 10, 0, 0, DateTimeKind.Utc),
                DueDate = new DateTime(2024, 1, 14, 10, 0, 0, DateTimeKind.Utc),
                AssignedTo = "Администратор",
                IsCompleted = true
            },
            new TaskItem
            {
                Id = 3,
                Title = "Протестировать CRUD операции",
                Description = "Провести тестирование всех CRUD операций",
                Status = TaskStatus.New,
                Priority = TaskPriority.Medium,
                CreatedAt = new DateTime(2024, 1, 16, 10, 0, 0, DateTimeKind.Utc),
                DueDate = new DateTime(2024, 1, 18, 10, 0, 0, DateTimeKind.Utc),
                AssignedTo = "Тестировщик",
                IsCompleted = false
            }
        );
    }
}