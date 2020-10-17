using Reader.Data;
using Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reader.Services
{
    public interface IItemsService
    {
        Task GetFullContent(int id);
        Item GetItem(int id);
        IEnumerable<ItemSummary> GetUnread();
        IEnumerable<ItemSummary> GetRead();
        Task AddItem(Item item);
        bool ItemExists(string uri);
        Task MarkAsRead(int id);
        Task MarkAllAsRead(IEnumerable<int> ids);
        Task MarkAsUnread(int id);
    }

    public class ItemsService : IItemsService
    {
        private readonly Context _context;

        public ItemsService(Context context)
        {
            _context = context;
        }

        public async Task AddItem(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task GetFullContent(int id)
        {
            var item = _context.Items.Find(id);

            SmartReader.Reader sr = new SmartReader.Reader(item.Uri)
            {
                Debug = true,
                LoggerDelegate = Console.WriteLine,
            };

            SmartReader.Article article = await sr.GetArticleAsync();

            if (article.IsReadable)
            {
                await article.ConvertImagesToDataUriAsync();
                item.FullContent = article.Content;
                await _context.SaveChangesAsync();
            }
        }

        public Item GetItem(int id)
        {
            return _context.Items.Find(id);
        }

        public bool ItemExists(string uri)
        {
            return _context.Items.Any(x => x.Uri == uri);
        }

        public async Task MarkAllAsRead(IEnumerable<int> ids)
        {
            var now = DateTime.Now;
            foreach (var id in ids)
            {
                var item = _context.Items.Find(id);
                item.Read = now;
                item.FullContent = null;
                _context.Items.Update(item);
            }
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsRead(int id)
        {
            var item = _context.Items.Find(id);
            item.Read = DateTime.Now;
            item.FullContent = null;
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsUnread(int id)
        {
            var item = _context.Items.Find(id);
            item.Read = null;
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<ItemSummary> GetUnread()
        {
            return _context.Items.Where(i => i.Read == null).OrderByDescending(i => i.Published).Select(i =>
            new ItemSummary
            {
                Id = i.Id,
                Title = i.Title,
                Published = i.Published == null ? null : i.Published.ToString(),
                FeedTitle = i.Feed.Title
            });
        }

        public IEnumerable<ItemSummary> GetRead()
        {
            return _context.Items.Where(i => i.Read != null).OrderByDescending(i => i.Published).Select(i => new ItemSummary
            {
                Id = i.Id,
                Title = i.Title,
                Published = i.Published == null ? null : i.Published.ToString(),
                FeedTitle = i.Feed.Title
            });
        }
    }

    public class ItemSummary
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Published { get; set; }
        public string FeedTitle { get; set; }
    }
}
