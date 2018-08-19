using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebPages;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationBalances;
using EmploApiSDK.Client;
using EmploApiSDK.Logger;
using Hangfire;
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

        public IntegratedVacationsBalanceDto GetVacationDataForSingleEmployee(string employeeIdentifier, string externalVacationTypeId)
        {
            int retryCounter = 0;
            IntegratedVacationsBalanceDtoWrapper balance = GetVacationData(employeeIdentifier.AsList(), externalVacationTypeId).First();

            while (retryCounter++ < int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]) && balance != null && balance.MissingData)
            {
                Thread.Sleep(retryCounter * 500);
                balance = GetVacationData(employeeIdentifier.AsList(), externalVacationTypeId).First();
            }

            if (balance == null || balance.MissingData)
            {
                throw new Exception("Nie uda³o siê pobraæ balansu urlopowego dla pracownika.");
            }
            else
            {
                return balance.Result;
            }
        }

        public void SyncVacationData(DateTime synchronizationTime, string externalVacationTypeId, List<string> employeeIdentifiers = null)
        {
            _logger.WriteLine($"Vacation data synchronization for employees {(employeeIdentifiers == null ? string.Empty : string.Join(",", employeeIdentifiers))} will be performed in {double.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"]) / 60000} minutes");
            var jobId = BackgroundJob.Schedule(
                () => SyncVacationDataInternal(synchronizationTime, externalVacationTypeId, employeeIdentifiers, 0),
                TimeSpan.FromMilliseconds(int.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"])));
        }

        #region GetVacationDataLogic

        public async Task SyncVacationDataInternal(DateTime synchronizationTime, string externalVacationTypeId, List<string> employeeIdentifiers, int counter = 0)
        {
            bool mockMode;
            if (bool.TryParse(ConfigurationManager.AppSettings["MockMode"], out mockMode) && mockMode &&
                employeeIdentifiers != null)
            {
                await Import(synchronizationTime, employeeIdentifiers.Select(ei =>
                    new IntegratedVacationsBalanceDtoWrapper()
                    {
                        MissingData = false,
                        Result = new IntegratedVacationsBalanceDto()
                        {
                            ExternalEmployeeId = ei,
                            OnDemandDays = -1,
                            OutstandingDays = -1,
                            OutstandingHours = -1,
                            AvailableHours = -1,
                            AvailableDays = -1,
                            ExternalVacationTypeId = externalVacationTypeId
                        }
                    }).ToList());
                return;
            }


            if (counter == 0)
            {
                //The first request might provide us with outdated data.
                //That's why we're triggering data recalculation with the first request to Cogisoft.
                _logger.WriteLine(
                    $"Triggering data recalculation in Cogisoft system with a fire-and-forget request");

                GetVacationData(employeeIdentifiers, externalVacationTypeId);

                _logger.WriteLine(
                    $"Cogisoft query for recalculated data will be performed in {GetOperationDelay(employeeIdentifiers).TotalSeconds} seconds");

                var closureSecureCounter = counter + 1;
                var jobId = BackgroundJob.Schedule(
                    () => SyncVacationDataInternal(synchronizationTime, externalVacationTypeId, employeeIdentifiers, closureSecureCounter),
                    GetOperationDelay(employeeIdentifiers));

                return;
            }
            else if (counter >= int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]))
            {
                if (employeeIdentifiers != null && employeeIdentifiers.Any())
                {
                    _logger.WriteLine(
                        $"Maximum retry count ({int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"])}) exceeded for employees:",
                        LogLevelEnum.Error);
                    _logger.WriteLine(
                        string.Join(", ", employeeIdentifiers), LogLevelEnum.Error);

                    return;
                }
                else
                {
                    _logger.WriteLine(
                        $"Maximum retry count ({int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"])}) exceeded for all employees.",
                        LogLevelEnum.Error);

                    return;
                }
            }

            var vacationData = GetVacationData(employeeIdentifiers, externalVacationTypeId);

            if (vacationData.Any(vd => !vd.MissingData))
            {
                await Import(synchronizationTime, vacationData);
            }

            var missingData = vacationData.Where(vd => vd.MissingData).ToList();

            if (missingData.Any())
            {
                _logger.WriteLine(
                    $"Cogisoft query for missing data will be performed in {GetOperationDelay(missingData).TotalSeconds} seconds");

                var closureSecureCounter = counter + 1;
                var jobId = BackgroundJob.Schedule(
                    () => SyncVacationDataInternal(synchronizationTime, externalVacationTypeId, missingData.Select(md => md.Result.ExternalEmployeeId).ToList(), closureSecureCounter),
                    GetOperationDelay(missingData));
            }
        }


        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationData(List<string> employeeIdentifiers, string externalVacationTypeId)
        {
            GetVacationDataRequestCogisoftModel cogisoftRequest = new GetVacationDataRequestCogisoftModel(employeeIdentifiers);
            List<IntegratedVacationsBalanceDtoWrapper> modelsQueryResult = new List<IntegratedVacationsBalanceDtoWrapper>();

            if (employeeIdentifiers != null && employeeIdentifiers.Any())
            {
                _logger.WriteLine($"GetVacationData started for employees: {string.Join(", ", employeeIdentifiers)}");
            }
            else
            {
                _logger.WriteLine($"GetVacationData started for all employees");
            }

            using (var client = new CogisoftServiceClient(_logger))
            {
                bool anyObjectsLeft;
                do
                {
                    var response =
                        client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                            VacationDataResponseCogisoftModel>(cogisoftRequest);

                    anyObjectsLeft = response.AnyRemainingObjectsLeft();

                    cogisoftRequest.IncrementQueryIndex();

                    modelsQueryResult.AddRange(response.GetVacationDataCollection()
                        .Select(r => new IntegratedVacationsBalanceDtoWrapper(r, externalVacationTypeId)));
                } while (anyObjectsLeft);
            }

            return modelsQueryResult;
        }

        private TimeSpan GetOperationDelay<T>(List<T> employeeIdentifiers)
        {
            int multiplier = 0;

            if (employeeIdentifiers == null || !employeeIdentifiers.Any())
            {
                multiplier = int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]);
            }
            else
            {
                multiplier = employeeIdentifiers.Count;
            }

            return TimeSpan.FromMilliseconds(
                int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]) + 500 * multiplier);
        }

        #endregion

        #region ImportLogic

        private async Task Import(DateTime synchronizationTimestamp, List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels)
        {
            foreach (var importDataChunk in employeeVacationDataModels.Where(m => !m.MissingData).Chunk(100))
            {
                var request = JsonConvert.SerializeObject(
                    new ImportIntegratedVacationsBalanceDataRequestModel
                    {
                        SynchronizationTime = synchronizationTimestamp,
                        BalanceList = importDataChunk.Select(m => m.Result).ToList()
                    });

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

        #endregion
    }
}
