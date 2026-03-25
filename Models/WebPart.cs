using Newtonsoft.Json;

namespace WebPartDashboard.Models;

public class WebPart
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public WebPartType Type { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 3;
    public Dictionary<string, object> Settings { get; set; } = new();
    public string Data { get; set; } = "{}";
    
    public int UserId { get; set; }
}