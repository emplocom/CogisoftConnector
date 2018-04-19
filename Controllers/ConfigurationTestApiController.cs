using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using CogisoftConnector.Logic;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Controllers
{
    public class ConfigurationTestApiController : ApiController
    {
        private ConfigurationTestLogic _configurationTestLogic;

        public ConfigurationTestApiController()
        {
            ILogger logger = LoggerFactory.CreateLogger(null);
            _configurationTestLogic = new ConfigurationTestLogic(logger);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> TestConnection()
        {
            var emploResult = await _configurationTestLogic.TestEmploConnection();
            var cogisoftResult = _configurationTestLogic.TestCogisoftConnection();

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"Emplo API connection test: {emploResult}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}Cogisoft API connection test: {cogisoftResult}") };

            //var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"<html><body><b>Emplo API connection test:</b> {_configurationTestLogic.TestEmploConnection()}</br></br></br><b>Cogisoft API connection test:</b> {_configurationTestLogic.TestCogisoftConnection()}</body></html>") };
            //response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            //return response;
        }
    }
}