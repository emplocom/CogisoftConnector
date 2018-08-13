using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
    public class GetVacationRequestByIdCogisoftModel : IRequestCogisoftModel
    {
        public class P
        {
            public int s { get; set; } = 1;
        }

        public class Q
        {
            public string tbl { get; set; } = "KADR:NIEOBECNOSCI";
            public List<string> ss { get; set; } = new List<string> { "FLD__ID" };
            public string fs { get; set; }
            public P p { get; set; }

            public Q(string vacationIdentifier)
            {
                fs = $"FLD__ID == {vacationIdentifier}";
                p = new P();
            }
        }

        public class Qp
        {
            public string token { get; set; }
            public Q q { get; set; }

            public Qp(string vacationIdentifier)
            {
                q = new Q(vacationIdentifier);
            }
        }

        public Qp qp { get; set; }

        public GetVacationRequestByIdCogisoftModel(string vacationIdentifier)
        {
            qp = new Qp(vacationIdentifier);
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
            qp.token = token;
        }
    }
}
