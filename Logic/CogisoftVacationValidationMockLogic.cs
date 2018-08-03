﻿using System;
using System.Threading;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationBalances;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;
using Newtonsoft.Json;

namespace CogisoftConnector.Logic
{
    public class CogisoftVacationValidationMockLogic : ICogisoftVacationValidationLogic
    {
        public VacationValidationResponseModel ValidateVacationRequest(VacationValidationRequestModel emploRequest)
        {
            var employeeCalendar = GetMockedEmployeeCalendar(emploRequest.Since, emploRequest.Until);
            var employeeVacationBalance = GetMockedBalance(emploRequest.ExternalEmployeeId, emploRequest.ExternalVacationTypeId);

            return CogisoftVacationValidator.PerformValidation(employeeCalendar, employeeVacationBalance,
                emploRequest.IsOnDemand);
        }

        private GetEmployeeCalendarForPeriodResponseCogisoftModel GetMockedEmployeeCalendar(DateTime since, DateTime until)
        {
            if (since == new DateTime(2018, 8, 1) && until == new DateTime(2018, 8, 7))
            {
                return JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>("{\"f\":\"json\",\"timetable\":[{\"qf\":false,\"day\":[{\"d\":\"2018-08-01\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-02\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-03\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-04\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-05\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-06\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-07\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]}],\"bc\":[]}]}");
            }
            if (since == new DateTime(2018, 8, 1) && until == new DateTime(2018, 8, 31))
            {
                return JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>("{\"f\":\"json\",\"timetable\":[{\"qf\":false,\"day\":[{\"d\":\"2018-08-01\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-02\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-03\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-04\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-05\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-06\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-07\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-08\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-09\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-10\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-11\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-12\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-13\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-14\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-15\",\"type\":\"WŚ\",\"e\":[{\"name\":\"Wniebowzięcie Najświętszej Marii Panny\",\"xsi.type\":\"ns0.holiday\"}]},{\"d\":\"2018-08-16\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-17\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-18\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-19\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-20\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-21\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-22\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-23\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-24\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-25\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-26\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-27\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-28\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-29\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-30\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-31\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]}],\"bc\":[]}]}");
            }
            if (since == new DateTime(2018, 8, 1) && until == new DateTime(2018, 9, 30))
            {
                return JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>("{\"f\":\"json\",\"timetable\":[{\"qf\":false,\"day\":[{\"d\":\"2018-08-01\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-02\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-03\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-04\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-05\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-06\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-07\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-08\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-09\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-10\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-11\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-12\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-13\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-14\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-15\",\"type\":\"WŚ\",\"e\":[{\"name\":\"Wniebowzięcie Najświętszej Marii Panny\",\"xsi.type\":\"ns0.holiday\"}]},{\"d\":\"2018-08-16\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-17\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-18\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-19\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-20\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-21\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-22\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-23\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-24\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-25\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-08-26\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-08-27\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-28\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-29\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-30\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-31\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-01\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-09-02\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-09-03\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-04\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-05\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-06\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-07\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-08\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-09-09\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-09-10\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-11\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-12\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-13\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-14\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-15\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-09-16\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-09-17\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-18\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-19\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-20\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-21\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-22\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-09-23\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-09-24\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-25\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-26\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-27\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-28\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-09-29\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-09-30\",\"type\":\"WN\",\"e\":[]}],\"bc\":[{\"from\":\"2018-07-01\",\"to\":\"2018-09-30\",\"expectedWT\":30720,\"actualWT\":30720}]}]}");
            }
            if (since == new DateTime(2018, 8, 1) && until == new DateTime(2018, 8, 3))
            {
                return JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>("{\"f\":\"json\",\"timetable\":[{\"qf\":false,\"day\":[{\"d\":\"2018-08-01\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-02\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-03\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]}],\"bc\":[]}]}");
            }
            if (since == new DateTime(2018, 11, 1) && until == new DateTime(2018, 11, 15))
            {
                Thread.Sleep(20000);
                return JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>("{\"f\":\"json\",\"timetable\":[{\"qf\":false,\"day\":[{\"d\":\"2018-11-01\",\"type\":\"WŚ\",\"e\":[{\"name\":\"Uroczystość Wszystkich Świętych\",\"xsi.type\":\"ns0.holiday\"}]},{\"d\":\"2018-11-02\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-03\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-11-04\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-11-05\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-06\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-07\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-08\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-09\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-10\",\"type\":\"W5\",\"e\":[]},{\"d\":\"2018-11-11\",\"type\":\"WN\",\"e\":[]},{\"d\":\"2018-11-12\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-13\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-14\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-11-15\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]}],\"bc\":[]}]}");
            }

            Thread.Sleep(55000);
            var unfinishedCalendar = JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>("{\"f\":\"json\",\"timetable\":[{\"qf\":false,\"day\":[{\"d\":\"2018-08-01\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-02\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]},{\"d\":\"2018-08-03\",\"type\":\"R\",\"e\":[{\"id\":1,\"from\":\"07:30:00\",\"to\":\"15:30:00\",\"xsi.type\":\"ns0.shift\"}]}],\"bc\":[]}]}");
            unfinishedCalendar.timetable[0].Cid = "jakasWartosc";
            return unfinishedCalendar;
        }

        private IntegratedVacationsBalanceDto GetMockedBalance(string externalEmployeeId, string externalVacationTypeId)
        {
            return new IntegratedVacationsBalanceDto()
            {
                ExternalEmployeeId = externalEmployeeId,
                OnDemandDays = 4,
                OutstandingDays = 1,
                OutstandingHours = 8,
                AvailableHours = 216,
                AvailableDays = 27,
                ExternalVacationTypeId = externalVacationTypeId
            };
        }
    }
}