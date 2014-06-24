using System.Collections.Generic;

namespace HebrewSearch.Models
{
    public class ContentPage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public List<string> Categories { get; set; }
    }
}