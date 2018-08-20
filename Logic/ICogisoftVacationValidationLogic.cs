using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;

namespace CogisoftConnector.Logic
{
    public interface ICogisoftVacationValidationLogic
    {
        IntegratedVacationValidationResponse ValidateVacationRequest(IntegratedVacationValidationExternalRequest emploExternalRequest);
    }
}
