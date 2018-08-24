using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationBalances;

namespace CogisoftConnector.Logic
{
    public interface ISyncVacationDataLogic
    {
        IntegratedVacationsBalanceDto GetVacationDataForSingleEmployee(string employeeIdentifier,
            string externalVacationTypeId);

        void SyncVacationData(DateTime synchronizationTime, string externalVacationTypeId,
            List<string> employeeIdentifiers = null);
    }
}
