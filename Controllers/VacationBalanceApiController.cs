using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.WebPages;
using CogisoftConnector.Logic;

namespace CogisoftConnector.Controllers
{
    public class VacationBalanceApiController : ApiController
    {
        readonly ISyncVacationDataLogic _cogisoftSyncVacationDataLogic;

        public VacationBalanceApiController(ISyncVacationDataLogic cogisoftSyncVacationDataLogic)
        {
            _cogisoftSyncVacationDataLogic = cogisoftSyncVacationDataLogic;
        }

        /// <summary>
        /// Triggers vacations days balance synchronization between Cogisoft and emplo for all employees.
        /// Vacation balance data is retrieved from Cogisoft and passed to emplo's API.
        /// Should be run periodically by a scheduler.
        /// </summary>
        [HttpGet]
        public HttpResponseMessage SynchronizeVacationDays([FromUri] string listOfIds = "", [FromUri] string vacationTypeIdentifier = "")
        {
            var externalVacationTypeId = vacationTypeIdentifier == string.Empty ? ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"] : vacationTypeIdentifier;

            if (listOfIds.IsEmpty())
            {
                _cogisoftSyncVacationDataLogic.SyncVacationData(DateTime.UtcNow, externalVacationTypeId);
            }
            else
            {
                _cogisoftSyncVacationDataLogic.SyncVacationData(DateTime.UtcNow, externalVacationTypeId, listOfIds.Split(',').ToList());
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
