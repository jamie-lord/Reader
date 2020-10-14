using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reader.Services;

namespace Reader.Pages
{
    public class UnreadModel : PageModel
    {
        private readonly IItemsService _itemsService;

        public IEnumerable<UnreadItem> Items { get; private set; }

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
            Items = _itemsService.Unread();
        }
    }
}
