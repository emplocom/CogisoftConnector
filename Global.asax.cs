using System.Configuration;
using System.Web.Routing;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace CogisoftConnector
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            bool mockMode;
            if(bool.TryParse(ConfigurationManager.AppSettings["MockMode"], out mockMode) && mockMode)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);
            }

            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
