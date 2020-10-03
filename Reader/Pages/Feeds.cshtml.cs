using Microsoft.AspNetCore.Mvc.RazorPages;
using Reader.Data;
using Reader.Models;
using System.Collections.Generic;
using System.Linq;

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
    }
}
