using System;
using System.Threading;
using System.Threading.Tasks;
using CommandCenter.Webhook;
using Microsoft.Extensions.Logging;
using Microsoft.Marketplace;
using Microsoft.Marketplace.Models;
using Newtonsoft.Json;

namespace CommandCenter.Marketplace
{
    /// <summary>
    /// Implements the protocol aspect of the fulfillment API
    /// </summary>
    public class MarketplaceProcessor : IMarketplaceProcessor
    {
        private readonly ILogger<MarketplaceProcessor> logger;
        private readonly IMarketplaceClient marketplaceClient;
        private readonly IWebhookHandler webhookHandler;

        public MarketplaceProcessor(IMarketplaceClient marketplaceClient,
            IWebhookHandler webhookHandler,
            ILogger<MarketplaceProcessor> logger)
        {
            this.logger = logger;
            this.marketplaceClient = marketplaceClient;
            this.webhookHandler = webhookHandler;
            this.marketplaceClient = marketplaceClient;
        }

        public async Task ActivateSubscriptionAsync(Guid subscriptionId, string planId, CancellationToken cancellationToken)
        {
            await this.marketplaceClient.Fulfillment.ActivateSubscriptionAsync(subscriptionId,
                null,
                null,
                planId,
                null,
                cancellationToken);
            
            this.logger.LogInformation($"Activated subscription {subscriptionId} with plan {planId}");
        }

        public async Task<ResolvedSubscription> GetSubscriptionFromPurchaseIdentificationTokenAsync(string token, CancellationToken cancellationToken)
        {
            if (token == default)
            {
                throw new ApplicationException("marketplace purchase identification token is empty");
            }

            var resolvedSubscription = await this.marketplaceClient.Fulfillment.ResolveAsync(token, null, null, cancellationToken);
        
            this.logger.LogInformation($"Resolved subscription {resolvedSubscription.Id} with plan {resolvedSubscription.PlanId}");
            
            return resolvedSubscription;
        }

        public async Task OperationAckAsync(Guid subscriptionId, Guid operationId, string planId, int quantity, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Ackonwledging operation {operationId} for subscription {subscriptionId}");

            await marketplaceClient.SubscriptionOperations.UpdateOperationStatusAsync(
                    subscriptionId,
                    operationId,
                    null,
                    null,
                    planId,
                    quantity,
                    UpdateOperationStatusEnum.Success,
                    cancellationToken);
        }

        public async Task ProcessWebhookNotificationAsync(WebhookPayload payload, CancellationToken cancellationToken)
        {
            // Always query the fulfillment API for the received Operation for security reasons. Webhook endpoint is not authenticated.
            var operationDetails = await marketplaceClient.SubscriptionOperations.GetOperationStatusAsync(
                payload.SubscriptionId,
                payload.OperationId,
                null,
                null,
                cancellationToken);

            if (operationDetails == null)
            {
                logger.LogError(
                    $"Operation query returned {JsonConvert.SerializeObject(operationDetails)} for subscription {payload.SubscriptionId} operation {payload.OperationId}");
                return;
            }

            this.logger.LogInformation($"Received webhook notification, operation {payload.OperationId}, for subscription {payload.SubscriptionId}, details are {JsonConvert.SerializeObject(payload)}");

            switch (payload.Action)
            {
                case WebhookAction.Unsubscribe:
                    await webhookHandler.UnsubscribedAsync(payload);
                    break;

                case WebhookAction.ChangePlan:
                    await webhookHandler.ChangePlanAsync(payload);
                    break;

                case WebhookAction.ChangeQuantity:
                    await webhookHandler.ChangeQuantityAsync(payload);
                    break;

                case WebhookAction.Suspend:
                    await webhookHandler.SuspendedAsync(payload);
                    break;

                case WebhookAction.Reinstate:
                    await webhookHandler.ReinstatedAsync(payload);
                    break;

                case WebhookAction.Transfer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
