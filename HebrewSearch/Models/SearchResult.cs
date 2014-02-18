using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.ViewEngines.Razor;

namespace HebrewSearch.Models
{
    public class SearchResult
    {
        public string Id { get; set; }
        public IHtmlString Title { get; set; }
        public string[] Categories { get; set; }
        public IHtmlString Snippets { get; set; }
    }
}