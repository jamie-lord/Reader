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

        public IEnumerable<ReadItemSummary> Items { get; private set; }

        public string PageTitle
        {
            get
            {
                return $"{Items?.Count()} Read";
            }
        }

        public ReadModel(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        public void OnGet()
        {
            Items = _itemsService.GetRead();
        }

        public async Task<IActionResult> OnPostMarkAsUnread(int id)
        {
            await _itemsService.MarkAsUnread(id);
            return new OkResult();
        }
    }
}
