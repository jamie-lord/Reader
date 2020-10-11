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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RefreshAllFeeds(stoppingToken);
        }

        private async Task RefreshAllFeeds(CancellationToken stoppingToken)
        {
            using (var scope = Services.CreateScope())
            {
                var feedsService = scope.ServiceProvider.GetRequiredService<IFeedsService>();
                await feedsService.RefreshAllFeeds(stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
