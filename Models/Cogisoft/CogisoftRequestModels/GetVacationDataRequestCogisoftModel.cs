using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
    public class GetVacationDataRequestCogisoftModel : IRequestCogisoftModel
    {
        //{ 
        //    "qp":{ 
        //          "token": "xxx", 
        //          "q":{
        //              "tbl":"KADR:PRACOWNICY_DANE", 
        //              "ss": [ "FLD__ID", "FLD_URLOP_POZ_D", "FLD_URLOP_POZ_H", "FLD_URLOP_ZAL_D", "FLD_URLOP_ZAL_H", "FLD_URLOP_NAZ_D" ],
        //              "p": { 
        //                  "s": 1 
        //                   } 
        //              }
        //         }

        public GetVacationDataRequestCogisoftModel(List<string> employeeIds = null)
        {
            qp = new QP(employeeIds);
        }

        public QP qp;

        public class QP
        {
            public string token { get; set; }
            public Q q;

            public QP(List<string> employeeIds)
            {
                q = new Q(employeeIds);
            }
        }

        public class Q
        {
            public string tbl = "KADR:PRACOWNICY_DANE";
            public string[] ss = new[] { "FLD__ID", "FLD_URLOP_POZ_D", "FLD_URLOP_POZ_H", "FLD_URLOP_ZAL_D", "FLD_URLOP_ZAL_H", "FLD_URLOP_NAZ_D" };
            public P p;

            public string fs;

            public Q(List<string> employeeIds)
            {
                p = new P();

                if (employeeIds != null && employeeIds.Any())
                {
                    this.fs =
                        $"FLD__ID in [{string.Join(",", employeeIds)}]";
                }
                else
                {
                    this.fs = string.Empty;
                }
            }
        }

        public class P
        {
            /// <summary>
            /// Page size
            /// </summary>
            public int s = int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]);
            /// <summary>
            /// Page index
            /// </summary>
            public int i = 0;
            /// <summary>
            /// Offset
            /// </summary>
            public int off = 0;
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

        public void IncrementQueryIndex()
        {
            this.qp.q.p.i++;
        }
    }
}
