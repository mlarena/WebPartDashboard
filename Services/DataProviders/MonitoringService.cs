using Microsoft.EntityFrameworkCore;
using WebPartDashboard.Data;
using WebPartDashboard.Models.Entities;

namespace WebPartDashboard.Services.DataProviders;
public interface IMonitoringService
{
    Task<List<MonitoringPost>> GetAllPostsAsync();
    Task<MonitoringPost?> GetPostByIdAsync(int id);
    Task<MonitoringPost> CreatePostAsync(MonitoringPost post);
    Task<MonitoringPost?> UpdatePostAsync(int id, MonitoringPost post);
    Task<bool> DeletePostAsync(int id);
    
    Task<List<SensorType>> GetSensorTypesAsync();
    Task<List<Sensor>> GetSensorsByPostIdAsync(int postId);
    Task<Sensor> AddSensorAsync(Sensor sensor);
    Task<bool> DeleteSensorAsync(int id);
}

public class MonitoringService : IMonitoringService
{
    private readonly ApplicationDbContext _context;

    public MonitoringService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MonitoringPost>> GetAllPostsAsync()
    {
        return await _context.MonitoringPosts
            .Include(p => p.Sensors)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<MonitoringPost?> GetPostByIdAsync(int id)
    {
        return await _context.MonitoringPosts
            .Include(p => p.Sensors)
            .ThenInclude(s => s.SensorType)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<MonitoringPost> CreatePostAsync(MonitoringPost post)
    {
        post.CreatedAt = DateTime.Now;
        _context.MonitoringPosts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<MonitoringPost?> UpdatePostAsync(int id, MonitoringPost post)
    {
        var existing = await _context.MonitoringPosts.FindAsync(id);
        if (existing == null) return null;

        existing.Name = post.Name;
        existing.Description = post.Description;
        existing.Longitude = post.Longitude;
        existing.Latitude = post.Latitude;
        existing.IsMobile = post.IsMobile;
        existing.IsActive = post.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var post = await _context.MonitoringPosts.FindAsync(id);
        if (post == null) return false;

        _context.MonitoringPosts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SensorType>> GetSensorTypesAsync()
    {
        return await _context.SensorTypes.OrderBy(t => t.SensorTypeName).ToListAsync();
    }

    public async Task<List<Sensor>> GetSensorsByPostIdAsync(int postId)
    {
        return await _context.Sensors
            .Include(s => s.SensorType)
            .Where(s => s.MonitoringPostId == postId)
            .ToListAsync();
    }

    public async Task<Sensor> AddSensorAsync(Sensor sensor)
    {
        sensor.CreatedAt = DateTime.Now;
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        return sensor;
    }

    public async Task<bool> DeleteSensorAsync(int id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor == null) return false;

        _context.Sensors.Remove(sensor);
        await _context.SaveChangesAsync();
        return true;
    }
}
