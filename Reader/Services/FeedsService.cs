using CodeHollow.FeedReader;
using Reader.Data;
using Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Reader.Services
{
    public interface IFeedsService
    {
        Task RefreshAllFeeds(CancellationToken stoppingToken);
        IEnumerable<Models.Feed> GetFeeds();
        Task AddFeed(Models.Feed feed);
        Models.Feed Get(int id);
        Task UpdateFeed(Models.Feed feed);
        Task DeleteFeed(int id);
        Task RefreshFeed(int id);
        Task AddFeed(string uri);
    }

    public class FeedsService : IFeedsService
    {
        private readonly Context _context;
        private readonly IItemsService _itemsService;
        private const int RefreshDelay = 1000 * 60 * 30; // 30 minutes

        public FeedsService(Context context, IItemsService itemsService)
        {
            _context = context;
            _itemsService = itemsService;
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

        public IEnumerable<Models.Feed> GetFeeds()
        {
            return _context.Feeds;
        }

        public async Task AddFeed(Models.Feed feed)
        {
            _context.Feeds.Add(feed);
            await _context.SaveChangesAsync();
        }

        public Models.Feed Get(int id)
        {
            return _context.Feeds.Find(id);
        }

        public async Task UpdateFeed(Models.Feed feed)
        {
            _context.Feeds.Update(feed);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFeed(int id)
        {
            var feed = _context.Feeds.Find(id);
            var items = _context.Items.Where(x => x.Feed == feed);
            foreach (var item in items)
            {
                _context.Items.Remove(item);
            }
            _context.Feeds.Remove(feed);
            await _context.SaveChangesAsync();
        }

        public async Task RefreshFeed(int id)
        {
            var feed = Get(id);
            var result = await FeedReader.ReadAsync(feed.Uri);

            feed.LastChecked = DateTime.Now;
            await UpdateFeed(feed);

            foreach (var item in result.Items)
            {
                if (_itemsService.ItemExists(item.Link))
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

                await _itemsService.AddItem(newItem);
                await _itemsService.GetFullContent(newItem.Id);
            }
        }

        public async Task AddFeed(string uri)
        {
            var result = await FeedReader.ReadAsync(uri);
            var newFeed = new Models.Feed
            {
                Uri = uri,
                Title = result.Title,
                Added = DateTime.Now,
                LastChecked = DateTime.Now
            };
            await AddFeed(newFeed);
            foreach (var item in result.Items)
            {
                if (_itemsService.ItemExists(item.Link))
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
                    Feed = newFeed,
                    Published = item.PublishingDate,
                    Title = item.Title,
                    Uri = item.Link
                };
                await _itemsService.AddItem(newItem);
                await _itemsService.GetFullContent(newItem.Id);
            }
        }
    }
}
