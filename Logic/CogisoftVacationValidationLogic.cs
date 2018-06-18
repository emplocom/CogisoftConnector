using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CogisoftConnector.Models.Cogisoft;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Logic
{
    public class CogisoftVacationValidationLogic
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
            VacationValidationResponseModel response = new VacationValidationResponseModel();

            using (var client = new CogisoftServiceClient(_logger))
            {
                GetEmployeeCalendarForPeriodRequestCogisoftModel employeeCalendarRequest = new GetEmployeeCalendarForPeriodRequestCogisoftModel(emploRequest.Since, emploRequest.Until, emploRequest.ExternalEmployeeId);

                var employeeCalendarResponse =
                    client.PerformRequestReceiveResponse<GetEmployeeCalendarForPeriodRequestCogisoftModel,
                        GetEmployeeCalendarForPeriodResponseCogisoftModel>(employeeCalendarRequest);

                var employeeVacationBalance =
                    _cogisoftSyncVacationDataLogic.GetVacationDataForSingleEmployee(emploRequest.ExternalEmployeeId);

                //var workDaysDuringVacationRequest = employeeCalendarResponse.GetWorkingDaysCount();

                //if (emploRequest.IsOnDemand)
                //{
                //    if (employeeVacationBalance.OnDemandDays >= workDaysDuringVacationRequest)
                //    {
                //        response.RequestIsValid = true;
                //    }

                //    response.RequestIsValid = false;
                //}
                //else
                //{
                //    if (workDaysDuringVacationRequest <= Math.Floor(employeeVacationBalance.AvailableDays))
                //    {
                //        response.RequestIsValid = true;
                //    }
                //    else if (employeeVacationBalance.AvailableDays == 0 && employeeVacationBalance.AvailableHours > 0 && workDaysDuringVacationRequest == 1)
                //    {
                //        //TODO: shift validation...?
                //        response.RequestIsValid = false;
                //    }

                //    response.RequestIsValid = false;
                //}

                var workDaysDuringVacationRequest = employeeCalendarResponse.GetWorkingDaysCount();
                var workHoursDuringVacationRequest = employeeCalendarResponse.GetWorkingHoursCount();

                if (emploRequest.IsOnDemand)
                {
                    if (Math.Floor(employeeVacationBalance.OnDemandDays) >= workDaysDuringVacationRequest)
                    {
                        response.RequestIsValid = true;
                    }
                    else
                    {
                        response.RequestIsValid = false;
                        response.ValidationMessageCollection.Add($"Dostępne dni: {employeeVacationBalance.OnDemandDays} d, wniosek zużyłby: {workDaysDuringVacationRequest} d");
                        response.ValidationMessageCollection.AddRange(employeeCalendarResponse.SerializeCalendarInformation());
                    }
                }
                else
                {
                    if (employeeVacationBalance.AvailableHours >= workHoursDuringVacationRequest)
                    {
                        response.RequestIsValid = true;
                    }
                    else
                    {
                        response.RequestIsValid = false;
                        response.ValidationMessageCollection.Add($"Dostępne godziny: {employeeVacationBalance.AvailableHours} h, wniosek zużyłby: {workHoursDuringVacationRequest} h");
                        response.ValidationMessageCollection.AddRange(employeeCalendarResponse.SerializeCalendarInformation());
                    }
                }
                
                return response;
            }
        }
    }
}