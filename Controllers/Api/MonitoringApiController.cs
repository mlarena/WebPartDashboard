using Microsoft.AspNetCore.Mvc;
using WebPartDashboard.Models.Entities;
using WebPartDashboard.Services.DataProviders;

namespace WebPartDashboard.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class MonitoringApiController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;

    public MonitoringApiController(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts() => Ok(await _monitoringService.GetAllPostsAsync());

    [HttpGet("posts/{id}")]
    public async Task<IActionResult> GetPost(int id) => Ok(await _monitoringService.GetPostByIdAsync(id));

    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost(MonitoringPost post) => Ok(await _monitoringService.CreatePostAsync(post));

    [HttpPut("posts/{id}")]
    public async Task<IActionResult> UpdatePost(int id, MonitoringPost post) => Ok(await _monitoringService.UpdatePostAsync(id, post));

    [HttpDelete("posts/{id}")]
    public async Task<IActionResult> DeletePost(int id) => Ok(await _monitoringService.DeletePostAsync(id));

    [HttpGet("sensor-types")]
    public async Task<IActionResult> GetSensorTypes() => Ok(await _monitoringService.GetSensorTypesAsync());

    [HttpGet("posts/{postId}/sensors")]
    public async Task<IActionResult> GetSensors(int postId) => Ok(await _monitoringService.GetSensorsByPostIdAsync(postId));

    [HttpPost("sensors")]
    public async Task<IActionResult> AddSensor(Sensor sensor) => Ok(await _monitoringService.AddSensorAsync(sensor));

    [HttpDelete("sensors/{id}")]
    public async Task<IActionResult> DeleteSensor(int id) => Ok(await _monitoringService.DeleteSensorAsync(id));
}
