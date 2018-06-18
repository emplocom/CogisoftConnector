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

        //private async Task SyncVacationDataRecursive(CogisoftServiceClient client, string externalVacationTypeIdentifier, int retryCounter = 0, List<string> employeeIds = null, bool withRepeatedFirstCogisoftQuery = false)
        //{
        //    if (employeeIds != null)
        //    {
        //        _logger.WriteLine($"SyncVacationData started for employees: {string.Join(", ", employeeIds)}");
        //    }
        //    else
        //    {
        //        employeeIds = new List<string>();
        //        _logger.WriteLine($"SyncVacationData started for all employees");
        //    }
        //    _logger.WriteLine($"SyncVacationData retry counter: {retryCounter}");

        //    GetVacationDataRequestCogisoftModel requestCogisoftRequest = new GetVacationDataRequestCogisoftModel(employeeIds);

        //    List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels = new List<IntegratedVacationsBalanceDtoWrapper>();
        //    bool anyObjectsLeft;

        //    do
        //    {
        //        if (withRepeatedFirstCogisoftQuery)
        //        {
        //            _logger.WriteLine(
        //                $"Triggering data recalculation in Cogisoft system with a fire-and-forget request.");
        //        }

        //        var response =
        //            client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
        //                VacationDataResponseCogisoftModel>(requestCogisoftRequest);

        //        if (withRepeatedFirstCogisoftQuery)
        //        {
        //            _logger.WriteLine(
        //                $"Cogisoft query for recalculated data will be performed in {GetRetryInterval(employeeIds) / 1000} seconds.");
        //            //The first request might provide us with outdated data while triggering data recalculation in Cogisoft
        //            Thread.Sleep(GetRetryInterval(employeeIds));

        //            response = client.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
        //                VacationDataResponseCogisoftModel>(requestCogisoftRequest);
        //        }

        //        employeeVacationDataModels.AddRange(response.GetVacationDataCollection()
        //            .Select(r => new IntegratedVacationsBalanceDtoWrapper(r, externalVacationTypeIdentifier)));

        //        anyObjectsLeft = response.AnyRemainingObjectsLeft();

        //        requestCogisoftRequest.IncrementQueryIndex();
        //    } while (anyObjectsLeft);

        //    if (employeeVacationDataModels.Any(m => !m.MissingData))
        //    {
        //        foreach (var dataChunk in employeeVacationDataModels.Where(m => !m.MissingData).Chunk(100).ToList())
        //        {
        //            await Import(dataChunk.ToList());
        //        }
        //    }

        //    if (employeeVacationDataModels.Any(m => m.MissingData))
        //    {
        //        if (retryCounter < int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]))
        //        {
        //            Thread.Sleep(GetRetryInterval(employeeIds));

        //            retryCounter++;

        //            foreach (var employeeIdsChunk in employeeVacationDataModels.Where(m => m.MissingData)
        //                .Select(m => m.Result.ExternalEmployeeId)
        //                .Chunk(int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]))
        //                .ToList())
        //            {
        //                await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, retryCounter,
        //                    employeeIdsChunk.ToList());
        //            }
        //        }
        //        else
        //        {
        //            _logger.WriteLine(
        //                $"Maximum retry count ({ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]}) exceeded for employees:",
        //                LogLevelEnum.Error);
        //            _logger.WriteLine(
        //                string.Join(", ",
        //                    employeeVacationDataModels.Where(m => m.MissingData)
        //                        .Select(m => m.Result.ExternalEmployeeId).ToList()), LogLevelEnum.Error);
        //        }
        //    }
        //}

        //private LogLevelEnum MapImportStatusToLogLevel(ImportVacationDataStatusCode status)
        //{
        //    switch (status)
        //    {
        //        case ImportVacationDataStatusCode.Warning:
        //            return LogLevelEnum.Warning;
        //        case ImportVacationDataStatusCode.Error:
        //            return LogLevelEnum.Error;
        //        default:
        //            return LogLevelEnum.Information;
        //    }
        //}

        //private async Task SyncVacationData(string externalVacationTypeIdentifier, List<string> employeeIds = null, bool withRepeatedFirstCogisoftQuery = false)
        //{
        //    try
        //    {
        //        using (var client = new CogisoftServiceClient(_logger))
        //        {
        //            if (employeeIds == null || !employeeIds.Any())
        //            {
        //                await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, 0,
        //                    null, withRepeatedFirstCogisoftQuery);
        //            }
        //            else
        //            {
        //                foreach (var employeeIdsChunk in employeeIds
        //                    .Chunk(int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]))
        //                    .ToList())
        //                {
        //                    await SyncVacationDataRecursive(client, externalVacationTypeIdentifier, 0,
        //                        employeeIdsChunk.ToList(), withRepeatedFirstCogisoftQuery);
        //                }
        //            }


        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.WriteLine($"An unexpected error occurred, exception: {ExceptionLoggingUtils.ExceptionAsString(e)}", LogLevelEnum.Error);
        //    }
        //}

        //private async Task SyncVacationDataForSingleEmployeeWithDelay(string externalVacationTypeIdentifier,
        //    string employeeIdentifier)
        //{
        //    _logger.WriteLine(
        //        $"Vacation balance synchronization for employee {employeeIdentifier} will be performed in {int.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"]) / 1000} seconds.");
        //    Thread.Sleep(int.Parse(ConfigurationManager.AppSettings["EmployeeVacationBalanceSynchronizationDelay_ms"]));
        //    await SyncVacationData(externalVacationTypeIdentifier, new List<string>() {employeeIdentifier}, true);
        //}

        //public void SyncVacationDataForSingleEmployee(string externalVacationTypeIdentifier, string employeeIdentifier)
        //{
        //    Task.Run(() =>
        //        SyncVacationDataForSingleEmployeeWithDelay(externalVacationTypeIdentifier, employeeIdentifier)
        //    );
        //}

        //public async Task SyncVacationData(List<string> employeeIdentifiers = null)
        //{
        //    if (employeeIdentifiers == null || !employeeIdentifiers.Any())
        //    {
        //        await SyncVacationData(ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"]);
        //    }
        //    else
        //    {
        //        await SyncVacationData(ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"], employeeIdentifiers);
        //    }
        //}

        //private int GetRetryInterval(List<string> employeeIds)
        //{
        //    int employeeCount = 0;

        //    if (employeeIds == null || !employeeIds.Any())
        //    {
        //        employeeCount = int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]);
        //    }
        //    else
        //    {
        //        employeeCount = employeeIds.Count;
        //    }

        //    return int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]) + 200 * employeeCount;
        //}

        //private async Task Import(List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels)
        //{
        //    var request = JsonConvert.SerializeObject(
        //        new ImportIntegratedVacationsBalanceDataRequestModel
        //        {
        //            BalanceList = employeeVacationDataModels.Where(m => !m.MissingData).Select(m => m.Result).ToList()
        //        });

        //    bool dryRun;
        //    if (bool.TryParse(ConfigurationManager.AppSettings["DryRun"], out dryRun) && dryRun)
        //    {
        //        _logger.WriteLine(
        //            "Importer is in DryRun mode, data retrieved from Cogisoft will be printed to log, but it won't be sent to emplo.");
        //        _logger.WriteLine(request);
        //    }
        //    else
        //    {
        //        var response = await _apiClient
        //            .SendPostAsync<ImportIntegratedVacationsBalanceDataResponseModel>(
        //                request, _apiConfiguration.ImportIntegratedVacationsBalanceDataUrl);

        //        response.resultRows = response.resultRows.OrderBy(r => r.ExternalEmployeeId).ToList();

        //        response.resultRows.ForEach(r =>
        //            _logger.WriteLine(
        //                $"Employee Id: {r.ExternalEmployeeId}, Import result status: [{r.OperationStatus.ToString()}]{(r.Message.IsEmpty() ? string.Empty : $", Message: {r.Message}")}",
        //                MapImportStatusToLogLevel(r.OperationStatus)));
        //    }
        //}



        //------------------------------------------------------------//

        public IntegratedVacationsBalanceDto GetVacationDataForSingleEmployee(string employeeIdentifier)
        {
            return GetVacationData(employeeIdentifier.AsList()).First().Result;
        }

        public void SyncVacationDataForSingleEmployee(string employeeIdentifier)
        {
            Task.Run(() =>
                SyncVacationData(employeeIdentifier.AsList())
            );
        }

        public async Task SyncVacationData(List<string> employeeIdentifiers = null)
        {
            var vacationData = GetVacationData(employeeIdentifiers);
            foreach (var dataChunk in vacationData.Chunk(100).ToList())
            {
                await Import(dataChunk.ToList());
            }
        }

        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationData(List<string> employeeIdentifiers = null)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                return GetVacationDataRecursive(client, employeeIdentifiers);
            }
        }

        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationDataRecursive(CogisoftServiceClient cogisoftClient, List<string> employeeIdentifiers = null, int retryCounter = 0)
        {
            if (employeeIdentifiers != null)
            {
                _logger.WriteLine($"SyncVacationData started for employees: {string.Join(", ", employeeIdentifiers)}");
            }
            else
            {
                employeeIdentifiers = new List<string>();
                _logger.WriteLine($"SyncVacationData started for all employees");
            }
            _logger.WriteLine($"SyncVacationData retry counter: {retryCounter}");


            GetVacationDataRequestCogisoftModel requestCogisoftRequest = new GetVacationDataRequestCogisoftModel(employeeIdentifiers);
            List<IntegratedVacationsBalanceDtoWrapper> modelsQueryResult = new List<IntegratedVacationsBalanceDtoWrapper>();
            List<IntegratedVacationsBalanceDtoWrapper> modelsFinalCollection = new List<IntegratedVacationsBalanceDtoWrapper>();
            bool anyObjectsLeft;


            if (retryCounter == 0)
            {
                _logger.WriteLine(
                    $"Triggering data recalculation in Cogisoft system with a fire-and-forget request.");

                do
                {
                    var response =
                        cogisoftClient.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                            VacationDataResponseCogisoftModel>(requestCogisoftRequest);

                    anyObjectsLeft = response.AnyRemainingObjectsLeft();

                    requestCogisoftRequest.IncrementQueryIndex();
                } while (anyObjectsLeft);

                _logger.WriteLine(
                    $"Cogisoft query for recalculated data will be performed in {GetRetryInterval(employeeIdentifiers.Count) / 1000} seconds.");
                //The first request might provide us with outdated data while triggering data recalculation in Cogisoft
                Thread.Sleep(GetRetryInterval(employeeIdentifiers.Count));
            }


            do
            {
                var response =
                    cogisoftClient.PerformRequestReceiveResponse<GetVacationDataRequestCogisoftModel,
                        VacationDataResponseCogisoftModel>(requestCogisoftRequest);

                modelsQueryResult.AddRange(response.GetVacationDataCollection()
                    .Select(r => new IntegratedVacationsBalanceDtoWrapper(r, ConfigurationManager.AppSettings["DefaultVacationTypeIdForSynchronization"])));

                anyObjectsLeft = response.AnyRemainingObjectsLeft();

                requestCogisoftRequest.IncrementQueryIndex();
            } while (anyObjectsLeft);


            modelsFinalCollection.AddRange(modelsQueryResult.Where(m => !m.MissingData));


            var modelsWithMissingData = modelsQueryResult.Where(m => m.MissingData).ToList();
            if (modelsWithMissingData.Any())
            {
                if (retryCounter < int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]))
                {
                    Thread.Sleep(GetRetryInterval(modelsWithMissingData.Count));

                    retryCounter++;

                    modelsFinalCollection.AddRange(GetVacationDataRecursive(cogisoftClient, modelsWithMissingData
                        .Select(m => m.Result.ExternalEmployeeId).ToList(), retryCounter));
                }
                else
                {
                    _logger.WriteLine(
                        $"Maximum retry count ({ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]}) exceeded for employees:",
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

        private int GetRetryInterval(int? employeeCount)
        {
            if (!employeeCount.HasValue)
            {
                employeeCount = int.Parse(ConfigurationManager.AppSettings["CogisoftQueryPageSize"]);
            }

            return int.Parse(ConfigurationManager.AppSettings["GetVacationDataRetryInterval_ms"]) + 500 * employeeCount.Value;
        }
    }
}
