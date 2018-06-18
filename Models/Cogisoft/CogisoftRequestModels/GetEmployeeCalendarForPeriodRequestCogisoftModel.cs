using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.Cogisoft
{
    public class P
    {
        public P(DateTime from, DateTime to, string externalEmployeeIdentifier)
        {
            this.from = from.ToString();
            this.to = to.ToString();
            this.card = externalEmployeeIdentifier;
        }

        public string token { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string card { get; set; }
    }

    public class GetEmployeeCalendarForPeriodRequestCogisoftModel : IRequestCogisoftModel
    {
        public P p { get; set; }

        public GetEmployeeCalendarForPeriodRequestCogisoftModel(DateTime from, DateTime to, string externalEmployeeIdentifier)
        {
            p = new P(from, to, externalEmployeeIdentifier);
        }

        public string GetSOAPEnvelope()
        {
            return QueryEnvelope.Envelope;
        }

        public string GetSOAPEndpoint()
        {
            return $@"{ConfigurationManager.AppSettings["EndpointAddress"]}/DASH/Query?wsdl";
        }

        public void SetToken(string token)
        {
            this.p.token = token;
        }
    }
}