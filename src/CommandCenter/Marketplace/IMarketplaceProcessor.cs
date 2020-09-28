// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Marketplace
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Marketplace.SaaS.Models;

    /// <summary>
    /// Marketplace processor interface.
    /// </summary>
    public interface IMarketplaceProcessor
    {
        /// <summary>
        /// Get subscription calling resolve.
        /// </summary>
        /// <param name="token">Marketplace identification token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Resolved subscription.</returns>
        Task<ResolvedSubscription> GetSubscriptionFromPurchaseIdentificationTokenAsync(string token, CancellationToken cancellationToken);

        /// <summary>
        /// Process webhook.
        /// </summary>
        /// <param name="payload">Webhook payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessWebhookNotificationAsync(WebhookPayload payload, CancellationToken cancellationToken);

        /// <summary>
        /// Activate subscription.
        /// </summary>
        /// <param name="subscriptionId">Subscription Id.</param>
        /// <param name="planId">Plan.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ActivateSubscriptionAsync(Guid subscriptionId, string planId, CancellationToken cancellationToken);

        /// <summary>
        /// Acknowledge operation.
        /// </summary>
        /// <param name="subscriptionId">Subscription Id.</param>
        /// <param name="operationId">Operation Id.</param>
        /// <param name="planId">PlanId.</param>
        /// <param name="quantity">Quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task OperationAckAsync(Guid subscriptionId, Guid operationId, string planId, int quantity, CancellationToken cancellationToken);
    }
}
