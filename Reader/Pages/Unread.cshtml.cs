using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Reader.Services;

namespace Reader.Pages
{
    public class UnreadModel : PageModel
    {
        private readonly IItemsService _itemsService;

        public IEnumerable<ItemSummary> Items { get; private set; }

        public string PageTitle
        {
            get
            {
                return $"{Items?.Count()} Unread";
            }
        }

        public UnreadModel(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        public void OnGet()
        {
            Items = _itemsService.GetUnread();
        }

        public async Task<IActionResult> OnPostMarkAsRead(int id)
        {
            await _itemsService.MarkAsRead(id);
            return RedirectToPage("Unread");
        }

        public async Task<IActionResult> OnPostMarkAllAsRead(string ids)
        {
            var idsList = JsonConvert.DeserializeObject<IEnumerable<int>>(ids);
            await _itemsService.MarkAllAsRead(idsList);
            return RedirectToPage("Unread");
        }
    }
}
