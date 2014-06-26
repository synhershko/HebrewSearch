using System;
using System.Collections.Generic;
using System.Linq;
using HebrewSearch.Models;
using Nancy;
using Nancy.ViewEngines.Razor;
using NElasticsearch;
using NElasticsearch.Commands;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace HebrewSearch
{
    public class HomeModule : NancyModule
    {
        public HomeModule(ElasticsearchRestClient client)
        {
            const int pageSize = 10;

            Get["/search", true] = async (p, ct) =>
                       {
                           var vm = new HomeViewModel();
                           ViewBag.Query = string.Empty;

                           string q = Request.Query.q;
                           int page = 1;

                           if (q != null)
                           {
                               ViewBag.Query = q;
                               var filter = new {missing = new {field = "redirect"}};

                               var analyzer = "hebrew";
                               SearchType searchType;                               
                               if (Enum.TryParse(Request.Query.type, true, out searchType))
                               {
                                   switch (searchType)
                                   {
                                       case SearchType.Morphological:
                                           analyzer = "hebrew_query";
                                           break;
                                       case SearchType.MorphologicalOptimized:
                                           analyzer = "hebrew_query_light";
                                           break;
                                       case SearchType.Exact:
                                       case SearchType.Naive:
                                           analyzer = "hebrew_exact";
                                           break;
                                       default:
                                           analyzer = "hebrew_query";
                                           break;
                                   }
                               }

                               var query = new {multi_match = new {query = q, fields = new[] {"title", "text"}, analyzer = analyzer}};

                               var results = client.Search<ContentPage>(new
                                                          {
                                                              query = new { filtered = new
                                                              {
                                                                  query = query,
                                                                  filter = filter,
                                                              }},

                                                              highlight = new
                                                                          {
                                                                              fields = new
                                                                                       {
                                                                                           title = new {number_of_fragments = 0 },
                                                                                           text = new {},
                                                                                       },
                                                                              pre_tags = new[] {"<b>"}, post_tags = new[]{"</b>"},
                                                                              
                                                                          },

                                                              aggregations = new {categories = new
                                                                                  {
                                                                                      terms = new
                                                                                              {
                                                                                                  field = "link",
                                                                                                  size = 50,
                                                                                              }
                                                                                  }},

                                                              _source = new[] { "title", "categories", "author" },

                                                              from = pageSize * (page - 1),
                                                              size = pageSize,
                                                          }
                                                          , "hebrew-wikipedia-20140610", "contentpage");

                               // TODO
//                               if (results.status != HttpStatusCode.OK)
//                               {
//                                   vm.ErrorString = 
//                               }
                                                            
                               vm.TotalResults = results.hits.total;
                               vm.Results = new List<SearchResult>();
                               foreach (var hit in results.hits.hits)
                               {
                                   var r = new SearchResult();
                                   if (hit.highlight.ContainsKey("title"))
                                   {
                                       r.Title = new NonEncodedHtmlString(String.Join("... ", hit.highlight["title"]));
                                   }
                                   else
                                   {
                                       r.Title = new NonEncodedHtmlString(hit._source.Title);
                                   }

                                   if (hit.highlight.ContainsKey("text"))
                                   {
                                       r.Snippets = new NonEncodedHtmlString(String.Join("... ", hit.highlight["text"]));
                                   }

                                   r.Categories = hit._source.Categories;
                                   r.Id = hit._id;

                                   vm.Results.Add(r);
                               }

                               vm.CategoryFacets = results.aggregations["categories"].Buckets.Select(x => new TermAndCount
                                                                                                            {
                                                                                                                Term = x.Key,
                                                                                                                Count = x.DocCount,
                                                                                                            });
                           }


                           return View["Search", vm];
                       };

            Get["/"] = p =>
                       {
                           var vm = new HomeViewModel();

                           return View["Main", vm];
                       };

            Get["/about"] = p => View["About"];

            Get["/contact"] = p => View["Contact"];
        }
    }

    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Indexes = new string[]{};
            Results = new List<SearchResult>();
            CategoryFacets = new List<TermAndCount>();
        }

        public string Query { get; set; }
        public long TotalResults { get; set; }
        public string[] Indexes { get; set; }
        public List<SearchResult> Results { get; set; }
        public string ErrorString { get; set; }
        public IEnumerable<TermAndCount> CategoryFacets { get; set; }
    }
}