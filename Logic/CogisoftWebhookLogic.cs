using System.Collections.Generic;
using System.Threading.Tasks;
using CogisoftConnector.Models;
using CogisoftConnector.Models.WebhookModels.CogisoftRequestModels;
using CogisoftConnector.Models.WebhookModels.CogisoftResponseModels;
using CogisoftConnector.Models.WebhookModels.EmploRequestModels;
using EmploApiSDK.Logger;
using Newtonsoft.Json;

namespace CogisoftConnector.Logic
{
    public class CogisoftWebhookLogic
    {
        private ILogger _logger;

        public CogisoftWebhookLogic(ILogger logger)
        {
            _logger = logger;
        }

        public string SendVacationCreatedRequest(VacationCreatedWebhookModel emploRequest)
        {
            using (var client = new CogisoftServiceClient())
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
            using (var client = new CogisoftServiceClient())
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
            using (var client = new CogisoftServiceClient())
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
            using (var client = new CogisoftServiceClient())
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
