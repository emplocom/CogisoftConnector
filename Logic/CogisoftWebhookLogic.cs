using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationWebhooks.RequestModels;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Logic
{
    public class CogisoftWebhookLogic
    {
        private readonly ILogger _logger;

        public CogisoftWebhookLogic(ILogger logger)
        {
            _logger = logger;
        }

        public string SendVacationCreatedRequest(VacationCreatedWebhookModel emploRequest)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                VacationCreatedRequestCogisoftModel cogisoftRequest = new VacationCreatedRequestCogisoftModel(
                    emploRequest.ExternalEmployeeId,
                    emploRequest.ExternalVacationTypeId,
                    emploRequest.Since.ToString(),
                    ((int) emploRequest.Duration).ToString()
                );

                var response =
                    client.PerformRequestReceiveResponse<VacationCreatedRequestCogisoftModel,
                        AsyncCommisionResponseCogisoftModel>(cogisoftRequest);

                return response.commision[0].ToString();
            }
        }

        public string SendVacationEditedRequest(VacationEditedWebhookModel emploRequest)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                VacationEditedRequestCogisoftModel cogisoftRequest = new VacationEditedRequestCogisoftModel(
                    emploRequest.ExternalVacationId,
                    emploRequest.ExternalVacationTypeId,
                    emploRequest.Since.ToString(),
                    ((int)emploRequest.Duration).ToString()
                );

                var response =
                    client.PerformRequestReceiveResponse<VacationEditedRequestCogisoftModel,
                        AsyncCommisionResponseCogisoftModel>(cogisoftRequest);

                return response.commision[0].ToString();
            }
        }

        public string SendVacationCancelledRequest(VacationStatusChangedWebhookModel emploRequest)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                VacationCancelledRequestCogisoftModel cogisoftRequest = new VacationCancelledRequestCogisoftModel(
                    emploRequest.ExternalVacationId
                );

                var response =
                    client.PerformRequestReceiveResponse<VacationCancelledRequestCogisoftModel,
                        AsyncCommisionResponseCogisoftModel>(cogisoftRequest);

                return response.commision[0].ToString();
            }
        }

        public AsyncProcessingResultResponseCogisoftModel CheckAsyncOperationState(string commisionIdentifier)
        {
            using (var client = new CogisoftServiceClient(_logger))
            {
                AsyncCommisionStatusRequestCogisoftModel cogisoftRequest = new AsyncCommisionStatusRequestCogisoftModel(
                    commisionIdentifier);

                var response =
                    client.PerformRequestReceiveResponse<AsyncCommisionStatusRequestCogisoftModel,
                        AsyncProcessingResultResponseCogisoftModel>(cogisoftRequest);

                return response;
            }
        }
    }
}
