using System.Configuration;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
    public class VacationCreatedRequestCogisoftModel : IRequestCogisoftModel
    {
        //{ "r":"json",
        //    "or": { "token":"xxx",
        //        "o":[
            
        //        {"tbl":"KADR:NIEOBECNOSCI",
        //            "t":"i",
        //            "h": { 
        //                "c": [ "FKF_PRAC","FKF_KOD_NIEOB","FLD_DATA_DO","FLD_DNI_KALEND"]
        //            },
        //            "d": [{
        //                "c": ["29","1","2017-02-02","5"]
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

            public Or(string employeeIdentifier, string absenceCode, string dateSince, string duration)
            {
                o[0] = new O(employeeIdentifier, absenceCode, dateSince, duration);
            }
        }

        public class O
        {
            public string tbl = "KADR:NIEOBECNOSCI";
            public string t = "i";

            public H h { get; set; }

            public class H
            {
                public string[] c = new[] {"FKF_PRAC", "FKF_KOD_NIEOB", "FLD_DATA_OD", "FLD_DNI_KALEND"};
            }

            public D[] d = new D[1];

            public class D
            {
                public string[] c = new string[4];

                public D(string employeeIdentifier, string absenceCode, string dateSince, string duration)
                {
                    c = new[] {employeeIdentifier, absenceCode, dateSince, duration};
                }
            }

            public O (string employeeIdentifier, string absenceCode, string dateSince, string duration)
            {
                h = new H();
                d[0] = new D(employeeIdentifier, absenceCode, dateSince, duration);
            }
        }

        public VacationCreatedRequestCogisoftModel(string employeeIdentifier, string absenceCode, string dateSince, string duration)
        {
            or = new Or(employeeIdentifier, absenceCode, dateSince, duration);
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
