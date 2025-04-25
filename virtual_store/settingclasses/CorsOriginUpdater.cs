using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using virtual_store.Models;

public class CorsOriginUpdater : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public static List<string> AllowedOrigins { get; private set; } = new List<string>();

    public CorsOriginUpdater(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public static DateTime PST()
    {
        DateTime utcNow = DateTime.UtcNow;

        // Define the Pakistan Standard Time zone (use "Asia/Karachi" as an alternative ID)
        TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");

        // Convert the UTC time to Pakistan Standard Time
        DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pakistanTimeZone);

        // Assign it to your desired variable
        var tds = pakistanTime;
        return tds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<VirtualStoreContext>();
                var origins = await dbContext.AccessDetails.Where(m=>m.AccessStatus=="Active").Select(a => a.AccessLink).ToListAsync(stoppingToken);
   
                // Update allowed origins
             
                AllowedOrigins = origins;
                Console.WriteLine($"Updated CORS Origins at {DateTime.Now}: {string.Join(", ", origins)}");
            }

            // Wait for 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    
}
