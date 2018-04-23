using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CogisoftConnector.Logic;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Controllers
{
    public class VacationBalanceApiController : ApiController
    {
        CogisoftSyncVacationDataLogic _cogisoftSyncVacationDataLogic;

        public VacationBalanceApiController()
        {
            ILogger logger = LoggerFactory.CreateLogger(null);
            _cogisoftSyncVacationDataLogic = new CogisoftSyncVacationDataLogic(logger);
        }

        /// <summary>
        /// Triggers vacations days balance synchronization between Cogisoft and emplo for all employees.
        /// Vacation balance data is retrieved from Cogisoft and passed to emplo's API.
        /// Should be run periodically by a scheduler.
        /// </summary>
        [HttpGet]
        public async Task<HttpResponseMessage> SynchronizeVacationDays()
        {
            await _cogisoftSyncVacationDataLogic.SyncAllVacationData();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
