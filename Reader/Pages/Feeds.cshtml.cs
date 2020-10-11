using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reader.Models;
using Reader.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reader.Pages
{
    public class FeedsModel : PageModel
    {
        private readonly IFeedsService _feedsService;

        public IEnumerable<Feed> Feeds { get; private set; }

        public string PageTitle
        {
            get
            {
                return $"{Feeds?.Count()} Feeds";
            }
        }

        public FeedsModel(IFeedsService feedsService)
        {
            _feedsService = feedsService;
        }

        public void OnGet()
        {
            Feeds = _feedsService.GetFeeds();
        }

        [BindProperty]
        public string NewFeedUri { get; set; }
        public async Task<IActionResult> OnPostAddNewFeedAsync()
        {
            await _feedsService.AddFeed(NewFeedUri);
            return RedirectToPage("Feeds");
        }

        public async Task<IActionResult> OnPostRefreshFeedAsync(int id)
        {
            await _feedsService.RefreshFeed(id);
            return RedirectToPage("Feeds");
        }

        public async Task<IActionResult> OnPostDeleteFeed(int id)
        {
            await _feedsService.DeleteFeed(id);
            return RedirectToPage("Feeds");
        }
    }
}
