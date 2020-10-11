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
        IEnumerable<Item> Unread();
        Task AddItem(Item item);
        bool ItemExists(string uri);
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

        public IEnumerable<Item> Unread()
        {
            return _context.Items;
        }
    }
}
