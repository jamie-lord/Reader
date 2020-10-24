using System;

namespace Reader.Models
{
    public class Item
    {
        public int Id { get; set; }
        public Feed Feed { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Read { get; set; }
    }
}
