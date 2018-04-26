using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using EmploApiSDK.Logger;
using EmploApiSDK.Logic.EmployeeImport;

namespace CogisoftConnector.Logic
{
    public class CogisoftEmployeeImportConfiguration : BaseImportConfiguration
    {
        ///<exception cref = "EmploApiClientFatalException" > Thrown when a fatal error, requiring request abortion, has occurred </exception>
        public CogisoftEmployeeImportConfiguration(ILogger logger) : base(logger)
        {
        }
    }
}