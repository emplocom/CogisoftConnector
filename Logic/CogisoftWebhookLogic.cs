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
    public class CogisoftWebhookLogic : IWebhookLogic
    {
        private readonly ILogger _logger;

        public CogisoftWebhookLogic(ILogger logger)
        {
            _logger = logger;
        }

        public string SendVacationCreatedRequest(VacationWebhookRequestModel emploRequest)
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

        public string SendVacationEditedRequest(VacationWebhookRequestModel emploRequest)
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

        public string SendVacationCancelledRequest(VacationWebhookRequestModel emploRequest)
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
                    _logger.WriteLine($"Vacation request {vacationRequestId} found in Cogisoft system, performing deletion...");
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

                    int retryCounter = 0;
                    _logger.WriteLine($"Waiting for deletion operation to finish, retry counter: {retryCounter}...");
                    asyncResponse =
                        client.PerformRequestReceiveResponse<AsyncCommisionStatusRequestCogisoftModel,
                            AsyncProcessingResultResponseCogisoftModel>(asyncCogisoftRequest);

                    while (!asyncResponse.ci[0].processed && retryCounter++ < 6)
                    {
                        Thread.Sleep(5000);

                        _logger.WriteLine($"Waiting for deletion operation to finish, retry counter: {retryCounter}...");

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
                        _logger.WriteLine($"Checking if request {vacationRequestId} has been successfully deleted...");
                        checkIfVacationExistsResponse =
                            client.PerformRequestReceiveResponse<GetVacationRequestByIdCogisoftModel,
                                GetVacationRequestByIdResponseCogisoftModel>(checkIfVacationExistsRequest);

                        if (!checkIfVacationExistsResponse.VacationRequestExists())
                        {
                            _logger.WriteLine($"Request {vacationRequestId} deletion successful");
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            throw new Exception($"Request {vacationRequestId} deletion failed");
                        }
                    }
                }
                else
                {
                    _logger.WriteLine($"Vacation request with Id {vacationRequestId} does not exist - returning OK status.");
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
