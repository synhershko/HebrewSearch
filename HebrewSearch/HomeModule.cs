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
            Get["/"] = (p) =>
                       {
                           var vm = new HomeViewModel();

                           return View["Search", vm];
                       };
            
            Get["/search/{q*}", true] = async (p, ct) =>
                       {
                           var vm = new HomeViewModel();

                           string q = p.q;

                           if (q != null) { 
                               var results = await elasticClient.SearchAsync<ContentPage>(
                                   search => search.Type("contentpage").AllIndices().Query(
                                       mainQuery => mainQuery.Filtered(filtered => filtered.Query(
                                        query => query.Bool(b => b.Should(
                                            bc => bc.Match(_ => _.OnField("title").Boost(5.0).QueryString(q)),
                                            bc => bc.Match(_ => _.OnField("text").QueryString(q))
                                            )
                                          )
                                         ).Filter(
                                            filter => filter.And(_ => _.Missing("redirect"))
                                         )
                                       ))
                                       .Highlight(h => h.PreTags("<b>").PostTags("</b>").OnFields(_ => _.OnField("title").NumberOfFragments(0), _ => _.OnField("text")))
                                       .Fields("title", "categories", "author")
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
                                   vm.Results.Add(r);
                               }
                           }


                           return View["Search", vm];
                       };
        }
    }

    public class HomeViewModel
    {
        public int TotalResults { get; set; }
        public string[] Indexes { get; set; }
        public List<SearchResult> Results { get; set; }
        public string ErrorString { get; set; }
    }
}