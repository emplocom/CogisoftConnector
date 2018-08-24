using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebPages;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationBalances;
using EmploApiSDK.Client;
using EmploApiSDK.Logger;
using Hangfire;
using Newtonsoft.Json;

namespace CogisoftConnector.Logic
{
    public class CogisoftSyncVacationDataMockLogic : ISyncVacationDataLogic
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

        public CogisoftSyncVacationDataMockLogic(ILogger logger)
        {
            _logger = logger;
            _apiClient = new ApiClient(_logger, _apiConfiguration);
        }

        public IntegratedVacationsBalanceDto GetVacationDataForSingleEmployee(string employeeIdentifier, string externalVacationTypeId)
        {
            return new IntegratedVacationsBalanceDto()
            {
                ExternalEmployeeId = employeeIdentifier,
                OnDemandDays = 4,
                OutstandingDays = 1,
                OutstandingHours = 8,
                AvailableHours = 216,
                AvailableDays = 27,
                ExternalVacationTypeId = externalVacationTypeId
            };
        }

        public void SyncVacationData(DateTime synchronizationTime, string externalVacationTypeId,
            List<string> employeeIdentifiers = null)
        {
            if (employeeIdentifiers == null || !employeeIdentifiers.Any())
            {
                return;
            }

            BackgroundJob.Enqueue(() => Import(synchronizationTime,
                GetVacationData(employeeIdentifiers, externalVacationTypeId)));
        }

        private List<IntegratedVacationsBalanceDtoWrapper> GetVacationData(List<string> employeeIdentifiers, string externalVacationTypeId)
        {
            Random rnd = new Random();
            return employeeIdentifiers.Select(ei =>
                new IntegratedVacationsBalanceDtoWrapper()
                {
                    MissingData = false,
                    Result = new IntegratedVacationsBalanceDto()
                    {
                        ExternalEmployeeId = ei,
                        OnDemandDays = rnd.Next(1, 4),
                        OutstandingDays = rnd.Next(0, 5),
                        OutstandingHours = rnd.Next(0, 40),
                        AvailableHours = rnd.Next(0, 200),
                        AvailableDays = rnd.Next(0, 26),
                        ExternalVacationTypeId = externalVacationTypeId
                    }
                }).ToList();
        }

        #region ImportLogic

        public async Task Import(DateTime synchronizationTimestamp, List<IntegratedVacationsBalanceDtoWrapper> employeeVacationDataModels)
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