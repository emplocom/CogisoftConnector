using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using CogisoftConnector.Models.CogisoftRequestModels;
using CogisoftConnector.Models.CogisoftResponseModels;
using CogisoftConnector.Models.WebhookModels.CogisoftRequestModels;
using CogisoftConnector.Models.WebhookModels.CogisoftSOAPEnvelopeModels;
using Newtonsoft.Json;

namespace CogisoftConnector.Logic
{
    public class CogisoftServiceClient : IDisposable
    {
        private string _token;

        public CogisoftServiceClient()
        {
            var loginRequest = new LoginRequestCogisoftModel(
                ConfigurationManager.AppSettings["LinkName"],
                ConfigurationManager.AppSettings["LinkPassword"],
                ConfigurationManager.AppSettings["OperatorLogin"],
                ConfigurationManager.AppSettings["OperatorPassword"]);

            var response =
                PerformRequestReceiveResponse<LoginRequestCogisoftModel, LoginResponseCogisoftModel>(loginRequest);

            _token = response.logon.token;
        }

        public TResponse PerformRequestReceiveResponse<TRequest, TResponse>(TRequest request)
            where TRequest : IRequestCogisoftModel
        {
            //-----------
            if (!bool.Parse(ConfigurationManager.AppSettings["ValidateCogisoftSslCertificate"]))
            {
                //Tylko dla serwera testowego!
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.DefaultConnectionLimit = 9999;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object s, X509Certificate certificate,
                        X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    { return true; };
            }
            //-----------
            request.SetToken(_token);

            var json = JsonConvert.SerializeObject(request);

            var xml = XDocument.Parse(request.GetSOAPEnvelope());

            var node = xml.Root.Descendants("json").First();
            node.Value = json;

            var httpClient = HttpClientProvider.HttpClient;

            var httpResponseMessage = httpClient.PostAsync(
                    request.GetSOAPEndpoint(),
                    new StringContent(xml.ToString(), Encoding.UTF8, "text/xml"))
                .Result;


            var responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;

            try
            {
                var responseXml = XDocument.Parse(responseString);
                var node2 = responseXml.Root.Descendants("json").First();
                return JsonConvert.DeserializeObject<TResponse>(node2.Value);
            }
            catch (Exception e)
            {
                var exception = new Exception(e.Message + ", Received response: " + responseString);
                throw exception;
            }
        }

        public void Dispose()
        {
            var logoutEnvelope = XDocument.Parse(LogoutEnvelope.Envelope);
            var node = logoutEnvelope.Root.Descendants("token").First();
            node.Value = _token;

            var httpClient = HttpClientProvider.HttpClient;

            var httpResponseMessage = httpClient.PostAsync(
                    $"{ConfigurationManager.AppSettings["EndpointAddress"]}/DASH/Login?wsdl",
                    new StringContent(logoutEnvelope.ToString(), Encoding.UTF8, "text/xml"))
                .Result;

            var responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;

            try
            {
                var responseXml = XDocument.Parse(responseString);
                var node2 = responseXml.Root.Descendants("tokenExisted").First();

                if (!node2.Value.Equals("true"))
                {
                    throw new Exception("Token release failed!");
                }
            }
            catch (Exception e)
            {
                var exception = new Exception(e.Message + ", Received response: " + responseString);
                throw exception;
            }
        }
    }
}
