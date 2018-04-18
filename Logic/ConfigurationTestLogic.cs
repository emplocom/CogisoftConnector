using System;
using System.Configuration;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.Client;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Logic
{
    public class ConfigurationTestLogic
    {
        private readonly ILogger _logger;

        public ConfigurationTestLogic(ILogger logger)
        {
            _logger = logger;
        }

        public string TestEmploConnection()
        {
            try
            {
                ApiConfiguration _apiConfiguration = new ApiConfiguration()
                {
                    EmploUrl = ConfigurationManager.AppSettings["EmploUrl"],
                    ApiPath = ConfigurationManager.AppSettings["ApiPath"] ?? "apiv2",
                    Login = ConfigurationManager.AppSettings["Login"],
                    Password = ConfigurationManager.AppSettings["Password"]
                };

                var apiClient = new ApiClient(_logger, _apiConfiguration);

                var response = apiClient
                    .SendGet<Object>(_apiConfiguration.CheckUserHasAccessUrl);

                return "Success!";
            }
            catch (Exception e)
            {
                return ExceptionLoggingUtils.ExceptionAsString(e);
            }
        }

        public string TestCogisoftConnection()
        {
            try
            {
                using (var client = new CogisoftServiceClient(_logger))
                {
                    TestConnectionRequestCogisoftModel cogisoftRequest = new TestConnectionRequestCogisoftModel();

                    var response =
                        client.PerformRequestReceiveResponse<TestConnectionRequestCogisoftModel,
                            TestConnectionResponseCogisoftModel>(cogisoftRequest);

                    return "Success!";
                }
            }
            catch (Exception e)
            {
                return ExceptionLoggingUtils.ExceptionAsString(e);
            }
        }
    }
}