using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HebrewSearch.Models;
using Nancy;
using Nest;

namespace HebrewSearch
{
    public class HomeModule : NancyModule
    {
        public HomeModule(IElasticClient elasticClient)
        {
            Get["/", true] = async (p, ct) =>
                       {
                           var vm = new HomeViewModel();

                           string q = p.q;

                           if (q != null) { 
                               var results = elasticClient.SearchAsync<ContentPage>(
                                   search => search.AllIndices().Query(
                                        query => query.Bool(b => b.Must(bc => bc.Match(_ => _.Analyzer("hebrew_query_light").OnField("title").QueryString(q))))
                                       )
                                   );
                               //await Task.WhenAll(results);
                               vm.Results = results.Result;
                           }


                           return View["Search", vm];
                       };
        }
    }

    public class HomeViewModel
    {
        public string[] Indexes { get; set; }
        public IEnumerable<ContentPage> Results { get; set; }
    }
}