using System;
using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;
using Nest;

namespace HebrewSearch
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var settings = new ConnectionSettings(new Uri(""))
                //.SetDefaultIndex("mydefaultindex")
            ;
            var elasticClient = new ElasticClient(settings);
            container.Register<IElasticClient>(elasticClient);
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            conventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("static", @"static"));
            base.ConfigureConventions(conventions);
        }
    }
}