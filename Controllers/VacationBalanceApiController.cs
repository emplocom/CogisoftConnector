using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.WebPages;
using CogisoftConnector.Logic;
using Hangfire;

namespace CogisoftConnector.Controllers
{
    public class VacationBalanceApiController : ApiController
    {
        CogisoftSyncVacationDataLogic _cogisoftSyncVacationDataLogic;

        public VacationBalanceApiController(CogisoftSyncVacationDataLogic cogisoftSyncVacationDataLogic)
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
            if (listOfIds.IsEmpty())
            {
                _cogisoftSyncVacationDataLogic.SyncVacationData(DateTime.UtcNow);
            }
            else
            {
                _cogisoftSyncVacationDataLogic.SyncVacationData(DateTime.UtcNow, listOfIds.Split(',').ToList());
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
