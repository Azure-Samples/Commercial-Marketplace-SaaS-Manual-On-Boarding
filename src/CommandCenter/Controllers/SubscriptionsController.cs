// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandCenter.Models;
    using CommandCenter.OperationsStore;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Marketplace.SaaS;
    using Microsoft.Marketplace.SaaS.Models;

    /// <summary>
    /// Subscriptions panel.
    /// </summary>
    [Authorize("CommandCenterAdmin")]
    public class SubscriptionsController : Controller
    {
        private readonly IMarketplaceSaaSClient marketplaceClient;

        private readonly IOperationsStore operationsStore;

        private readonly CommandCenterOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionsController"/> class.
        /// </summary>
        /// <param name="marketplaceClient">Marketplace API client.</param>
        /// <param name="operationsStore">Operations store.</param>
        /// <param name="options">OSOlution options.</param>
        public SubscriptionsController(
            IMarketplaceSaaSClient marketplaceClient,
            IOperationsStore operationsStore,
            IOptionsMonitor<CommandCenterOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.marketplaceClient = marketplaceClient;
            this.operationsStore = operationsStore;
            this.options = options.CurrentValue;
        }

        /// <summary>
        /// Error page.
        /// </summary>
        /// <returns>Action result.</returns>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier,
                });
        }

        /// <summary>
        /// Subscriptions dashboard home.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Action result.</returns>
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var subscriptions = await this.marketplaceClient.FulfillmentOperations.ListAllSubscriptionsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var subscriptionsViewModel = subscriptions.Select(SubscriptionViewModel.FromSubscription)
                .Where(s => s.State != SubscriptionStatusEnum.Unsubscribed || this.options.ShowUnsubscribed);

            var newViewModel = new List<SubscriptionViewModel>();

            foreach (var subscription in subscriptionsViewModel)
            {
                var recordedSubscriptionOperations =
                    await this.operationsStore.GetAllSubscriptionRecordsAsync(
                        subscription.SubscriptionId,
                        cancellationToken).ConfigureAwait(false);

                subscription.ExistingOperations = (await this.operationsStore.GetAllSubscriptionRecordsAsync(
                    subscription.SubscriptionId,
                    cancellationToken).ConfigureAwait(false)).Any();
                subscription.OperationCount = recordedSubscriptionOperations.Count();
                newViewModel.Add(subscription);
            }

            return this.View(newViewModel.OrderByDescending(s => s.SubscriptionName));
        }

        /// <summary>
        /// Not authorized.
        /// </summary>
        /// <returns>Action result.</returns>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotAuthorized()
        {
            return this.View();
        }

        /// <summary>
        /// Return subscription operations.
        /// </summary>
        /// <param name="subscriptionId">Subcription.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Action result.</returns>
        public async Task<IActionResult> Operations(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscriptionOperations =
                await this.operationsStore.GetAllSubscriptionRecordsAsync(subscriptionId, cancellationToken).ConfigureAwait(false);

            var subscription =
                await this.marketplaceClient.FulfillmentOperations.GetSubscriptionAsync(subscriptionId, null, null, cancellationToken).ConfigureAwait(false);

            var operations = new List<Operation>();

            foreach (var operation in subscriptionOperations)
            {
                operations.Add(
                    await this.marketplaceClient.SubscriptionOperations.GetOperationStatusAsync(
                        subscriptionId,
                        operation.OperationId,
                        null,
                        null,
                        cancellationToken).ConfigureAwait(false));
            }

            return this.View(new OperationsViewModel
            {
                SubscriptionName = subscription.Name,
                Operations = operations,
            });
        }

        /// <summary>
        /// Actions on a subscription.
        /// </summary>
        /// <param name="subscriptionId">Subscription ID.</param>
        /// <param name="subscriptionAction">Action to take.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IActionResult> SubscriptionAction(
            Guid subscriptionId,
            ActionsEnum subscriptionAction,
            CancellationToken cancellationToken)
        {
            switch (subscriptionAction)
            {
                case ActionsEnum.Activate:
                    break;

                case ActionsEnum.Update:
                    var availablePlans = await this.marketplaceClient.FulfillmentOperations.ListAvailablePlansAsync(
                        subscriptionId,
                        null,
                        null,
                        cancellationToken).ConfigureAwait(false);

                    var subscription = await this.marketplaceClient.FulfillmentOperations.GetSubscriptionAsync(
                        subscriptionId,
                        null,
                        null,
                        cancellationToken).ConfigureAwait(false);

                    var pendingOperations = await this.marketplaceClient.SubscriptionOperations.ListOperationsAsync(
                        subscriptionId,
                        null,
                        null,
                        cancellationToken).ConfigureAwait(false);

                    var updateSubscriptionViewModel = new UpdateSubscriptionViewModel
                    {
                        SubscriptionId = subscriptionId,
                        SubscriptionName = subscription.Name,
                        CurrentPlan = subscription.PlanId,
                        AvailablePlans = availablePlans.Plans,
                        PendingOperations = pendingOperations.Operations.Any(
                            o => o.Status == OperationStatusEnum.InProgress),
                    };

                    return this.View("UpdateSubscription", updateSubscriptionViewModel);

                case ActionsEnum.Ack:
                    break;

                case ActionsEnum.Unsubscribe:
                    await this.marketplaceClient.FulfillmentOperations.DeleteSubscriptionAsync(
                        subscriptionId,
                        null,
                        null,
                        cancellationToken).ConfigureAwait(false);

                    return this.RedirectToAction("Index");

                default:
                    throw new ArgumentOutOfRangeException(nameof(subscriptionAction), subscriptionAction, null);
            }

            return this.View();
        }

        /// <summary>
        /// Update subscription action.
        /// </summary>
        /// <param name="model">Subscription details.</param>
        /// <param name="cancellationToken">Cancellationtoken.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateSubscription(
            UpdateSubscriptionViewModel model,
            CancellationToken cancellationToken)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var pendingOperations = await this.marketplaceClient.SubscriptionOperations.ListOperationsAsync(
                model.SubscriptionId,
                null,
                null,
                cancellationToken).ConfigureAwait(false);

            if (pendingOperations.Operations.Any(o => o.Status == OperationStatusEnum.InProgress))
            {
                return this.RedirectToAction("Index");
            }

            var updateResult = await this.marketplaceClient.FulfillmentOperations.UpdateSubscriptionAsync(
                model.SubscriptionId,
                null,
                null,
                model.NewPlan,
                null,
                cancellationToken).ConfigureAwait(false);

            await this.operationsStore.RecordAsync(model.SubscriptionId, updateResult.OperationId, cancellationToken).ConfigureAwait(false);

            return this.RedirectToAction("Index");
        }
    }
}