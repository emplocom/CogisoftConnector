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
using Newtonsoft.Json;

namespace CogisoftConnector.Logic
{
    internal class SyncParameters
    {
        public List<string> EmployeeIdentifiers { get; set; }
        public bool SkipInitialRecalculation { get; set; }
        public int MaxRetryCount { get; set; }
        public int RetryInterval_ms { get; set; }
        public int CogisoftQueryPageSize { get; set; }
        public string DefaultExternalVacationTypeId { get; set; }

        internal SyncParameters(List<string> employeeIdentifiers)
        {
            EmployeeIdentifiers = employeeIdentifiers ?? new List<string>();

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
            return GetVacationData(new SyncParameters(employeeIdentifier.AsList()) {SkipInitialRecalculation = true, RetryInterval_ms = 1000}).First().Result;
        }

        public void SyncVacationDataForSingleEmployee(string employeeIdentifier)
        {
            Task.Run(() =>
                SyncVacationData(employeeIdentifier.AsList())
            );
        }

        public async Task SyncVacationData(List<string> employeeIdentifiers = null)
        {
            var vacationData = GetVacationData(new SyncParameters(employeeIdentifiers));
            foreach (var dataChunk in vacationData.Chunk(100).ToList())
            {
                await Import(dataChunk.ToList());
            }
        }

        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationData(SyncParameters syncParameters)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                return GetVacationDataRecursive(syncParameters, client);
            }
        }

        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationDataRecursive(SyncParameters syncParameters, CogisoftServiceClient client, int retryCounter = 0)
        {
            if (syncParameters.EmployeeIdentifiers.Any())
            {
                _logger.WriteLine($"SyncVacationData started for employees: {string.Join(", ", syncParameters.EmployeeIdentifiers)}");
            }
            else
            {
                _logger.WriteLine($"SyncVacationData started for all employees");
            }
            _logger.WriteLine($"SyncVacationData retry counter: {retryCounter}");


            GetVacationDataRequestCogisoftModel requestCogisoftRequest = new GetVacationDataRequestCogisoftModel(syncParameters.EmployeeIdentifiers);
            List<IntegratedVacationsBalanceDtoWrapper> modelsQueryResult = new List<IntegratedVacationsBalanceDtoWrapper>();
            List<IntegratedVacationsBalanceDtoWrapper> modelsFinalCollection = new List<IntegratedVacationsBalanceDtoWrapper>();
            bool anyObjectsLeft;


            if (retryCounter == 0 && !syncParameters.SkipInitialRecalculation)
            {
                _logger.WriteLine(
                    $"Triggering data recalculation in Cogisoft system with a fire-and-forget request.");

                do
                {
                    var response =
                        client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                            VacationDataResponseCogisoftModel>(requestCogisoftRequest);

                    anyObjectsLeft = response.AnyRemainingObjectsLeft();

                    requestCogisoftRequest.IncrementQueryIndex();
                } while (anyObjectsLeft);

                _logger.WriteLine(
                    $"Cogisoft query for recalculated data will be performed in {GetRetryInterval(syncParameters) / 1000} seconds.");
                //The first request might provide us with outdated data while triggering data recalculation in Cogisoft
                Thread.Sleep(GetRetryInterval(syncParameters));
            }


            do
            {
                requestCogisoftRequest.ResetQueryIndex();

                var response =
                    client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                        VacationDataResponseCogisoftModel>(requestCogisoftRequest);

                modelsQueryResult.AddRange(response.GetVacationDataCollection()
                    .Select(r => new IntegratedVacationsBalanceDtoWrapper(r, syncParameters.DefaultExternalVacationTypeId)));

                anyObjectsLeft = response.AnyRemainingObjectsLeft();

                requestCogisoftRequest.IncrementQueryIndex();
            } while (anyObjectsLeft);


            modelsFinalCollection.AddRange(modelsQueryResult.Where(m => !m.MissingData));


            var modelsWithMissingData = modelsQueryResult.Where(m => m.MissingData).ToList();
            if (modelsWithMissingData.Any())
            {
                if (retryCounter < syncParameters.MaxRetryCount)
                {
                    Thread.Sleep(GetRetryInterval(syncParameters));

                    retryCounter++;
                    syncParameters.EmployeeIdentifiers = modelsWithMissingData
                        .Select(m => m.Result.ExternalEmployeeId).ToList();
                    modelsFinalCollection.AddRange(GetVacationDataRecursive(syncParameters, client, retryCounter));
                }
                else
                {
                    _logger.WriteLine(
                        $"Maximum retry count ({syncParameters.MaxRetryCount}) exceeded for employees:",
                        LogLevelEnum.Error);
                    _logger.WriteLine(
                        string.Join(", ",
                            modelsWithMissingData
                                .Select(m => m.Result.ExternalEmployeeId).ToList()), LogLevelEnum.Error);
                }

            }


            return modelsFinalCollection;
        }

        private async Task Import(List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels)
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

        private int GetRetryInterval(SyncParameters syncParameters)
        {
            int employeeCount = syncParameters.EmployeeIdentifiers.Count;
            if (!syncParameters.EmployeeIdentifiers.Any())
            {
                employeeCount = syncParameters.CogisoftQueryPageSize;
            }

            return syncParameters.RetryInterval_ms + 500 * employeeCount;
        }
    }
}
