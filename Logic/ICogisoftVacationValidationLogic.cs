using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;

namespace CogisoftConnector.Logic
{
    public interface ICogisoftVacationValidationLogic
    {
        VacationValidationResponseModel ValidateVacationRequest(VacationValidationRequestModel emploRequest);
    }
}
