namespace WebPartDashboard.Models;

public class DashboardViewModel
{
    public List<WebPart> WebParts { get; set; } = new();
    public List<WebPartType> AvailableWebParts { get; set; } = new();
}