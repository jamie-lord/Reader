using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reader.Data;
using Reader.Models;

namespace Reader.Pages
{
    public class UnreadModel : PageModel
    {
        private readonly Context _context;
        public IEnumerable<Item> Items { get; private set; }

        public string PageTitle
        {
            get
            {
                return $"{Items?.Count()} Unread";
            }
        }

        public UnreadModel(Context context)
        {
            _context = context;
        }

        public void OnGet()
        {
            Items = _context.Items.ToList();
        }
    }
}
