using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using CogisoftConnector.Logic;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationValidation;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationWebhooks.RequestModels;
using EmploApiSDK.ApiModels.Vacations.IntegratedVacationWebhooks.ResponseModels;
using EmploApiSDK.Logger;
using Newtonsoft.Json;

namespace CogisoftConnector.Controllers
{
    /// <summary>
    /// Actions from this controller should be registered as webhook endpoints in emplo
    /// </summary>
    public class VacationRequestsApiController : ApiController
    {
        CogisoftWebhookLogic _cogisoftWebhookLogic;
        CogisoftSyncVacationDataLogic _cogisoftSyncVacationDataLogic;
        ICogisoftVacationValidationLogic _cogisoftVacationValidationLogic;
        ILogger _logger;

        public VacationRequestsApiController(CogisoftWebhookLogic cogisoftWebhookLogic,
            CogisoftSyncVacationDataLogic cogisoftSyncVacationData,
            ICogisoftVacationValidationLogic cogisoftVacationValidationLogic, ILogger logger)
        {
            _logger = LoggerFactory.CreateLogger(null);

            _cogisoftWebhookLogic = cogisoftWebhookLogic;
            _cogisoftSyncVacationDataLogic = cogisoftSyncVacationData;
            _cogisoftVacationValidationLogic = cogisoftVacationValidationLogic;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint listening for vacation creation event in emplo
        /// </summary>
        [HttpPost]
        public HttpResponseMessage VacationCreated([FromBody] VacationWebhookRequestModel model)
        {
            _logger.WriteLine($"Webhook received: VacationCreated, {JsonConvert.SerializeObject(model)}");

            try
            {
                var response =
                    new HttpResponseMessage(HttpStatusCode.Accepted);

                var commisionId = _cogisoftWebhookLogic.SendVacationCreatedRequest(model);

                var url = this.Url.Link("DefaultApi",
                    new
                    {
                        Controller = "VacationRequestsApi",
                        Action = "CheckAsyncOperationState",
                        commisionIdentifier = commisionId
                    });

                response.Content = new StringContent(string.Empty);
                response.Content.Headers.ContentLocation = new Uri(url);

                _logger.WriteLine($"Webhook VacationCreated response: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (Exception e)
            {
                throw new HttpResponseException(BuildErrorResponseFromException(e));
            }
        }

        /// <summary>
        /// Endpoint listening for vacation update event in emplo
        /// </summary>
        [HttpPost]
        public HttpResponseMessage VacationUpdated([FromBody] VacationWebhookRequestModel model)
        {
            _logger.WriteLine($"Webhook received: VacationUpdated, {JsonConvert.SerializeObject(model)}");

            try
            {
                if (model.Status == VacationStatusEnum.Canceled
                    || model.Status == VacationStatusEnum.Removed
                    || model.Status == VacationStatusEnum.Rejected)
                {
                    _logger.WriteLine($"Webhook VacationUpdated ignored: vacation with this status no longer exists in the Cogisoft system");
                    if (model.HasManagedVacationDaysBalance)
                    {
                        _cogisoftSyncVacationDataLogic.SyncVacationData(model.OperationTime, model.ExternalVacationTypeId, model.ExternalEmployeeId.AsList());
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    var response =
                        new HttpResponseMessage(HttpStatusCode.Accepted);

                    var commisionId = _cogisoftWebhookLogic.SendVacationEditedRequest(model);

                    var url = this.Url.Link("DefaultApi",
                        new
                        {
                            Controller = "VacationRequestsApi",
                            Action = "CheckAsyncOperationState",
                            commisionIdentifier = commisionId
                        });

                    response.Content = new StringContent(string.Empty);
                    response.Content.Headers.ContentLocation = new Uri(url);

                    _logger.WriteLine($"Webhook VacationUpdated response: {JsonConvert.SerializeObject(response)}");
                    return response;
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(BuildErrorResponseFromException(e));
            }
        }

        /// <summary>
        /// Endpoint listening for vacation status change event in emplo
        /// </summary>
        [HttpPost]
        public HttpResponseMessage VacationStatusChanged([FromBody] VacationWebhookRequestModel model)
        {
            _logger.WriteLine($"Webhook received: VacationStatusChanged, {JsonConvert.SerializeObject(model)}");

            try
            {
                if (model.Status == VacationStatusEnum.Canceled
                    || model.Status == VacationStatusEnum.Removed
                    || model.Status == VacationStatusEnum.Rejected)
                {
                    var response =
                        new HttpResponseMessage(HttpStatusCode.Accepted);

                    var commisionId = _cogisoftWebhookLogic.SendVacationCancelledRequest(model);

                    var url = this.Url.Link("DefaultApi",
                        new
                        {
                            Controller = "VacationRequestsApi",
                            Action = "CheckAsyncOperationState",
                            commisionIdentifier = commisionId
                        });

                    response.Content = new StringContent(string.Empty);
                    response.Content.Headers.ContentLocation = new Uri(url);

                    _logger.WriteLine($"Webhook VacationStatusChanged response: {JsonConvert.SerializeObject(response)}");
                    return response;
                }
                else
                {
                    _logger.WriteLine($"Webhook VacationStatusChanged ignored");
                    if (model.HasManagedVacationDaysBalance)
                    {
                        _cogisoftSyncVacationDataLogic.SyncVacationData(model.OperationTime, model.ExternalVacationTypeId, model.ExternalEmployeeId.AsList());
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(BuildErrorResponseFromException(e));
            }
        }

        /// <summary>
        /// Returns status of asynchronous request to Cogisoft API
        /// </summary>
        /// <param name="commisionIdentifier">Original request id</param>
        /// <param name="model">Vacation webhook request model</param>
        [HttpPost]
        public HttpResponseMessage CheckAsyncOperationState(string commisionIdentifier, [FromBody] VacationWebhookRequestModel model)
        {
            _logger.WriteLine($"Webhook status check, Commission Id: {commisionIdentifier}, Webhook event: {model.WebhookEvent}, External employee identifier: {model.ExternalEmployeeId}, External vacation type identifier {model.ExternalVacationTypeId}, HasManagedVacationDaysBalance: {model.HasManagedVacationDaysBalance}");

            try
            {
                var result = _cogisoftWebhookLogic.CheckAsyncOperationState(commisionIdentifier);

                if (!result.ci[0].processed)
                {
                    var response =
                        new HttpResponseMessage(HttpStatusCode.Accepted);

                    var url = this.Url.Link("DefaultApi",
                        new
                        {
                            Controller = "VacationRequestsApi",
                            Action = "CheckAsyncOperationState",
                            commisionIdentifier
                        });

                    response.Content = new StringContent(string.Empty);
                    response.Content.Headers.ContentLocation = new Uri(url);

                    _logger.WriteLine($"Status check result: STILL PROCESSING, response: {JsonConvert.SerializeObject(response)}");
                    return response;
                }
                else
                {
                    if (result.ci[0].code != 0)
                    {
                        throw new Exception("An error occurred during request processing by the external system, error code: " + result.ci[0].code);
                    }

                    if(model.HasManagedVacationDaysBalance)
                    {
                        _cogisoftSyncVacationDataLogic.SyncVacationData(model.OperationTime, model.ExternalVacationTypeId, model.ExternalEmployeeId.AsList());
                    }
                    
                    if (model.WebhookEvent == WebhookEvent.VacationCreated)
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.Created)
                        {
                            Content = new StringContent(
                                JsonConvert.SerializeObject(
                                    new CreatedObjectResponseEmploModel() {CreatedObjectIdentifier = result.ci[0].id}),
                                Encoding.UTF8, "application/json")
                        };

                        _logger.WriteLine($"Status check result: CREATED, response: {JsonConvert.SerializeObject(response)}");
                        return response;
                    }
                    else
                    {
                        _logger.WriteLine($"Status check result: OK");
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }
                }
            }
            catch (Exception e)
            {
                throw new HttpResponseException(BuildErrorResponseFromException(e));
            }
        }

        /// <summary>
        /// Endpoint listening for vacation validation request from emplo
        /// </summary>
        [HttpPost]
        public HttpResponseMessage ValidateVacationRequest([FromBody] IntegratedVacationValidationExternalRequest model)
        {
            _logger.WriteLine($"Request received: ValidateVacationRequest, {JsonConvert.SerializeObject(model)}");

            try
            {
                var validationResult = _cogisoftVacationValidationLogic.ValidateVacationRequest(model);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(validationResult),
                        Encoding.UTF8, "application/json")
                };

                _logger.WriteLine($"Vacation request validation response: {JsonConvert.SerializeObject(response)}");
                return response;
            }
            catch (Exception e)
            {
                throw new HttpResponseException(BuildErrorResponseFromException(e));
            }
        }

        /// <summary>
        /// Endpoint listening for vacation status change event in emplo
        /// </summary>
        [HttpPost]
        public HttpResponseMessage VacationWebhookErrorRecovery([FromBody] VacationWebhookErrorRecoveryModel model)
        {
            _logger.WriteLine($"Action received: VacationWebhookErrorRecovery, {JsonConvert.SerializeObject(model)}");

            try
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);

                if (model.ExternalVacationId != null && !model.ExternalVacationId.Equals(string.Empty))
                {
                    _logger.WriteLine($"Vacation Id {model.ExternalVacationId} received, performing synchronous cancellation...");
                    result = _cogisoftWebhookLogic.PerformSynchronousCancellation(model.ExternalVacationId);
                }

                if (model.HasManagedVacationDaysBalance)
                {
                    _cogisoftSyncVacationDataLogic.SyncVacationData(model.OperationTime, model.ExternalVacationTypeId, model.ExternalEmployeeId.AsList());
                }

                return result;
            }
            catch (Exception e)
            {
                throw new HttpResponseException(BuildErrorResponseFromException(e));
            }
        }

        [NonAction]
        private HttpResponseMessage BuildErrorResponseFromException(Exception e)
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new ErrorMessageResponseEmploModel() { ErrorMessage = ExceptionLoggingUtils.ExceptionAsString(e) }), Encoding.UTF8,
                    "application/json")
            };

            _logger.WriteLine($"Status check result: ERROR, response: {JsonConvert.SerializeObject(response)}", LogLevelEnum.Error);
            return response;
        }
    }
}
