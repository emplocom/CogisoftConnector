using System.Net.Http;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationWebhooks.RequestModels;

namespace CogisoftConnector.Logic
{
    public interface IWebhookLogic
    {
        string SendVacationCreatedRequest(VacationWebhookRequestModel emploRequest);

        string SendVacationEditedRequest(VacationWebhookRequestModel emploRequest);

        string SendVacationCancelledRequest(VacationWebhookRequestModel emploRequest);

        HttpResponseMessage PerformSynchronousCancellation(string vacationRequestId);

        AsyncProcessingResultResponseCogisoftModel CheckAsyncOperationState(string commisionIdentifier);
    }
}
