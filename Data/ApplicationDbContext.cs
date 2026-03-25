using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Models;
using WebPartDashboard.Models.Entities;
using TaskStatus = WebPartDashboard.Models.Entities.TaskStatus;
using TaskPriority = WebPartDashboard.Models.Entities.TaskPriority;
namespace WebPartDashboard.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<MonitoringPost> MonitoringPosts { get; set; }
    public DbSet<SensorType> SensorTypes { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка схем для новых таблиц
        modelBuilder.Entity<MonitoringPost>().ToTable("MonitoringPost", "public");
        modelBuilder.Entity<SensorType>().ToTable("SensorType", "public");
        modelBuilder.Entity<Sensor>().ToTable("Sensor", "public");
        
        // Настройка индексов
        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.Status);
            
        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.AssignedTo);

        // Добавляем базовые типы датчиков с фиксированными датами
        modelBuilder.Entity<SensorType>().HasData(
            new SensorType { Id = 1, SensorTypeName = "Температура", Description = "Датчик температуры воздуха", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SensorType { Id = 2, SensorTypeName = "Влажность", Description = "Датчик влажности воздуха", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SensorType { Id = 3, SensorTypeName = "Давление", Description = "Датчик атмосферного давления", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
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