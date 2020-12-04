using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reader.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reader.Pages
{
    public class ReadModel : PageModel
    {
        private readonly IItemsService _itemsService;

        public IEnumerable<IGrouping<string, ItemSummary>> Items { get; private set; }

        public int ItemsCount
        {
            get
            {
                return Items == null ? 0 : Items.Select(x => x.Count()).Sum();
            }
        }

        public ReadModel(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        public void OnGet()
        {
            Items = _itemsService.GetRead().GroupBy(i => i.DatePublished); ;
        }

        public async Task<IActionResult> OnPostMarkAsUnread(int id)
        {
            await _itemsService.MarkAsUnread(id);
            return new OkResult();
        }
    }
}
