using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Reader.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reader.Pages
{
    public class UnreadModel : PageModel
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

        public UnreadModel(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        public void OnGet()
        {
            Items = _itemsService.GetUnread().GroupBy(i => i.DatePublished);
        }

        public async Task<IActionResult> OnPostMarkAsRead(int id)
        {
            await _itemsService.MarkAsRead(id);
            return new OkResult();
        }

        public async Task<IActionResult> OnPostMarkAllAsRead(string ids)
        {
            var idsList = JsonConvert.DeserializeObject<IEnumerable<int>>(ids);
            await _itemsService.MarkAllAsRead(idsList);
            return RedirectToPage("Unread");
        }
    }
}
