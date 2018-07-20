using System;
using System.Threading;
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

                var employeeCalendar = GetEmployeeCalendar(emploRequest.Since, emploRequest.Until,
                    emploRequest.ExternalEmployeeId, client);

                if (employeeCalendar.timetable[0].Cid != null)
                {
                    response.RequestIsValid = false;
                    response.Message = "Wystąpił błąd - kalendarz dla nie jest jeszcze gotowy. Zaczekaj kilka minut i spróbuj ponownie.";
                    return response;
                }

                var employeeVacationBalance =
                    _cogisoftSyncVacationDataLogic.GetVacationDataForSingleEmployee(emploRequest.ExternalEmployeeId);
                
                var workHoursDuringVacationRequest = employeeCalendar.GetWorkingHoursCount();

                if (emploRequest.IsOnDemand)
                {
                    var workDaysDuringVacationRequest = employeeCalendar.GetWorkingDaysCount();

                    response.RequestIsValid =
                        Math.Floor(employeeVacationBalance.OnDemandDays) >= workDaysDuringVacationRequest;
                    response.RequestIsValid = response.RequestIsValid =
                        employeeVacationBalance.AvailableHours >= workHoursDuringVacationRequest;

                    response.RequestIsValid = true;
                    response.Message =
                        $"Dostępne dni na żądanie: {employeeVacationBalance.OnDemandDays} d, dostępne godziny: {employeeVacationBalance.AvailableHours}, wniosek zużywa: {workDaysDuringVacationRequest} d";
                    response.AdditionalMessagesCollection.AddRange(
                        employeeCalendar.SerializeCalendarInformation());

                }
                else
                {
                    response.RequestIsValid = employeeVacationBalance.AvailableHours >= workHoursDuringVacationRequest;

                    response.Message = $"Dostępne godziny: {employeeVacationBalance.AvailableHours} h, wniosek zużywa: {workHoursDuringVacationRequest} h";
                    response.AdditionalMessagesCollection.AddRange(employeeCalendar.SerializeCalendarInformation());
                }
                
                return response;
            }
        }

        private GetEmployeeCalendarForPeriodResponseCogisoftModel GetEmployeeCalendar(DateTime since, DateTime until, string externalEmployeeId, CogisoftServiceClient client)
        {
            //return JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>(
            //    @"{ ""f"":""json"", ""timetable"":[ { ""qf"":true, ""day"":[ { ""d"":""2018-01-01"", ""type"":""WŚ"", ""e"":[ { ""name"":""Nowy Rok"", ""xsi.type"":""ns0.holiday"" } ] }, { ""d"":""2018-01-02"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-03"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-04"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-05"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-06"", ""type"":""WŚ"", ""e"":[ { ""name"":""Trzech Króli"", ""xsi.type"":""ns0.holiday"" } ] }, { ""d"":""2018-01-07"", ""type"":""WN"", ""e"":[ ] }, { ""d"":""2018-01-08"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-09"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-10"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-11"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-12"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-13"", ""type"":""W5"", ""e"":[ ] }, { ""d"":""2018-01-14"", ""type"":""WN"", ""e"":[ ] }, { ""d"":""2018-01-15"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-16"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-17"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-18"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-19"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0. shift"" } ] }, { ""d"":""2018-01-20"", ""type"":""W5"", ""e"":[ ] }, { ""d"":""2018-01-21"", ""type"":""WN"", ""e"":[ ] }, { ""d"":""2018-01-22"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-23"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-24"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-25"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-26"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-27"", ""type"":""W5"", ""e"":[ ] }, { ""d"":""2018-01-28"", ""type"":""WN"", ""e"":[ ] }, { ""d"":""2018-01-29"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-30"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] }, { ""d"":""2018-01-31"", ""type"":""R"", ""e"":[ { ""id"":1, ""from"":""07:00:00"", ""to"":""15:00:00"", ""xsi.type"":""ns0.shift"" } ] } ], ""bc"":[ { ""from"":""2018-01-01"", ""to"":""2018-01-31"", ""expectedWT"":10560, ""actualWT"":10560 } ] } ] }");

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