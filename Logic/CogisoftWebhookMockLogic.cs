using System;
using System.Net;
using System.Net.Http;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationWebhooks.RequestModels;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Logic
{
    public class CogisoftWebhookMockLogic : IWebhookLogic
    {
        private readonly ILogger _logger;

        public CogisoftWebhookMockLogic(ILogger logger)
        {
            _logger = logger;
        }

        public string SendVacationCreatedRequest(VacationWebhookRequestModel emploRequest)
        {
            return Guid.NewGuid().ToString();
        }

        public string SendVacationEditedRequest(VacationWebhookRequestModel emploRequest)
        {
            return Guid.NewGuid().ToString();
        }

        public string SendVacationCancelledRequest(VacationWebhookRequestModel emploRequest)
        {
            return Guid.NewGuid().ToString();
        }

        public HttpResponseMessage PerformSynchronousCancellation(string vacationRequestId)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public AsyncProcessingResultResponseCogisoftModel CheckAsyncOperationState(string commisionIdentifier)
        {
            return new AsyncProcessingResultResponseCogisoftModel()
            {
                ci = new AsyncProcessingResultResponseCogisoftModel.Ci[] {new AsyncProcessingResultResponseCogisoftModel.Ci()
                {
                    code = 0,
                    id = commisionIdentifier,
                    processed = true
                } }
            };
        }
    }
}
