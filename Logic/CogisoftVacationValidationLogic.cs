using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using CogisoftConnector.Models.Cogisoft;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Logic
{
    public class CogisoftVacationValidationLogic : ICogisoftVacationValidationLogic
    {
        private readonly ILogger _logger;
        private readonly CogisoftSyncVacationDataLogic _cogisoftSyncVacationDataLogic;

        public CogisoftVacationValidationLogic(ILogger logger, CogisoftSyncVacationDataLogic cogisoftSyncVacationDataLogic)
        {
            _logger = logger;
            _cogisoftSyncVacationDataLogic = cogisoftSyncVacationDataLogic;
        }

        public VacationValidationResponseModel ValidateVacationRequest(VacationValidationRequestModel emploRequest)
        {
            var getCalendarTask = Task.Run(() => GetEmployeeCalendar(emploRequest.Since, emploRequest.Until,
                emploRequest.ExternalEmployeeId));

            var getVacationDataTask = Task.Run(() =>
                _cogisoftSyncVacationDataLogic.GetVacationDataForSingleEmployee(emploRequest.ExternalEmployeeId,
                    emploRequest.ExternalVacationTypeId));

            Task.WaitAll(getCalendarTask, getVacationDataTask);

            return CogisoftVacationValidator.PerformValidation(getCalendarTask.Result, getVacationDataTask.Result,
                emploRequest.IsOnDemand);
        }

        private GetEmployeeCalendarForPeriodResponseCogisoftModel GetEmployeeCalendar(DateTime since,
            DateTime until, string externalEmployeeId)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                GetEmployeeCalendarForPeriodRequestCogisoftModel employeeCalendarRequest =
                    new GetEmployeeCalendarForPeriodRequestCogisoftModel(since, until, externalEmployeeId);

                var employeeCalendarResponse =
                    client.PerformRequestReceiveResponse<GetEmployeeCalendarForPeriodRequestCogisoftModel,
                        GetEmployeeCalendarForPeriodResponseCogisoftModel>(employeeCalendarRequest);

                if (employeeCalendarResponse.timetable[0].Cid == null)
                {
                    return employeeCalendarResponse;
                }

                int retryCounter = 0;
                var asyncCommissionRequest =
                    new AsyncCommisionStatusRequestCogisoftModel(employeeCalendarResponse.timetable[0].Cid);
                AsyncProcessingResultResponseCogisoftModel asyncCommissionResponse;

                do
                {
                    retryCounter++;
                    Thread.Sleep(retryCounter * 500);
                    asyncCommissionResponse =
                        client.PerformRequestReceiveResponse<AsyncCommisionStatusRequestCogisoftModel,
                            AsyncProcessingResultResponseCogisoftModel>(asyncCommissionRequest);
                } while (!asyncCommissionResponse.ci[0].processed && retryCounter < int.Parse(ConfigurationManager.AppSettings["GetVacationDataMaxRetryCount"]));

                if (asyncCommissionResponse.ci[0].processed)
                {
                    employeeCalendarResponse =
                        client.PerformRequestReceiveResponse<GetEmployeeCalendarForPeriodRequestCogisoftModel,
                            GetEmployeeCalendarForPeriodResponseCogisoftModel>(employeeCalendarRequest);
                }

                return employeeCalendarResponse;
            }
        }
    }
}