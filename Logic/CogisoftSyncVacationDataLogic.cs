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

        public IntegratedVacationsBalanceDto GetVacationDataForSingleEmployee(string employeeIdentifier)
        {
            return GetVacationData(new QueryParameters() {SkipInitialRecalculation = true, RetryInterval_ms = 1000}, employeeIdentifier.AsList()).First().Result;
        }

        public void SyncVacationDataForSingleEmployee(string employeeIdentifier)
        {
            _logger.WriteLine($"Vacation data synchronization for employee {employeeIdentifier} will be performed in 5 minutes");
            var jobId = BackgroundJob.Schedule(
                () => SyncVacationData(employeeIdentifier.AsList()),
                TimeSpan.FromMilliseconds(int.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"])));
        }

        public async Task SyncVacationData(List<string> employeeIdentifiers = null)
        {
            var vacationData = GetVacationData(new QueryParameters(), employeeIdentifiers);
            foreach (var dataChunk in vacationData.Chunk(100).ToList())
            {
                await Import(dataChunk.ToList());
            }
        }

        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationData(QueryParameters queryParameters, List<string> employeeIdentifiers)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                return GetVacationDataRecursive(queryParameters, client, employeeIdentifiers);
            }
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
                    $"Cogisoft query for recalculated data will be performed in {GetRetryInterval(queryParameters, employeeIdentifiers) / 1000} seconds");

                requestCogisoftRequest.ResetQueryIndex();
                
                Thread.Sleep(GetRetryInterval(queryParameters, employeeIdentifiers));
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
                    Thread.Sleep(GetRetryInterval(queryParameters, employeeIdentifiers));

                    retryCounter++;
                    employeeIdentifiers = modelsWithMissingData
                        .Select(m => m.Result.ExternalEmployeeId).ToList();
                    modelsFinalCollection.AddRange(GetVacationDataRecursive(queryParameters, client, employeeIdentifiers, retryCounter));
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
                }

            }


            return modelsFinalCollection;
        }

        public async Task Import(List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels)
        {
            var request = JsonConvert.SerializeObject(
                new ImportIntegratedVacationsBalanceDataRequestModel
                {
                    BalanceList = employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result).ToList()
                });

            bool dryRun;
            if (bool.TryParse(ConfigurationManager.AppSettings["DryRun"], out dryRun) && dryRun)
            {
                _logger.WriteLine(
                    "Importer is in DryRun mode, data retrieved from Cogisoft will be printed to log, but it won't be sent to emplo");
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

        private int GetRetryInterval(QueryParameters queryParameters, List<string> employeeIdentifiers)
        {
            int employeeCount = employeeIdentifiers.Count;
            if (!employeeIdentifiers.Any())
            {
                employeeCount = queryParameters.CogisoftQueryPageSize;
            }

            return queryParameters.RetryInterval_ms + 500 * employeeCount;
        }
    }
}
