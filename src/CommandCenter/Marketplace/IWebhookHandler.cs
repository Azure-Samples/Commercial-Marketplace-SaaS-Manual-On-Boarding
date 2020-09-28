// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Marketplace
{
    using System.Threading.Tasks;

    /// <summary>
    ///  Webhook handler interface.
    /// </summary>
    public interface IWebhookHandler
    {
        /// <summary>
        /// Called when there is a plan change.
        /// </summary>
        /// <param name="payload">Webhook call payload.</param>
        /// <returns>void.</returns>
        Task ChangePlanAsync(WebhookPayload payload);

        /// <summary>
        /// Called when there is a change in plan quantity.
        /// </summary>
        /// <param name="payload">Webhook payload.</param>
        /// <returns>void.</returns>
        Task ChangeQuantityAsync(WebhookPayload payload);

        /// <summary>
        /// Called when there is a reinstate notification.
        /// </summary>
        /// <param name="payload">Webhook payload.</param>
        /// <returns>void.</returns>
        Task ReinstatedAsync(WebhookPayload payload);

        /// <summary>
        /// Called when there is a suspended notification.
        /// </summary>
        /// <param name="payload">Webhook payload.</param>
        /// <returns>void.</returns>
        Task SuspendedAsync(WebhookPayload payload);

        /// <summary>
        /// Called when there is a unsubscribed notification.
        /// </summary>
        /// <param name="payload">Webhook payload.</param>
        /// <returns>void.</returns>
        Task UnsubscribedAsync(WebhookPayload payload);
    }
}