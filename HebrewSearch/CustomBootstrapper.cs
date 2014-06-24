using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using NElasticsearch;

namespace HebrewSearch
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(new ElasticsearchRestClient("http://localhost:9200"));
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("static", @"static"));
            base.ConfigureConventions(conventions);
        }
    }
}