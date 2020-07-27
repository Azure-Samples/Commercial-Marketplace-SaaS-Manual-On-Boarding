using System;
using System.Threading;
using System.Threading.Tasks;
using CommandCenter.Webhook;
using Microsoft.Marketplace.Models;

namespace CommandCenter.Marketplace
{
    public interface IMarketplaceProcessor
    {
        Task<ResolvedSubscription> GetSubscriptionFromPurchaseIdentificationTokenAsync(string token, CancellationToken cancellationToken);
        Task ProcessWebhookNotificationAsync(WebhookPayload payload, CancellationToken cancellationToken);
        Task ActivateSubscriptionAsync(Guid subscriptionId, string planId, CancellationToken cancellationToken);
        Task OperationAckAsync(Guid subscriptionId, Guid operationId, string planId, int quantity, CancellationToken cancellationToken);
    }
}
