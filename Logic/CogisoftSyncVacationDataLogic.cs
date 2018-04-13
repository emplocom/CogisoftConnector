using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CogisoftConnector.Models.WebhookModels.CogisoftDataModels;
using CogisoftConnector.Models.WebhookModels.CogisoftRequestModels;
using CogisoftConnector.Models.WebhookModels.CogisoftResponseModels;
using EmploApiSDK;
using EmploApiSDK.Models;
using EmploApiSDK.Logger;
using Newtonsoft.Json;

namespace CogisoftConnector.Logic
{
    public class CogisoftSyncVacationDataLogic
    {
        private ILogger _logger;
        private ApiClient _apiClient;

        ApiConfiguration _apiConfiguration = new ApiConfiguration()
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

        private void SyncVacationDataRecursive(CogisoftServiceClient client, string externalVacationTypeIdentifier, int retryCounter = 0, List<string> employeeIds = null)
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

            GetVacationDataCogisoftModel cogisoftRequest = new GetVacationDataCogisoftModel(employeeIds);

            List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels = new List<IntegratedVacationsBalanceDtoWrapper>();
            bool anyObjectsLeft;

            do
            {
                var response =
                    client.PerformRequestReceiveResponse<GetVacationDataCogisoftModel,
                        VacationDataResponseCogisoftModel>(cogisoftRequest);

                employeeVacationDataModels.AddRange(response.GetEmployeeCollection()
                    .Select(r => new IntegratedVacationsBalanceDtoWrapper(r, externalVacationTypeIdentifier)));

                anyObjectsLeft = response.AnyRemainingObjectsLeft();

                cogisoftRequest.IncrementQueryIndex();
            } while (anyObjectsLeft);

            if (employeeVacationDataModels.Any(m => !m.MissingData))
            {
                var request = JsonConvert.SerializeObject(
                    new ImportIntegratedVacationsBalanceDataRequestModel
                    {
                        BalanceList = employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result).ToList()
                    });

                var response = _apiClient
                    .SendPostAsync<ImportIntegratedVacationsBalanceDataResponseModel>(
                        request, _apiConfiguration.ImportIntegratedVacationsBalanceDataUrl).Result;

                if (response.OperationStatus == ImportVacationDataStatusCode.Ok)
                {
                    _logger.WriteLine($"Employee data synchronization succeeded for employee Ids: (retry counter: {retryCounter})");
                    _logger.WriteLine(string.Join(", ", employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result.ExternalEmployeeId)));
                }
                else
                {
                    _logger.WriteLine("An error occurred during import. Error message:");
                    _logger.WriteLine(response.ErrorMessage);
                }
            }

            if (employeeVacationDataModels.Any(m => m.MissingData))
            {
                if (retryCounter < int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]))
                {
                    System.Threading.Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]));

                    SyncVacationDataRecursive(client, externalVacationTypeIdentifier, ++retryCounter, employeeVacationDataModels.Where(m => m.MissingData)
                        .Select(m => m.Result.ExternalEmployeeId).ToList());
                }
                else
                {
                    _logger.WriteLine($"Maximum retry count ({ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]}) exceeded for employees:");
                    _logger.WriteLine(string.Join(", ", employeeVacationDataModels.Where(m => m.MissingData).Select(m => m.Result.ExternalEmployeeId).ToList()));
                }
            }
        }

        private void SyncVacationData(string externalVacationTypeIdentifier, List<string> employeeIds)
        {
            try
            {
                using (var client = new CogisoftServiceClient())
                {
                    SyncVacationDataRecursive(client, externalVacationTypeIdentifier, 0, employeeIds);
                }
            }
            catch (Exception e)
            {
                _logger.WriteLine($"An unexpected error occurred, exception: {ExceptionLoggingUtils.ExceptionAsString(e)}");
            }
        }

        public void SyncVacationDataForSingleEmployee(string externalVacationTypeIdentifier, string employeeIdentifier)
        {
            Task.Run(() =>
                SyncVacationData(externalVacationTypeIdentifier, new List<string>() { employeeIdentifier })
            );
        }

        public void SyncAllVacationData()
        {
            SyncVacationData(ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"], null);
        }
    }
}
