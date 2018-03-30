using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using CogisoftConnector.Models.WebhookModels.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.WebhookModels.CogisoftRequestModels
{
    public class VacationCancelledRequestCogisoftModel : IRequestCogisoftModel
    {
        //{ "r":"json",
        //    "or": { "token":"xxx",
        //        "o":[
            
        //        {"tbl":"KADR:NIEOBECNOSCI",
        //            "t":"d",
        //            "h": { 
        //                "c": [ "FLD__ID"]
        //            },
        //            "d": [{
        //                "c": ["123"]
        //            }
        //            ]
        //        }
        //        ] 
        //    }
        //}

        public string r = "json";
        public Or or;

        public class Or
        {
            public string token { get; set; }
            public O[] o = new O[1];

            public Or(string vacationIdentifier)
            {
                o[0] = new O(vacationIdentifier);
            }
        }

        public class O
        {
            public string tbl = "KADR:NIEOBECNOSCI";
            public string t = "d";

            public H h { get; set; }

            public class H
            {
                public string[] c = new[] { "FLD__ID" };
            }

            public D[] d = new D[1];

            public class D
            {
                public string[] c = new string[1];

                public D(string vacationIdentifier)
                {
                    c = new[] { vacationIdentifier};
                }
            }

            public O (string vacationIdentifier)
            {
                h = new H();
                d[0] = new D(vacationIdentifier);
            }
        }

        public VacationCancelledRequestCogisoftModel(string vacationIdentifier)
        {
            or = new Or(vacationIdentifier);
        }

        public string GetSOAPEnvelope()
        {
            return RequestEnvelope.Envelope;
        }

        public string GetSOAPEndpoint()
        {
            return $@"{ConfigurationManager.AppSettings["EndpointAddress"]}/DASH/Request?wsdl";
        }

        public void SetToken(string token)
        {
            or.token = token;
        }
    }
}
