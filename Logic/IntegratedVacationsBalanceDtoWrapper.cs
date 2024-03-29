using System;
using System.Linq;
using System.Web.WebPages;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationBalances;

namespace CogisoftConnector.Logic
{
    public class IntegratedVacationsBalanceDtoWrapper
    {
        public IntegratedVacationsBalanceDto Result { get; set; }
        public bool MissingData { get; set; }

        public IntegratedVacationsBalanceDtoWrapper()
        {

        }

        public IntegratedVacationsBalanceDtoWrapper(VacationDataResponseCogisoftModel.R employeeResponseCollection, string externalVacationTypeId)
        {
            Result = new IntegratedVacationsBalanceDto();

            Result.ExternalEmployeeId = employeeResponseCollection.sc[0].ToString();
            Result.ExternalVacationTypeId = externalVacationTypeId;

            if (employeeResponseCollection.sc.Skip(1).Any(r => r == null || r.ToString().IsEmpty() || r.ToString().Contains("n")))
            {
                MissingData = true;
                return;
            }

            var calculatedHoursInAWorkday = 
                CalculateHoursInAWorkday(
                    employeeResponseCollection.sc[1].ToString(), 
                    employeeResponseCollection.sc[2].ToString(), 
                    Result.ExternalEmployeeId, 
                    out Result.AvailableDays, 
                    out Result.AvailableHours);

            Result.OutstandingDays =
                ParseAndConvertDays(employeeResponseCollection.sc[3].ToString(), Result.ExternalEmployeeId, calculatedHoursInAWorkday);

            Result.OutstandingHours = ParseAndConvertHours(employeeResponseCollection.sc[4].ToString(), Result.ExternalEmployeeId);

            Result.OnDemandDays =
                ParseAndConvertDays(employeeResponseCollection.sc[5].ToString(), Result.ExternalEmployeeId, calculatedHoursInAWorkday);
        }

        private decimal ParseAndConvertDays(string days, string externalEmployeeId, decimal? hoursInAWorkday = null)
        {
            //we can get days in the "24d 4:00" format
            int indexOfD = days.IndexOf("d", StringComparison.Ordinal);
            var daysComponent = indexOfD == -1 ? days : days.Substring(0, indexOfD);

            decimal parsedDays;

            if (!decimal.TryParse(daysComponent, out parsedDays))
            {
                throw new Exception($"Could not parse days. Days' string: {daysComponent}, full string: {days}, External employee Id: {externalEmployeeId}");
            }

            if (indexOfD == -1 || !hoursInAWorkday.HasValue)
            {
                return parsedDays;
            }

            var timeComponent = days.Replace(daysComponent + "d", string.Empty).Trim();

            var parsedTimeComponent = ParseAndConvertHours(timeComponent, externalEmployeeId);

            return parsedDays + (parsedTimeComponent / hoursInAWorkday.Value);
        }

        private decimal? CalculateHoursInAWorkday(string availableDays, string availableHours, string externalEmployeeId, out decimal parsedDays, out decimal parsedHours)
        {
            int indexOfD = availableDays.IndexOf("d", StringComparison.Ordinal);
            var daysComponent = indexOfD == -1 ? availableDays : availableDays.Substring(0, indexOfD);
            decimal parsedTimeComponent = 0;

            parsedHours = ParseAndConvertHours(availableHours, Result.ExternalEmployeeId);

            if (!decimal.TryParse(daysComponent, out parsedDays))
            {
                throw new Exception($"Could not parse days. Days' string: {daysComponent}, full string: {availableDays}, External employee Id: {externalEmployeeId}");
            }

            if (indexOfD != -1 && parsedDays != 0)
            {
                var timeComponent = availableDays.Replace(daysComponent + "d", string.Empty).Trim();
                parsedTimeComponent = ParseAndConvertHours(timeComponent, externalEmployeeId);
                var hoursInAWorkday = (parsedHours - parsedTimeComponent) / parsedDays;
                parsedDays = parsedDays + (parsedTimeComponent / hoursInAWorkday);
                return hoursInAWorkday;
            }
            else
            {
                return null;
            }
        }

        private decimal ParseAndConvertHours(string hours, string externalEmployeeId)
        {
            var hourComponent = hours.Substring(0, hours.IndexOf(":", StringComparison.Ordinal));
            var minuteComponent = hours.Substring(hours.IndexOf(":", StringComparison.Ordinal) + 1);

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
