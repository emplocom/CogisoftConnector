using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.WebPages;
using CogisoftConnector.Logic;
using EmploApiSDK.Logger;
using Hangfire;

namespace CogisoftConnector.Controllers
{
    public class EmployeeImportApiController : ApiController
    {
        private EmployeeImportLogic _employeeImportLogic;

        public EmployeeImportApiController()
        {
            ILogger logger = LoggerFactory.CreateLogger(null);
            _employeeImportLogic = new EmployeeImportLogic(logger);
        }

        /// <summary>
        /// Triggers employee import from Cogisoft to emplo for all employees.
        /// </summary>
        [HttpGet]
        public async Task<HttpResponseMessage> SynchronizeEmployees([FromUri] string listOfIds = "")
        {
            await _employeeImportLogic.ImportEmployeeData();
            if (listOfIds.IsEmpty())
            {
                var jobId = BackgroundJob.Enqueue(
                    () => _employeeImportLogic.ImportEmployeeData(null));
            }
            else
            {
                var jobId = BackgroundJob.Enqueue(
                    () => _employeeImportLogic.ImportEmployeeData(listOfIds.Split(',').ToList()));
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
