using System;
using System.Linq;
using System.Web.WebPages;
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

            if (employeeResponseCollection.sc.Skip(1).Any(r => r == null || r.ToString().IsEmpty() || r.ToString().Contains("n")))
            {
                MissingData = true;
                return;
            }

            Result.AvailableDays =
                ParseAndConvertDays(employeeResponseCollection.sc[1].ToString(), Result.ExternalEmployeeId);

            Result.AvailableHours = ParseAndConvertHours(employeeResponseCollection.sc[2].ToString(), Result.ExternalEmployeeId);

            Result.OutstandingDays =
                ParseAndConvertDays(employeeResponseCollection.sc[3].ToString(), Result.ExternalEmployeeId);

            Result.OutstandingHours = ParseAndConvertHours(employeeResponseCollection.sc[4].ToString(), Result.ExternalEmployeeId);

            Result.OnDemandDays =
                ParseAndConvertDays(employeeResponseCollection.sc[5].ToString(), Result.ExternalEmployeeId);
        }

        private decimal ParseAndConvertDays(string days, string externalEmployeeId)
        {
            //we can get days in the "24d 4:00" format - only the whole day part interests us
            int indexOfD = days.IndexOf("d", StringComparison.Ordinal);
            var onlyDaysString = indexOfD == -1 ? days : days.Substring(0, indexOfD);

            decimal parsedDays;

            if (!decimal.TryParse(onlyDaysString, out parsedDays))
            {
                throw new Exception($"Could not parse days. Days' string: {onlyDaysString}, full string: {days}, External employee Id: {externalEmployeeId}");
            }

            return parsedDays;
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
