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
    public class EmployeeImportController : ApiController
    {
        private EmployeeImportLogic _employeeImportLogic;

        public EmployeeImportController()
        {
            ILogger logger = LoggerFactory.CreateLogger(null);
            _employeeImportLogic = new EmployeeImportLogic(logger);
        }

        /// <summary>
        /// Triggers employee import from Cogisoft to emplo for all employees.
        /// </summary>
        [HttpPost]
        public HttpResponseMessage SynchronizeEmployees()
        {
            _employeeImportLogic.ImportEmployeeData();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        public HttpResponseMessage Ping()
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Ok!") };
        }
    }
}
