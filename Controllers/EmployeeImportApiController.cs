using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CogisoftConnector.Logic;
using EmploApiSDK.Logger;

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
        public HttpResponseMessage SynchronizeEmployees()
        {
            _employeeImportLogic.ImportEmployeeData();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
