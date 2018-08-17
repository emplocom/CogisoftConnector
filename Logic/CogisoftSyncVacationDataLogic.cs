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
    internal class QueryParameters
    {
        public bool SkipInitialRecalculation { get; set; }
        public int MaxRetryCount { get; set; }
        public int RetryInterval_ms { get; set; }
        public int CogisoftQueryPageSize { get; set; }
        public string DefaultExternalVacationTypeId { get; set; }

        internal QueryParameters()
        {
            SkipInitialRecalculation = false;
            MaxRetryCount = int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]);
            RetryInterval_ms = int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]);
            CogisoftQueryPageSize = int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]);
            DefaultExternalVacationTypeId = ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"];
        }
    }

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
            using (var client = new CogisoftServiceClient(_logger))
            {
                return GetVacationDataRecursive(new QueryParameters() { SkipInitialRecalculation = true, RetryInterval_ms = 1000 }, client, employeeIdentifier.AsList()).First().Result;
            }
        }

        public void SyncVacationData(DateTime synchronizationTime, List<string> employeeIdentifiers = null)
        {
            _logger.WriteLine($"Vacation data synchronization for employees {(employeeIdentifiers == null ? string.Empty : string.Join(",", employeeIdentifiers))} will be performed in {double.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"]) / 60000} minutes");
            var jobId = BackgroundJob.Schedule(
                () => SyncVacationDataInternal(synchronizationTime, employeeIdentifiers),
                TimeSpan.FromMilliseconds(int.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"])));
        }

        #region GetVacationDataLogic

        public async Task SyncVacationDataInternal(DateTime synchronizationTime, List<string> employeeIdentifiers = null)
        {
            List<IntegratedVacationsBalanceDtoWrapper> vacationData;

            bool mockMode;
            if(bool.TryParse(ConfigurationManager.AppSettings["MockMode"], out mockMode) && mockMode)
            {
                if (employeeIdentifiers != null)
                {
                    vacationData = employeeIdentifiers.Select(ei => new IntegratedVacationsBalanceDtoWrapper()
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
                            ExternalVacationTypeId = (new QueryParameters()).DefaultExternalVacationTypeId
                        }
                    }).ToList();
                }
                else
                {
                    return;
                }
            }
            else
            {
                using (var client = new CogisoftServiceClient(_logger))
                {
                    vacationData = GetVacationDataRecursive(new QueryParameters(), client, employeeIdentifiers);
                }
            }

            foreach (var dataChunk in vacationData.Where(vd => !vd.MissingData).Chunk(100).ToList())
            {
                await Import(synchronizationTime, dataChunk.ToList());
            }

            _logger.WriteLine($"Vacation data import finished");
        }

        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationDataRecursive(QueryParameters queryParameters, CogisoftServiceClient client, List<string> employeeIdentifiers, int retryCounter = 0)
        {
            if (employeeIdentifiers != null && employeeIdentifiers.Any())
            {
                _logger.WriteLine($"GetVacationData started for employees: {string.Join(", ", employeeIdentifiers)}");
            }
            else
            {
                employeeIdentifiers = new List<string>();
                _logger.WriteLine($"GetVacationData started for all employees");
            }
            _logger.WriteLine($"GetVacationData retry counter: {retryCounter}");


            GetVacationDataRequestCogisoftModel requestCogisoftRequest = new GetVacationDataRequestCogisoftModel(employeeIdentifiers);
            List<IntegratedVacationsBalanceDtoWrapper> modelsQueryResult = new List<IntegratedVacationsBalanceDtoWrapper>();
            List<IntegratedVacationsBalanceDtoWrapper> modelsFinalCollection = new List<IntegratedVacationsBalanceDtoWrapper>();
            bool anyObjectsLeft;


            if (retryCounter == 0 && !queryParameters.SkipInitialRecalculation)
            {
                //The first request might provide us with outdated data while triggering data recalculation in Cogisoft
                _logger.WriteLine(
                    $"Triggering data recalculation in Cogisoft system with a fire-and-forget request");

                do
                {
                    var response =
                        client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                            VacationDataResponseCogisoftModel>(requestCogisoftRequest);

                    anyObjectsLeft = response.AnyRemainingObjectsLeft();

                    requestCogisoftRequest.IncrementQueryIndex();
                } while (anyObjectsLeft);

                _logger.WriteLine(
                    $"Cogisoft query for recalculated data will be performed in {GetRetryInterval(queryParameters, employeeIdentifiers.Count) / 1000} seconds");

                requestCogisoftRequest.ResetQueryIndex();
                
                Thread.Sleep(GetRetryInterval(queryParameters, employeeIdentifiers.Count));
            }


            do
            {
                var response =
                    client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                        VacationDataResponseCogisoftModel>(requestCogisoftRequest);

                modelsQueryResult.AddRange(response.GetVacationDataCollection()
                    .Select(r => new IntegratedVacationsBalanceDtoWrapper(r, queryParameters.DefaultExternalVacationTypeId)));

                anyObjectsLeft = response.AnyRemainingObjectsLeft();

                requestCogisoftRequest.IncrementQueryIndex();
            } while (anyObjectsLeft);


            modelsFinalCollection.AddRange(modelsQueryResult.Where(m => !m.MissingData));


            var modelsWithMissingData = modelsQueryResult.Where(m => m.MissingData).ToList();
            if (modelsWithMissingData.Any())
            {
                if (retryCounter < queryParameters.MaxRetryCount)
                {
                    _logger.WriteLine(
                        $"Cogisoft query for missing data will be performed in {GetRetryInterval(queryParameters, modelsWithMissingData.Count) / 1000} seconds");

                    Thread.Sleep(GetRetryInterval(queryParameters, modelsWithMissingData.Count));

                    retryCounter++;
                    modelsFinalCollection.AddRange(GetVacationDataRecursive(queryParameters, client, modelsWithMissingData
                        .Select(m => m.Result.ExternalEmployeeId).ToList(), retryCounter));
                }
                else
                {
                    _logger.WriteLine(
                        $"Maximum retry count ({queryParameters.MaxRetryCount}) exceeded for employees:",
                        LogLevelEnum.Error);
                    _logger.WriteLine(
                        string.Join(", ",
                            modelsWithMissingData
                                .Select(m => m.Result.ExternalEmployeeId).ToList()), LogLevelEnum.Error);

                    modelsFinalCollection.AddRange(modelsWithMissingData
                        .Select(m => m.Result.ExternalEmployeeId).Select(e => new IntegratedVacationsBalanceDtoWrapper
                        {
                            MissingData = true,
                            Result = new IntegratedVacationsBalanceDto()
                            {
                                ExternalEmployeeId = e,
                                OnDemandDays = -1,
                                OutstandingDays = -1,
                                OutstandingHours = -1,
                                AvailableHours = -1,
                                AvailableDays = -1,
                                ExternalVacationTypeId = queryParameters.DefaultExternalVacationTypeId
                            }
                        }));
                }

            }


            return modelsFinalCollection;
        }

        private int GetRetryInterval(QueryParameters queryParameters, int? employeesCount = null)
        {
            int multiplier = 0;
            if (!employeesCount.HasValue || employeesCount == 0)
            {
                multiplier = queryParameters.CogisoftQueryPageSize;
            }
            else
            {
                multiplier = employeesCount.Value;
            }

            return queryParameters.RetryInterval_ms + 500 * multiplier;
        }

        #endregion

        #region ImportLogic

        private async Task Import(DateTime synchronizationTimestamp, List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels)
        {
            var request = JsonConvert.SerializeObject(
                new ImportIntegratedVacationsBalanceDataRequestModel
                {
                    SynchronizationTime = synchronizationTimestamp,
                    BalanceList = employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result).ToList()
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
