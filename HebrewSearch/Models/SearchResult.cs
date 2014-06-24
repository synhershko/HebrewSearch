using System.Collections.Generic;
using Nancy.ViewEngines.Razor;

namespace HebrewSearch.Models
{
    public class SearchResult
    {
        public string Id { get; set; }
        public IHtmlString Title { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public IHtmlString Snippets { get; set; }
    }
}