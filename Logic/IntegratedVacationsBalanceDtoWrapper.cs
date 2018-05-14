using System;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.IntegratedVacations;

namespace CogisoftConnector.Logic
{
    public class IntegratedVacationsBalanceDtoWrapper
    {
        public IntegratedVacationsBalanceDto Result { get; set; }
        public bool MissingData { get; set; }

        public IntegratedVacationsBalanceDtoWrapper(VacationDataResponseCogisoftModel.R employeeResponseCollection, string externalVacationTypeId)
        {
            Result = new IntegratedVacationsBalanceDto();

            Result.ExternalEmployeeId = employeeResponseCollection.sc[0].ToString();
            Result.ExternalVacationTypeId = externalVacationTypeId;

            if (!decimal.TryParse(employeeResponseCollection.sc[1].ToString(), out Result.AvailableDays))
            {
                MissingData = true;
                return;
            }

            if (employeeResponseCollection.sc[2].ToString().Contains("n"))
            {
                MissingData = true;
                return;
            }
            else
            {
                Result.AvailableHours = ParseAndConvertHours(employeeResponseCollection.sc[2].ToString(), Result.ExternalEmployeeId);
            }

            if (!decimal.TryParse(employeeResponseCollection.sc[3].ToString(), out Result.OutstandingDays))
            {
                MissingData = true;
                return;
            }

            if (employeeResponseCollection.sc[4].ToString().Contains("n"))
            {
                MissingData = true;
                return;
            }
            else
            {
                Result.OutstandingHours = ParseAndConvertHours(employeeResponseCollection.sc[4].ToString(), Result.ExternalEmployeeId);
            }

            if (!decimal.TryParse(employeeResponseCollection.sc[5].ToString(), out Result.OnDemandDays))
            {
                MissingData = true;
                return;
            }
        }

        private decimal ParseAndConvertHours(string hours, string externalEmployeeId)
        {
            var hourComponent = hours.Substring(0, hours.IndexOf(":"));
            var minuteComponent = hours.Substring(hours.IndexOf(":") + 1);

            decimal parsedHourComponent;

            if (!decimal.TryParse(hourComponent, out parsedHourComponent))
            {
                throw new Exception($"Could not parse hour component. Hour component string: {hourComponent}, full string: {hours}, External employee Id: {externalEmployeeId}");
            }

            decimal parsedMinuteComponent;

            if (!decimal.TryParse(minuteComponent, out parsedMinuteComponent))
            {
                throw new Exception($"Could not parse minute component. Hour component string: {hourComponent}, full string: {hours}, External employee Id: {externalEmployeeId}");
            }

            return parsedHourComponent + (parsedMinuteComponent / 60.0M);
        }
    }
}
