using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CogisoftConnector.Models.WebhookModels.CogisoftSOAPEnvelopeModels;

namespace CogisoftConnector.Models.WebhookModels.CogisoftRequestModels
{
    public class GetVacationDataCogisoftModel : IRequestCogisoftModel
    {
        //{ 
        //    "qp":{ 
        //          "token": "xxx", 
        //          "q":{
        //              "tbl":"KADR:PRACOWNICY_DANE", 
        //              "ss": [ "FKF_OSOB", "FLD__ID", "FLD_URLOP_POZ_D", "FLD_URLOP_POZ_H", "FLD_URLOP_ZAL_D", "FLD_URLOP_ZAL_H", "FLD_URLOP_NAZ_D" ],
        //              "p": { 
        //                  "s": 1 
        //                   } 
        //              }
        //         }

        public GetVacationDataCogisoftModel(List<string> employeeIds = null)
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
            public string[] ss = new[] { "FLD__ID", "FKF_OSOB", "FLD_URLOP_POZ_D", "FLD_URLOP_POZ_H", "FLD_URLOP_ZAL_D", "FLD_URLOP_ZAL_H", "FLD_URLOP_NAZ_D" };
            public P p;

            public string fs;

            public Q(List<string> employeeIds)
            {
                p = new P();

                if (employeeIds != null && employeeIds.Any())
                {
                    this.fs = string.Join(" or ", employeeIds.Select(id => $"FKF_OSOB.FKF_OSOB == {id}"));
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
            public int s = 1500;
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
