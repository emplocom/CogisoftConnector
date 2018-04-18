using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
    public class TestConnectionRequestCogisoftModel : IRequestCogisoftModel
    {
        public TestConnectionRequestCogisoftModel()
        {
            qp = new Qp();
        }

        public class P
        {
            public int s { get; set; } = 1;
        }

        public class Q
        {
            public Q()
            {
                p = new P();
            }

            public string tbl { get; set; } = "KADR:PRACOWNICY";
            public List<string> ss { get; set; } = new List<string>() { "FLD__ID" };
            public P p { get; set; }
        }

        public class Qp
        {
            public Qp()
            {
                q = new Q();
            }

            public string token { get; set; }
            public Q q { get; set; }
        }

        public Qp qp { get; set; }

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