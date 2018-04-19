using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Employees;
using EmploApiSDK.Logger;
using EmploApiSDK.Logic.EmployeeImport;

namespace CogisoftConnector.Logic
{
    public class EmployeeImportLogic
    {
        private readonly ILogger _logger;

        private readonly ImportLogic _importLogic;

        private readonly ApiRequestModelBuilder _apiRequestModelBuilder;

        private readonly CogisoftEmployeeImportConfiguration _cogisoftEmployeeImportMappingConfiguration;

        public EmployeeImportLogic(ILogger logger)
        {
            _logger = logger;
            _importLogic = new ImportLogic(logger);
            _cogisoftEmployeeImportMappingConfiguration = new CogisoftEmployeeImportConfiguration(logger);
            _apiRequestModelBuilder = new ApiRequestModelBuilder(logger, _cogisoftEmployeeImportMappingConfiguration);
        }

        private async Task ImportEmployeeDataInternal(CogisoftServiceClient client)
        {
            _logger.WriteLine($"Employee import started");

            GetEmployeeDataRequestCogisoftModel cogisoftRequest = new GetEmployeeDataRequestCogisoftModel(_cogisoftEmployeeImportMappingConfiguration);

            var importMode = ConfigurationManager.AppSettings["ImportMode"];
            var requireRegistrationForNewEmployees = ConfigurationManager.AppSettings["RequireRegistrationForNewEmployees"];

            ImportUsersRequestModel importUsersRequestModel = new ImportUsersRequestModel(importMode, requireRegistrationForNewEmployees)
            {
                Rows = new List<UserDataRow>()
            };

            bool anyObjectsLeft;

            do
            {
                var cogisoftResponse =
                    client.PerformRequestReceiveResponse<GetEmployeeDataRequestCogisoftModel,
                        GetEmployeeDataResponseCogisoftModel>(cogisoftRequest);

                importUsersRequestModel.Rows.AddRange(cogisoftResponse.GetEmployeeCollection()
                    .Select(_apiRequestModelBuilder.BuildUserDataRow));

                anyObjectsLeft = cogisoftResponse.AnyRemainingObjectsLeft();

                cogisoftRequest.IncrementQueryIndex();
            } while (anyObjectsLeft);

            var result = await _importLogic.ImportEmployees(importUsersRequestModel);

            if (result == -1)
            {
                throw new Exception("An error occurred during import, check import logs for details");
            }
        }

        public async Task ImportEmployeeData()
        {
            try
            {
                using (var client = new CogisoftServiceClient(_logger))
                {
                    await ImportEmployeeDataInternal(client);
                }
            }
            catch (Exception e)
            {
                _logger.WriteLine($"An unexpected error occurred, exception: {ExceptionLoggingUtils.ExceptionAsString(e)}");
            }
        }
    }
}