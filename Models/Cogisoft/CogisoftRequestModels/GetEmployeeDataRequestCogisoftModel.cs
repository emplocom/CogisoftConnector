using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CogisoftConnector.Models.Cogisoft.CogisoftSOAPEnvelopeModels;

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
        public Q(List<string> employeeIdsToImport)
        {
            p = new P();

            fs =
                $"FLD_DATA_ZWOLN == null{(employeeIdsToImport.Any() ? $" and FLD__ID in [{string.Join(",", employeeIdsToImport)}]" : string.Empty)}";

            ss = new List<string>()
            {
                "FLD__ID",
                "FKF_OSOB.FKF_OSOB.FKF_WIZYTOWKA",
                "FKF_OSOB.FKF_OSOB.FLD_IMIE1",
                "FKF_OSOB.FKF_OSOB.FLD_NAZWISKO",
                "FKF_OSOB.FKF_OSOB.FKF_WIZYTOWKA.FKX_KNTK_DOM_KONTAKT_BY_RODZAJ_ADRES_20E_2DMAIL.FLD_KONTAKT",
                "FKF_STANOWISKO.FLD_NAZWA",
                "FKF_JEDN_ORG.FLD_SYMBOL",
                "FKF_JEDN_ORG.FLD_NAZWA",
                "FKF_JEDN_ORG.FKF_KIEROWNIK.FKF_WIZYTOWKA.FKX_KNTK_DOM_KONTAKT_BY_RODZAJ_ADRES_20E_2DMAIL.FLD_KONTAKT"
            };
        }

        public string tbl { get; set; } = "KADR:PRACOWNICY";
        public List<string> ss { get; set; }
        public string fs { get; set; }
        public P p { get; set; }
    }

    public class Qp
    {
        public Qp(List<string> employeeIdsToImport)
        {
            q = new Q(employeeIdsToImport);
        }

        public string token { get; set; }
        public Q q { get; set; }
    }

    public class GetEmployeeDataRequestCogisoftModel : IRequestCogisoftModel
    {
        public GetEmployeeDataRequestCogisoftModel(List<string> employeeIdsToImport)
        {
            qp = new Qp(employeeIdsToImport);
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