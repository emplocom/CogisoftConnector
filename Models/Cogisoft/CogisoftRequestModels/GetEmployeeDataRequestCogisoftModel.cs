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

    public class Q
    {
        public Q(List<string> selectProperties, List<string> employeeIdsToImport)
        {
            ss = selectProperties;
            p = new P();
            if (employeeIdsToImport.Any())
            {
                fs = $"FKF_OSOB.FKF_OSOB.FKF_WIZYTOWKA in [{string.Join(",", employeeIdsToImport)}]";
            }
        }

        public string tbl { get; set; } = "KADR:PRACOWNICY";
        public List<string> ss { get; set; }
        public string fs { get; set; }
        public P p { get; set; }
    }

    public class Qp
    {
        public Qp(List<string> selectProperties, List<string> employeeIdsToImport)
        {
            q = new Q(selectProperties, employeeIdsToImport);
        }

        public string token { get; set; }
        public Q q { get; set; }
    }

    public class GetEmployeeDataRequestCogisoftModel : IRequestCogisoftModel
    {
        public GetEmployeeDataRequestCogisoftModel(
            CogisoftEmployeeImportConfiguration cogisoftEmployeeImportMappingConfiguration, List<string> employeeIdsToImport)
        {
            qp = new Qp(cogisoftEmployeeImportMappingConfiguration.PropertyMappings.Select(m => m.ExternalPropertyName).ToList(), employeeIdsToImport);
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