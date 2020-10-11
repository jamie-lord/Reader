using Reader.Data;
using System;
using System.Threading.Tasks;

namespace Reader.Services
{
    public interface IItemsService
    {
        Task GetFullContent(int id);
    }

    public class ItemsService : IItemsService
    {
        private readonly Context _context;

        public ItemsService(Context context)
        {
            _context = context;
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
    }
}
