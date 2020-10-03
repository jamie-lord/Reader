using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reader.Data;
using Reader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CH = CodeHollow.FeedReader;

namespace Reader.Pages
{
    public class FeedsModel : PageModel
    {
        private readonly Context _context;
        public IEnumerable<Feed> Feeds { get; private set; }

        public FeedsModel(Context context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Feeds = _context.Feeds.AsEnumerable();
        }

        [BindProperty]
        public string NewFeedUri { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            var feed = await CH.FeedReader.ReadAsync(NewFeedUri);

            var newFeed = new Feed
            {
                Uri = NewFeedUri,
                Title = feed.Title,
                Added = DateTime.Now,
                LastChecked = DateTime.Now
            };

            _context.Feeds.Add(newFeed);
            await _context.SaveChangesAsync();

            return RedirectToPage("Feeds");
        }
    }
}
