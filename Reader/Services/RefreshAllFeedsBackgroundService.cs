using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reader.Services
{
    public class RefreshAllFeedsBackgroundService : BackgroundService
    {
        public RefreshAllFeedsBackgroundService(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        private const int RefreshDelay = 1000 * 60 * 20; // 20 minutes

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = Services.CreateScope())
                {
                    var feedsService = scope.ServiceProvider.GetRequiredService<IFeedsService>();
                    await feedsService.RefreshAllFeeds();
                }

                await Task.Delay(RefreshDelay, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
