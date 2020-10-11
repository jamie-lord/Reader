using CodeHollow.FeedReader;
using Reader.Data;
using Reader.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Reader.Services
{
    public interface IFeedsService
    {
        Task RefreshAllFeeds(CancellationToken stoppingToken);
    }

    public class FeedsService : IFeedsService
    {
        private readonly Context _context;
        private const int RefreshDelay = 1000 * 60 * 30; // 30 minutes

        public FeedsService(Context context)
        {
            _context = context;
        }

        public async Task RefreshAllFeeds(CancellationToken stoppingToken)
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

                await Task.Delay(RefreshDelay, stoppingToken);
            }
        }
    }
}
