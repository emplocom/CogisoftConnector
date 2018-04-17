using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using CogisoftConnector.Logic;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;
using Newtonsoft.Json;

namespace CogisoftConnector.Models.Cogisoft.CogisoftRequestModels
{
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

    public class Q
    {
        public Q(List<string> selectProperties)
        {
            ss = selectProperties;
        }

        public string tbl { get; set; } = "KADR:PRACOWNICY";
        public List<string> ss { get; set; }
        public P p { get; set; }
    }

    public class Qp
    {
        public Qp(List<string> selectProperties)
        {
            q = new Q(selectProperties);
        }

        public string token { get; set; }
        public Q q { get; set; }
    }

    public class GetEmployeeDataRequestCogisoftModel : IRequestCogisoftModel
    {
        public GetEmployeeDataRequestCogisoftModel(
            CogisoftEmployeeImportConfiguration cogisoftEmployeeImportMappingConfiguration)
        {
            qp = new Qp(cogisoftEmployeeImportMappingConfiguration.PropertyMappings.Select(m => m.FileHeaderName).ToList());
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

        public void IncrementQueryIndex()
        {
            this.qp.q.p.i++;
        }
    }
}