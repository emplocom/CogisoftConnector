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
using Newtonsoft.Json;

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
            try
            {
                _cogisoftEmployeeImportMappingConfiguration = new CogisoftEmployeeImportConfiguration(logger);
            }
            catch (EmploApiClientFatalException e)
            {
                _logger.WriteLine($"{ExceptionLoggingUtils.ExceptionAsString(e)}", LogLevelEnum.Error);
                throw;
            }
            _apiRequestModelBuilder = new ApiRequestModelBuilder(_cogisoftEmployeeImportMappingConfiguration);
        }

        private async Task ImportEmployeeDataInternal(CogisoftServiceClient client, List<string> employeeIdsToImport = null)
        {
            if (employeeIdsToImport == null)
            {
                employeeIdsToImport = new List<string>();
                _logger.WriteLine($"Employee import started");
            }
            else
            {
                _logger.WriteLine($"Employee import started for employees {string.Join(",", employeeIdsToImport)}");
            }

            GetEmployeeDataRequestCogisoftModel cogisoftRequest = new GetEmployeeDataRequestCogisoftModel(_cogisoftEmployeeImportMappingConfiguration, employeeIdsToImport);

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

            bool dryRun;
            if (bool.TryParse(ConfigurationManager.AppSettings["DryRun"], out dryRun) && dryRun)
            {
                _logger.WriteLine("Importer is in DryRun mode, data retrieved from Cogisoft will be printed to log, but it won't be sent to emplo.");
                var serializedData = JsonConvert.SerializeObject(importUsersRequestModel.Rows);
                _logger.WriteLine(serializedData);
            }
            else
            {
                var result = await _importLogic.ImportEmployees(importUsersRequestModel);
                if (result == -1)
                {
                    throw new Exception("An error has occurred during import");
                }
            }
        }

        public async Task ImportEmployeeData(List<string> employeeIdsToImport = null)
        {
            try
            {
                using (var client = new CogisoftServiceClient(_logger))
                {
                    await ImportEmployeeDataInternal(client, employeeIdsToImport);
                }
            }
            catch (Exception e)
            {
                _logger.WriteLine($"An unexpected error has occurred, exception: {ExceptionLoggingUtils.ExceptionAsString(e)}", LogLevelEnum.Error);
            }
        }
    }
}