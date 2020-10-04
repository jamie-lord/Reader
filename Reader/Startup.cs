using CodeHollow.FeedReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reader.Data;
using Reader.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Reader
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddDbContext<Context>();

            services.AddHostedService<RefreshAllFeedsBackgroundService>();
            services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }

    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    internal class ScopedProcessingService : IScopedProcessingService
    {
        private readonly Context _context;

        public ScopedProcessingService(Context context)
        {
            _context = context;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var feeds = _context.Feeds.ToList();

                foreach (var feed in feeds)
                {
                    try
                    {
                        var f = await FeedReader.ReadAsync(feed.Uri);

                        feed.LastChecked = DateTime.Now;
                        _context.Feeds.Update(feed);
                        await _context.SaveChangesAsync();

                        foreach (var item in f.Items)
                        {
                            if (_context.Items.Any(x => x.Uri == item.Link))
                            {
                                // Item already exists from previous fetch or another feed
                                continue;
                            }

                            var newItem = new Item
                            {
                                Author = item.Author,
                                Categories = item.Categories?.ToList(),
                                Content = item.Content,
                                Description = item.Description,
                                Feed = feed,
                                Published = item.PublishingDate,
                                Title = item.Title,
                                Uri = item.Link
                            };
                            _context.Items.Add(newItem);
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                await Task.Delay(1000 * 60 * 10, stoppingToken);
            }
        }
    }

    public class RefreshAllFeedsBackgroundService : BackgroundService
    {
        public RefreshAllFeedsBackgroundService(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();
                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
