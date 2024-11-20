using System.Web.Http;
using Owin;

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        // Configure Web API for self-host
        HttpConfiguration config = new HttpConfiguration();

        // Remove the XML formatter and ensure JSON is the default response format
        config.Formatters.Remove(config.Formatters.XmlFormatter);
        config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

        // Define default routes
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );

        // Enable attribute-based routing (optional)
        config.MapHttpAttributeRoutes();

        app.UseWebApi(config);
    }
}
