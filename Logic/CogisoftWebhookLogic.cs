using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
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

        public HttpResponseMessage PerformSynchronousCancellation(string vacationRequestId)
        {
            bool mockMode;
            if (bool.TryParse(ConfigurationManager.AppSettings["MockMode"], out mockMode) && mockMode)
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            using (var client = new CogisoftServiceClient(_logger))
            {
                GetVacationRequestByIdCogisoftModel checkIfVacationExistsRequest = new GetVacationRequestByIdCogisoftModel(
                    vacationRequestId
                );

                var checkIfVacationExistsResponse =
                    client.PerformRequestReceiveResponse<GetVacationRequestByIdCogisoftModel,
                        GetVacationRequestByIdResponseCogisoftModel>(checkIfVacationExistsRequest);

                if (checkIfVacationExistsResponse.VacationRequestExists())
                {
                    VacationCancelledRequestCogisoftModel cogisoftRequest = new VacationCancelledRequestCogisoftModel(
                        vacationRequestId
                    );

                    var response =
                        client.PerformRequestReceiveResponse<VacationCancelledRequestCogisoftModel,
                            AsyncCommisionResponseCogisoftModel>(cogisoftRequest);

                    var commisionIdentifier = response.commision[0].ToString();


                    AsyncProcessingResultResponseCogisoftModel asyncResponse = null;

                    AsyncCommisionStatusRequestCogisoftModel asyncCogisoftRequest =
                        new AsyncCommisionStatusRequestCogisoftModel(
                            commisionIdentifier);

                    asyncResponse =
                        client.PerformRequestReceiveResponse<AsyncCommisionStatusRequestCogisoftModel,
                            AsyncProcessingResultResponseCogisoftModel>(asyncCogisoftRequest);

                    int retryCounter = 6;
                    while (!asyncResponse.ci[0].processed && retryCounter-- > 0)
                    {
                        Thread.Sleep(5000);

                        asyncResponse =
                            client.PerformRequestReceiveResponse<AsyncCommisionStatusRequestCogisoftModel,
                                AsyncProcessingResultResponseCogisoftModel>(asyncCogisoftRequest);
                    };

                    if (!asyncResponse.ci[0].processed)
                    {
                        throw new Exception("The operation didn't finish in time");
                    }
                    else if (asyncResponse.ci[0].code != 0)
                    {
                        throw new Exception("An error occurred during request processing by the external system, error code: " + asyncResponse.ci[0].code);
                    }
                    else
                    {
                        checkIfVacationExistsResponse =
                            client.PerformRequestReceiveResponse<GetVacationRequestByIdCogisoftModel,
                                GetVacationRequestByIdResponseCogisoftModel>(checkIfVacationExistsRequest);

                        if (!checkIfVacationExistsResponse.VacationRequestExists())
                        {
                            _logger.WriteLine($"Request deletion successful");
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            throw new Exception("Request deletion failed");
                        }
                    }
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
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
