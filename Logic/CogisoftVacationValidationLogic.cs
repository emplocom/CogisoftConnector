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
        private readonly ISyncVacationDataLogic _syncVacationDataLogic;

        public CogisoftVacationValidationLogic(ILogger logger, ISyncVacationDataLogic syncVacationDataLogic)
        {
            _logger = logger;
            _syncVacationDataLogic = syncVacationDataLogic;
        }

        public IntegratedVacationValidationResponse ValidateVacationRequest(IntegratedVacationValidationExternalRequest emploExternalRequest)
        {
            var getCalendarTask = Task.Run(() => GetEmployeeCalendar(emploExternalRequest.Since, emploExternalRequest.Until,
                emploExternalRequest.ExternalEmployeeId));

            var getVacationDataTask = Task.Run(() =>
                _syncVacationDataLogic.GetVacationDataForSingleEmployee(emploExternalRequest.ExternalEmployeeId,
                    emploExternalRequest.ExternalVacationTypeId));

            Task.WaitAll(getCalendarTask, getVacationDataTask);

            return CogisoftVacationValidator.PerformValidation(getCalendarTask.Result, getVacationDataTask.Result,
                emploExternalRequest.IsOnDemand);
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