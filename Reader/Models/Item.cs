using System;
using System.Collections.Generic;

namespace Reader.Models
{
    public class Item
    {
        public int Id { get; set; }
        public Feed Feed { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public string Description { get; set; }
        public DateTime? Published { get; set; }
        public string Author { get; set; }
        public IList<string> Categories { get; set; }
        public string Content { get; set; }
        public string FullContent { get; set; }
        public DateTime? Read { get; set; }
    }
}
