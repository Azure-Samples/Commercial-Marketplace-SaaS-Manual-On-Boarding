// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Marketplace
{
    using System.Threading;
    using System.Threading.Tasks;
    using CommandCenter.Models;

    /// <summary>
    ///  Notification helper interface.
    /// </summary>
    public interface IMarketplaceNotificationHandler
    {
        /// <summary>
        /// Notify about change plan.
        /// </summary>
        /// <param name="notificationModel">Notification model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task NotifyChangePlanAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about new subscription.
        /// </summary>
        /// <param name="provisionModel">provisionModel model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessNewSubscriptionAsyc(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about new plan change.
        /// </summary>
        /// <param name="provisionModel">provisionModel model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessChangePlanAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about change quantity.
        /// </summary>
        /// <param name="notificationModel">Notification model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessChangeQuantityAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about conflict.
        /// </summary>
        /// <param name="notificationModel">Notification model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessOperationFailOrConflictAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about reinstate.
        /// </summary>
        /// <param name="notificationModel">Notification model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessReinstatedAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about suspend.
        /// </summary>
        /// <param name="notificationModel">Notification model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessSuspendedAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify about unsbscribe.
        /// </summary>
        /// <param name="notificationModel">Notification model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Void.</returns>
        Task ProcessUnsubscribedAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default);
    }
}