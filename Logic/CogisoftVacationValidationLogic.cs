using System;
using System.Threading;
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
            using (var client = new CogisoftServiceClient(_logger))
            {
                var employeeCalendar = GetEmployeeCalendar(emploRequest.Since, emploRequest.Until,
                    emploRequest.ExternalEmployeeId, client);
                
                var employeeVacationBalance =
                    _cogisoftSyncVacationDataLogic.GetVacationDataForSingleEmployee(emploRequest.ExternalEmployeeId, emploRequest.ExternalVacationTypeId);

                return CogisoftVacationValidator.PerformValidation(employeeCalendar, employeeVacationBalance,
                    emploRequest.IsOnDemand);
            }
        }

        private GetEmployeeCalendarForPeriodResponseCogisoftModel GetEmployeeCalendar(DateTime since, DateTime until, string externalEmployeeId, CogisoftServiceClient client)
        {
            GetEmployeeCalendarForPeriodRequestCogisoftModel employeeCalendarRequest = new GetEmployeeCalendarForPeriodRequestCogisoftModel(since, until, externalEmployeeId);

            var employeeCalendarResponse =
                client.PerformRequestReceiveResponse<GetEmployeeCalendarForPeriodRequestCogisoftModel,
                    GetEmployeeCalendarForPeriodResponseCogisoftModel>(employeeCalendarRequest);

            if (employeeCalendarResponse.timetable[0].Cid == null)
            {
                return employeeCalendarResponse;
            }

            int retryCounter = 0;
            var asyncCommissionRequest = new AsyncCommisionStatusRequestCogisoftModel(employeeCalendarResponse.timetable[0].Cid);
            AsyncProcessingResultResponseCogisoftModel asyncCommissionResponse;

            do
            {
                retryCounter++;
                Thread.Sleep(retryCounter * 1000);
                asyncCommissionResponse =
                    client.PerformRequestReceiveResponse<AsyncCommisionStatusRequestCogisoftModel,
                        AsyncProcessingResultResponseCogisoftModel>(asyncCommissionRequest);
            } while (!asyncCommissionResponse.ci[0].processed && retryCounter < 10);

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