using Owin;
using System.Web.Http;
using Swashbuckle.Application;
using System.Linq;

namespace WebProxy.Tests
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();            

            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "A title for your API");
                c.UseFullTypeNameInSchemaIds();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.FirstOrDefault());
            })
                .EnableSwaggerUi(); //for debugging purposes

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            appBuilder.UseWebApi(config);
        }
    }
}
