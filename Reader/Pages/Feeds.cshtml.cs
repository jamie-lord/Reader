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

        public string PageTitle
        {
            get
            {
                return $"Feeds ({Feeds?.Count()})";
            }
        }

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
        public async Task<IActionResult> OnPostAddNewFeedAsync()
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

            foreach (var item in feed.Items)
            {
                if (_context.Items.Any(x => x.Uri == item.Link))
                {
                    // Item already exists from previous fetch or another feed
                    continue;
                }

                var newItem = new Item
                {
                    Author = item.Author,
                    Categories = item.Categories?.ToList(),
                    Content = item.Content,
                    Description = item.Description,
                    Feed = newFeed,
                    Published = item.PublishingDate,
                    Title = item.Title,
                    Uri = item.Link
                };
                _context.Items.Add(newItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Feeds");
        }

        public async Task<IActionResult> OnPostRefreshFeedAsync(int id)
        {
            var feed = _context.Feeds.Find(id);

            var result = await CH.FeedReader.ReadAsync(feed.Uri);

            feed.LastChecked = DateTime.Now;
            _context.Feeds.Update(feed);
            await _context.SaveChangesAsync();

            foreach (var item in result.Items)
            {
                if (_context.Items.Any(x => x.Uri == item.Link))
                {
                    // Item already exists from previous fetch or another feed
                    continue;
                }

                var newItem = new Item
                {
                    Author = item.Author,
                    Categories = item.Categories?.ToList(),
                    Content = item.Content,
                    Description = item.Description,
                    Feed = feed,
                    Published = item.PublishingDate,
                    Title = item.Title,
                    Uri = item.Link
                };
                _context.Items.Add(newItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Feeds");
        }
    }
}
