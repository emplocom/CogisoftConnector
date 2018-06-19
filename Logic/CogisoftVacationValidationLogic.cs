using System;
using CogisoftConnector.Models.Cogisoft;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;
using EmploApiSDK.Logger;
using Newtonsoft.Json;

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

                //var employeeCalendarResponse =
                //    client.PerformRequestReceiveResponse<GetEmployeeCalendarForPeriodRequestCogisoftModel,
                //        GetEmployeeCalendarForPeriodResponseCogisoftModel>(employeeCalendarRequest);

                var employeeCalendarResponse =
                    JsonConvert.DeserializeObject<GetEmployeeCalendarForPeriodResponseCogisoftModel>(@"
{ ""calendar"": { ""d"":[ { ""date"":""2018-01-01"", ""type"":""W"", ""e"":[ { ""type"":""shift"", ""id"":""1"", ""from"":""08:00"", ""to"":""16:00"" } ]}, { ""date"":""2018-01-02"", ""type"":""W5"" }, { ""date"":""2018-01-03"", ""type"":""WN"", ""e"":[ { ""type"":""holiday"", ""name"":""Śledzik"" } ] } ], ""m"":[ {""nr"":""1"", ""balance"":""5""} ] } }
");

                var employeeVacationBalance =
                    _cogisoftSyncVacationDataLogic.GetVacationDataForSingleEmployee(emploRequest.ExternalEmployeeId);


                var workDaysDuringVacationRequest = employeeCalendarResponse.GetWorkingDaysCount();
                var workHoursDuringVacationRequest = employeeCalendarResponse.GetWorkingHoursCount();

                if (emploRequest.IsOnDemand)
                {
                    response.RequestIsValid = Math.Floor(employeeVacationBalance.OnDemandDays) >= workDaysDuringVacationRequest;

                    response.ValidationMessageCollection.Add($"Dostępne dni: {employeeVacationBalance.OnDemandDays} d, wniosek zużyłby: {workDaysDuringVacationRequest} d");
                    response.ValidationMessageCollection.AddRange(employeeCalendarResponse.SerializeCalendarInformation());
                }
                else
                {
                    response.RequestIsValid = employeeVacationBalance.AvailableHours >= workHoursDuringVacationRequest;

                    response.ValidationMessageCollection.Add($"Dostępne godziny: {employeeVacationBalance.AvailableHours} h, wniosek zużyłby: {workHoursDuringVacationRequest} h");
                    response.ValidationMessageCollection.AddRange(employeeCalendarResponse.SerializeCalendarInformation());
                }
                
                return response;
            }
        }
    }
}