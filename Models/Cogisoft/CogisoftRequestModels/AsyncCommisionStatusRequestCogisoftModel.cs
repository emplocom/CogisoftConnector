using System.Configuration;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
    public class AsyncCommisionStatusRequestCogisoftModel : IRequestCogisoftModel
    {
        //{"p": { "c":[1883056], "token":"xxx" }}

        public P p;

        public class P
        {
            public string[] c = new string[1];
            public string token { get; set; }
        }

        public AsyncCommisionStatusRequestCogisoftModel(string commisionIdentifier)
        {
            this.p = new P()
            {
                c = new []{ commisionIdentifier }
            };
        }

        public string GetSOAPEnvelope()
        {
            return StatusOfEnvelope.Envelope;
        }

        public string GetSOAPEndpoint()
        {
            return $@"{ConfigurationManager.AppSettings["EndpointAddress"]}/DASH/Request?wsdl";
        }

        public void SetToken(string token)
        {
            p.token = token;
        }
    }
}
