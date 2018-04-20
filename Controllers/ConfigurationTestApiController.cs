using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CogisoftConnector.Logic;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Controllers
{
    public class ConfigurationTestApiController : ApiController
    {
        private readonly ConfigurationTestLogic _configurationTestLogic;

        public ConfigurationTestApiController()
        {
            ILogger logger = LoggerFactory.CreateLogger(null);
            _configurationTestLogic = new ConfigurationTestLogic(logger);
        }

        /// <summary>
        /// Enables testing of Connector's configuration by sending test requests to the Cogisoft and emplo APIs.
        /// </summary>
        [HttpGet]
        public async Task<HttpResponseMessage> TestConnection()
        {
            var emploResult = await _configurationTestLogic.TestEmploConnection();
            var cogisoftResult = _configurationTestLogic.TestCogisoftConnection();

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"Emplo API connection test: {emploResult}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}Cogisoft API connection test: {cogisoftResult}") };
        }
    }
}