using System.Web.Routing;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace CogisoftConnector
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
