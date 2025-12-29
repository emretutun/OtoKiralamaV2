using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CarAvailabilityBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CarAvailabilityBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // ✅ DateOnly ile çalış (sipariş tarihlerin DateOnly ise)
            var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);

            // ✅ Bitiş tarihi geçmiş ve durum onaylı olan siparişlerdeki araçları serbest bırak
            var ordersToRelease = await context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Car)
                .Where(o => o.Status == "Onaylandı" && o.EndDate < todayUtc)
                .ToListAsync(stoppingToken);

            foreach (var order in ordersToRelease)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Car.IsAvailable = true;
                }

                order.Status = "Tamamlandı";
            }

            await context.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
