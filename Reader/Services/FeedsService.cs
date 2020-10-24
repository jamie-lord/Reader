using CodeHollow.FeedReader;
using Humanizer;
using Reader.Data;
using Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reader.Services
{
    public interface IFeedsService
    {
        Task RefreshAllFeeds();
        IEnumerable<FeedSummary> GetFeeds();
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

        public FeedsService(Context context, IItemsService itemsService)
        {
            _context = context;
            _itemsService = itemsService;
        }

        public async Task RefreshAllFeeds()
        {
            var feedIds = _context.Feeds.Select(f => f.Id);
            foreach (var id in feedIds)
            {
                try
                {
                    await RefreshFeed(id);
                }
                catch (Exception)
                {
                }
            }
        }

        public IEnumerable<FeedSummary> GetFeeds()
        {
            var feeds = _context.Feeds.Select(f => new FeedSummary
            {
                Id = f.Id,
                LastChecked = f.LastChecked.Humanize(false, null, null),
                Title = f.Title,
                ItemCount = f.Items.Count
            });

            return feeds;
        }

        public async Task AddFeed(Models.Feed feed)
        {
            _context.Feeds.Add(feed);
            await _context.SaveChangesAsync();
        }

        public Models.Feed Get(int id)
        {
            return _context.Feeds.SingleOrDefault(f => f.Id == id);
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
            await GetNewItems(feed, result.Items);
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
            await GetNewItems(newFeed, result.Items);
        }

        private async Task GetNewItems(Models.Feed feed, IEnumerable<FeedItem> items)
        {
            var newItems = new List<Item>();
            foreach (var item in items)
            {
                if (_itemsService.ItemExists(item.Link))
                {
                    // Item already exists from previous fetch or another feed
                    continue;
                }

                newItems.Add(new Item
                {
                    Feed = feed,
                    Published = item.PublishingDate,
                    Title = item.Title,
                    Uri = item.Link
                });
            }
            await _itemsService.AddItems(newItems);
        }
    }

    public class FeedSummary
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string LastChecked { get; set; }
        public int ItemCount { get; set; }
    }
}
