using System;
using System.Collections.Generic;
using System.Linq;
using HebrewSearch.Models;
using Nancy;
using Nancy.ViewEngines.Razor;
using Nest;

namespace HebrewSearch
{
    public class HomeModule : NancyModule
    {
        public HomeModule(IElasticClient elasticClient)
        {
            const int pageSize = 10;

            Get["/search", true] = async (p, ct) =>
                       {
                           var vm = new HomeViewModel();

                           string q = Request.Query.q;
                           int page = 1;

                           if (q != null) { 
                               var results = await elasticClient.SearchAsync<ContentPage>(
                                   search => search.Type("contentpage").Index("hebrew-wikipedia-20140208").Query(
                                       mainQuery => mainQuery.Filtered(filtered => filtered.Query(
                                        query => query.Bool(b => b.Should(
                                            bc => bc.MatchPhrase(_ => _.OnField("title").QueryString(q)),
                                            bc => bc.MatchPhrase(_ => _.OnField("text").QueryString(q))
                                            ) // .Analyzer("hebrew_query")
                                          )
                                         ).Filter(
                                            filter => filter.And(_ => _.Missing("redirect"))
                                         )
                                       ))
                                       .Highlight(h => h.PreTags("<b>").PostTags("</b>").OnFields(_ => _.OnField("title").NumberOfFragments(0), _ => _.OnField("text")))
                                       .Fields("title", "categories", "author")
                                       .FacetTerm("categories", f => f.OnField("categories").FacetFilter(filter => filter.And(_ => _.Missing("redirect"))).Size(50))
                                       .Size(pageSize).Skip(pageSize * (page - 1))
                                   );

                               if (results.ConnectionStatus.Error != null)
                               {
                                   vm.ErrorString = results.ConnectionStatus.Error.ToString();
                               }

                               vm.TotalResults = results.Total;
                               vm.Results = new List<SearchResult>();
                               foreach (var hit in results.DocumentsWithMetaData)
                               {
                                   var r = new SearchResult();
                                   if (hit.Highlights.ContainsKey("title"))
                                   {
                                       r.Title = new NonEncodedHtmlString(String.Join("... ", hit.Highlights["title"].Highlights));
                                   }
                                   else
                                   {
                                       r.Title = new NonEncodedHtmlString(hit.Fields.Title);
                                   }

                                   if (hit.Highlights.ContainsKey("text"))
                                   {
                                       r.Snippets = new NonEncodedHtmlString(String.Join("... ", hit.Highlights["text"].Highlights));
                                   }

                                   r.Categories = hit.Fields.Categories;

                                   vm.Results.Add(r);
                               }

                               vm.CategoryFacets = results.Facet<TermFacet>("categories").Items.Select(x => new TermAndCount
                                                                                                            {
                                                                                                                Term = x.Term,
                                                                                                                Count = x.Count,
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
        public int TotalResults { get; set; }
        public string[] Indexes { get; set; }
        public List<SearchResult> Results { get; set; }
        public string ErrorString { get; set; }
        public IEnumerable<TermAndCount> CategoryFacets { get; set; }
    }
}