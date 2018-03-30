using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CogisoftConnector.Models.WebhookModels.CogisoftResponseModels;
using EmploApiSDK.Models;

namespace CogisoftConnector.Models.WebhookModels.CogisoftDataModels
{
    public class IntegratedVacationsBalanceDtoWrapper
    {
        public IntegratedVacationsBalanceDto Result { get; set; }
        public bool MissingData { get; set; }

        public IntegratedVacationsBalanceDtoWrapper(VacationDataResponseCogisoftModel.R employeeResponseCollection, string externalVacationTypeId)
        {
            Result = new IntegratedVacationsBalanceDto();

            Result.ExternalEmployeeId = employeeResponseCollection.sc[1].ToString();
            Result.ExternalVacationTypeId = externalVacationTypeId;

            if (!decimal.TryParse(employeeResponseCollection.sc[2].ToString(), out Result.AvailableDays))
            {
                MissingData = true;
                return;
            }

            if (!decimal.TryParse(employeeResponseCollection.sc[3].ToString().Replace(":",","), out Result.AvailableHours))
            {
                MissingData = true;
                return;
            }

            if (!decimal.TryParse(employeeResponseCollection.sc[4].ToString(), out Result.OutstandingDays))
            {
                MissingData = true;
                return;
            }

            if (!decimal.TryParse(employeeResponseCollection.sc[5].ToString().Replace(":", ","), out Result.OutstandingHours))
            {
                MissingData = true;
                return;
            }

            if (!decimal.TryParse(employeeResponseCollection.sc[6].ToString(), out Result.OnDemandDays))
            {
                MissingData = true;
                return;
            }
        }
    }
}
