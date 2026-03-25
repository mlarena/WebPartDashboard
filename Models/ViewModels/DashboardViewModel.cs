namespace WebPartDashboard.Models.ViewModels;

using WebPartDashboard.Models;
using WebPartDashboard.Models.Entities;

public class DashboardViewModel
{
    public List<WebPart> WebParts { get; set; } = new();
    public List<WebPartType> AvailableWebParts { get; set; } = new();
}