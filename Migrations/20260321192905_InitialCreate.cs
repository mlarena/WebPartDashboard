using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebPartDashboard.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssignedTo", "CreatedAt", "Description", "DueDate", "IsCompleted", "Priority", "Status", "Title" },
                values: new object[,]
                {
                    { 1, "Разработчик", new DateTime(2024, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Реализовать CRUD функционал для управления задачами", new DateTime(2024, 1, 18, 10, 0, 0, 0, DateTimeKind.Utc), false, 2, 1, "Создать веб-часть для задач" },
                    { 2, "Администратор", new DateTime(2024, 1, 13, 10, 0, 0, 0, DateTimeKind.Utc), "Установить и настроить подключение к PostgreSQL", new DateTime(2024, 1, 14, 10, 0, 0, 0, DateTimeKind.Utc), true, 1, 3, "Настроить базу данных PostgreSQL" },
                    { 3, "Тестировщик", new DateTime(2024, 1, 16, 10, 0, 0, 0, DateTimeKind.Utc), "Провести тестирование всех CRUD операций", new DateTime(2024, 1, 18, 10, 0, 0, 0, DateTimeKind.Utc), false, 1, 0, "Протестировать CRUD операции" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedTo",
                table: "Tasks",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Status",
                table: "Tasks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
