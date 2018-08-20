using System;
using System.Configuration;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationBalances;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;

namespace CogisoftConnector.Logic
{
    public class CogisoftVacationValidator
    {
        private const string ErrorNotEnoughOnDemandDays = "Zbyt mało dostępnych dni na żądanie. ";
        private const string ErrorNotEnoughHours = "Zbyt mało dostępnych godzin urlopu. ";
        private const string ErrorNotEnoughOnDemandDaysAndHours = "Zbyt mało dostępnych dni na żądanie oraz godzin urlopu. ";

        public static IntegratedVacationValidationResponse PerformValidation(GetEmployeeCalendarForPeriodResponseCogisoftModel employeeCalendar, IntegratedVacationsBalanceDto employeeVacationBalance, bool isOnDemand)
        {
            IntegratedVacationValidationResponse response = new IntegratedVacationValidationResponse();

            if (employeeCalendar.timetable[0].Cid != null)
            {
                response.RequestIsValid = false;
                response.Message = "Wystąpił błąd - kalendarz nie jest jeszcze gotowy. Zaczekaj kilka minut i spróbuj ponownie.";
                return response;
            }

            var workHoursDuringVacationRequest = employeeCalendar.GetWorkingHoursCount();

            if (isOnDemand)
            {
                var workDaysDuringVacationRequest = employeeCalendar.GetWorkingDaysCount();

                var enoughDays = Math.Floor(employeeVacationBalance.OnDemandDays) >= workDaysDuringVacationRequest;
                var enoughHours = employeeVacationBalance.AvailableHours >= workHoursDuringVacationRequest;

                response.RequestIsValid = enoughDays && enoughHours;

                string errorMessage = string.Empty;

                if (!enoughDays && !enoughHours)
                {
                    errorMessage = ErrorNotEnoughOnDemandDaysAndHours;
                }
                else if (!enoughDays)
                {
                    errorMessage = ErrorNotEnoughOnDemandDays;
                }
                else if (!enoughHours)
                {
                    errorMessage = ErrorNotEnoughHours;
                }

                response.Message =
                    $"{errorMessage}Dostępne dni na żądanie/godziny urlopu wypoczynkowego: {employeeVacationBalance.OnDemandDays} d / {employeeVacationBalance.AvailableHours} h, wniosek zużywa: {workDaysDuringVacationRequest} d / {workHoursDuringVacationRequest} h";
                response.AdditionalMessagesCollection.AddRange(
                    employeeCalendar.SerializeCalendarInformation());
            }
            else
            {
                var enoughHours = employeeVacationBalance.AvailableHours >= workHoursDuringVacationRequest;
                response.RequestIsValid = enoughHours;

                string errorMessage = enoughHours ? string.Empty : ErrorNotEnoughHours;

                response.Message = $"{errorMessage}Dostępne godziny: {employeeVacationBalance.AvailableHours} h, wniosek zużywa: {workHoursDuringVacationRequest} h";
                response.AdditionalMessagesCollection.AddRange(employeeCalendar.SerializeCalendarInformation());
            }

            return response;
        }
    }
}