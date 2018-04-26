using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.IntegratedVacations;
using EmploApiSDK.Client;
using EmploApiSDK.Logger;
using Newtonsoft.Json;

namespace CogisoftConnector.Logic
{
    public class CogisoftSyncVacationDataLogic
    {
        private readonly ILogger _logger;
        private readonly ApiClient _apiClient;

        readonly ApiConfiguration _apiConfiguration = new ApiConfiguration()
        {
            EmploUrl = ConfigurationManager.AppSettings["EmploUrl"],
            ApiPath = ConfigurationManager.AppSettings["ApiPath"] ?? "apiv2",
            Login = ConfigurationManager.AppSettings["Login"],
            Password = ConfigurationManager.AppSettings["Password"]
        };

        public CogisoftSyncVacationDataLogic(ILogger logger)
        {
            _logger = logger;
            _apiClient = new ApiClient(_logger, _apiConfiguration);
        }

        private async Task SyncVacationDataRecursive(CogisoftServiceClient client, string externalVacationTypeIdentifier, int retryCounter = 0, List<string> employeeIds = null, bool withRepeatedFirstCogisoftQuery = false)
        {
            if (employeeIds != null)
            {
                _logger.WriteLine($"SyncVacationData started for employees: {string.Join(", ", employeeIds)}");
            }
            else
            {
                _logger.WriteLine($"SyncVacationData started for all employees");
            }
            _logger.WriteLine($"SyncVacationData retry counter: {retryCounter}");

            GetVacationDataRequestCogisoftModel requestCogisoftRequest = new GetVacationDataRequestCogisoftModel(employeeIds);

            List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels = new List<IntegratedVacationsBalanceDtoWrapper>();
            bool anyObjectsLeft;

            do
            {
                var response =
                    client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                        VacationDataResponseCogisoftModel>(requestCogisoftRequest);

                employeeVacationDataModels.AddRange(response.GetEmployeeCollection()
                    .Select(r => new IntegratedVacationsBalanceDtoWrapper(r, externalVacationTypeIdentifier)));

                anyObjectsLeft = response.AnyRemainingObjectsLeft();

                requestCogisoftRequest.IncrementQueryIndex();
            } while (anyObjectsLeft);

            if (employeeVacationDataModels.Any(m => !m.MissingData))
            {
                var request = JsonConvert.SerializeObject(
                    new ImportIntegratedVacationsBalanceDataRequestModel
                    {
                        BalanceList = employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result).ToList()
                    });

                if (withRepeatedFirstCogisoftQuery)
                {
                    //The first request might provide us with outdated data while triggering data recalculation in Cogisoft
                    Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]));

                    request = JsonConvert.SerializeObject(
                        new ImportIntegratedVacationsBalanceDataRequestModel
                        {
                            BalanceList = employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result).ToList()
                        });
                }

                var response = await _apiClient
                    .SendPostAsync<ImportIntegratedVacationsBalanceDataResponseModel>(
                        request, _apiConfiguration.ImportIntegratedVacationsBalanceDataUrl);

                if (response.OperationStatus == ImportVacationDataStatusCode.Ok)
                {
                    _logger.WriteLine($"Employee vacation data synchronization succeeded for employee Ids: (retry counter: {retryCounter})");
                    _logger.WriteLine(string.Join(", ", employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result.ExternalEmployeeId)));
                }
                else
                {
                    _logger.WriteLine("An error occurred during import. Error message:", LogLevelEnum.Error);
                    _logger.WriteLine(response.ErrorMessage, LogLevelEnum.Error);
                }
            }

            if (employeeVacationDataModels.Any(m => m.MissingData))
            {
                if (retryCounter < int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]))
                {
                    Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]));

                    await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, ++retryCounter, employeeVacationDataModels.Where(m => m.MissingData)
                        .Select(m => m.Result.ExternalEmployeeId).ToList());
                }
                else
                {
                    _logger.WriteLine($"Maximum retry count ({ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]}) exceeded for employees:", LogLevelEnum.Error);
                    _logger.WriteLine(string.Join(", ", employeeVacationDataModels.Where(m => m.MissingData).Select(m => m.Result.ExternalEmployeeId).ToList()), LogLevelEnum.Error);
                }
            }
        }

        private async Task SyncVacationData(string externalVacationTypeIdentifier, List<string> employeeIds = null, bool withRepeatedFirstCogisoftQuery = false)
        {
            try
            {
                using (var client = new CogisoftServiceClient(_logger))
                {
                    await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, 0, employeeIds, withRepeatedFirstCogisoftQuery);
                }
            }
            catch (Exception e)
            {
                _logger.WriteLine($"An unexpected error occurred, exception: {ExceptionLoggingUtils.ExceptionAsString(e)}", LogLevelEnum.Error);
            }
        }

        private async Task SyncVacationDataForSingleEmployeeWithDelay(string externalVacationTypeIdentifier,
            string employeeIdentifier)
        {
            Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"]));
            await SyncVacationData(externalVacationTypeIdentifier, new List<string>() {employeeIdentifier}, true);
        }

        public void SyncVacationDataForSingleEmployee(string externalVacationTypeIdentifier, string employeeIdentifier)
        {
            Task.Run(() =>
                SyncVacationDataForSingleEmployeeWithDelay(externalVacationTypeIdentifier, employeeIdentifier)
            );
        }

        public async Task SyncAllVacationData()
        {
            await SyncVacationData(ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"]);
        }
    }
}
