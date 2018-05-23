using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebPages;
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
                employeeIds = new List<string>();
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

                employeeVacationDataModels.AddRange(response.GetVacationDataCollection()
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

                bool dryRun;
                if (bool.TryParse(ConfigurationManager.AppSettings["DryRun"], out dryRun) && dryRun)
                {
                    _logger.WriteLine(
                        "Importer is in DryRun mode, data retrieved from Cogisoft will be printed to log, but it won't be sent to emplo.");
                    _logger.WriteLine(request);
                }
                else
                {
                    var response = await _apiClient
                        .SendPostAsync<ImportIntegratedVacationsBalanceDataResponseModel>(
                            request, _apiConfiguration.ImportIntegratedVacationsBalanceDataUrl);

                    response.resultRows = response.resultRows.OrderBy(r => r.ExternalEmployeeId).ToList();

                    response.resultRows.ForEach(r =>
                        _logger.WriteLine(
                            $"Employee Id: {r.ExternalEmployeeId}, Import result status: [{r.OperationStatus.ToString()}]{(r.Message.IsEmpty() ? string.Empty : $", Message: {r.Message}")}",
                            MapImportStatusToLogLevel(r.OperationStatus)));
                }
            }

            if (employeeVacationDataModels.Any(m => m.MissingData))
            {
                if (retryCounter < int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]))
                {
                    Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]));

                    retryCounter++;
                    await employeeVacationDataModels.Where(m => m.MissingData)
                        .Select(m => m.Result.ExternalEmployeeId)
                        .Chunk(int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]))
                        .ToList().ForEachAsync(
                            async employeeIdsChunk =>
                                await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, retryCounter,
                                    employeeIdsChunk.ToList()));
                }
                else
                {
                    _logger.WriteLine(
                        $"Maximum retry count ({ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]}) exceeded for employees:",
                        LogLevelEnum.Error);
                    _logger.WriteLine(
                        string.Join(", ",
                            employeeVacationDataModels.Where(m => m.MissingData)
                                .Select(m => m.Result.ExternalEmployeeId).ToList()), LogLevelEnum.Error);
                }
            }
        }

        private LogLevelEnum MapImportStatusToLogLevel(ImportVacationDataStatusCode status)
        {
            switch (status)
            {
                case ImportVacationDataStatusCode.Warning:
                    return LogLevelEnum.Warning;
                case ImportVacationDataStatusCode.Error:
                    return LogLevelEnum.Error;
                default:
                    return LogLevelEnum.Information;
            }
        }

        private async Task SyncVacationData(string externalVacationTypeIdentifier, List<string> employeeIds = null, bool withRepeatedFirstCogisoftQuery = false)
        {
            try
            {
                using (var client = new CogisoftServiceClient(_logger))
                {
                    if (employeeIds == null || !employeeIds.Any())
                    {
                        await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, 0,
                            null, withRepeatedFirstCogisoftQuery);
                    }
                    else
                    {
                        await employeeIds
                            .Chunk(int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]))
                            .ToList().ForEachAsync(async employeeIdsChunk =>
                                await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, 0,
                                    employeeIdsChunk.ToList(), withRepeatedFirstCogisoftQuery));
                    }


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

        public async Task SyncVacationData(List<string> employeeIdentifiers = null)
        {
            if (employeeIdentifiers == null || !employeeIdentifiers.Any())
            {
                await SyncVacationData(ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"]);
            }
            else
            {
                await SyncVacationData(ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"], employeeIdentifiers);
            }
        }
    }
}
