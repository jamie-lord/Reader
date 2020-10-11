using Microsoft.AspNetCore.Mvc.RazorPages;
using Reader.Models;
using Reader.Services;

namespace Reader.Pages
{
    public class SingleItemModel : PageModel
    {
        private readonly IItemsService _itemsService;

        public Item Item { get; private set; }

        public SingleItemModel(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        public void OnGet(int id)
        {
            Item = _itemsService.GetItem(id);
        }
    }
}
