// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Marketplace.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Marketplace.SaaS;
    using Microsoft.Marketplace.SaaS.Models;
    using Microsoft.Rest;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
#pragma warning disable SA1600 // Elements should be documented
    public class FulfillmentTests
#pragma warning restore SA1600 // Elements should be documented
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        private MarketplaceSaaSClient sut;

        /// <summary>
        /// Initializes a new instance of the <see cref="FulfillmentTests"/> class.
        /// </summary>
        public FulfillmentTests()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets<FulfillmentTests>()
                .AddEnvironmentVariables()
                .Build();

            this.sut = new MarketplaceSaaSClient(Guid.Parse(config["TenantId"]), Guid.Parse(config["ClientId"]), config["clientSecret"]);
        }

        [TestMethod]
#pragma warning disable SA1600 // Elements should be documented
        public async Task GetAllSubscriptionsWithNext()
#pragma warning restore SA1600 // Elements should be documented
        {
            var subscriptions = new List<Subscription>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var page = await this.sut.FulfillmentOperations.ListSubscriptionsAsync();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            subscriptions.AddRange(page);

            while (page.NextPageLink != default)
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                page = await this.sut.FulfillmentOperations.ListSubscriptionsNextAsync(page.NextPageLink);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                subscriptions.AddRange(page);
            }

            Debug.Print($"Received {subscriptions.Count} subscriptions");
            Debug.Print($"Received {subscriptions.Select(s => s.Id).Distinct().ToList().Count} distinct subscriptions");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var allSubscriptions = await this.sut.FulfillmentOperations.ListAllSubscriptionsAsync();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            Assert.IsTrue(subscriptions.Any());
            Assert.AreEqual(allSubscriptions.Count(), subscriptions.Count);
        }

        [TestMethod]
#pragma warning disable SA1600 // Elements should be documented
        public async Task GetSubscription()
#pragma warning restore SA1600 // Elements should be documented
        {
            var subscriptions = this.sut.FulfillmentOperations.ListSubscriptions();
            Assert.IsTrue(subscriptions.Any());

            var subscription = subscriptions.First();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var result = await this.sut.FulfillmentOperations.GetSubscriptionAsync(subscription.Id.Value);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Ignore]
#pragma warning disable SA1600 // Elements should be documented
        public async Task ResolveSubscription()
#pragma warning restore SA1600 // Elements should be documented
        {
            // This needs to be run manually after receiving a marketplace token on the landing page, and adding here.
            // Don't forget to urldecode if you are copying from the url param
            var marketplaceToken = "LWW36TzssfHGvUNuDpSy9hVq9RZxrompe1bZT+qRRzqbwx83uxHyRVmYXjjqaYs550FQ07JAu27Suk5YqlnkAoBKt6makiced8hbTFpBS3WRda8Y0KoDTAl9fb0LEkFbpJ1MrcRglQrZLkrmc63ZlLcTQhrq/TNmvVcx4tOlq2f+RcvOdbzPrj7J3bScBNX/1Ug70NXrRMlENawNNORdTZ34eSBaJ9YnBiFcJ7oxcU4=";
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var resolvedSubscription = await this.sut.FulfillmentOperations.ResolveAsync(marketplaceToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            Debug.Print(resolvedSubscription.SubscriptionName);
            Assert.IsNotNull(resolvedSubscription);
        }

        [TestMethod]
#pragma warning disable SA1600 // Elements should be documented
        public async Task UpdateSubscription()
#pragma warning restore SA1600 // Elements should be documented
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var subscriptions = await this.sut.FulfillmentOperations.ListSubscriptionsAsync();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            Assert.IsTrue(subscriptions.Any());

            var firstActiveSubscription = subscriptions.First(s => s.SaasSubscriptionStatus.Value == SubscriptionStatusEnum.Subscribed);

            var plan = firstActiveSubscription.PlanId == "silver" ? "gold" : "silver";

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var result = await this.sut.FulfillmentOperations.UpdateSubscriptionAsync(firstActiveSubscription.Id.Value, null, null, planId: plan);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            // Cannot check whether this succeeed or not.
            var operationId = result.OperationId;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var operation = await this.sut.SubscriptionOperations.GetOperationStatusAsync(firstActiveSubscription.Id.Value, operationId);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            Assert.IsNotNull(operation);

            Debug.Print(Enum.GetName(typeof(OperationStatusEnum), operation.Status));
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
#pragma warning disable SA1600 // Elements should be documented
        public async Task ThrowsWhenBothPlanIdandQuantityPresent()
#pragma warning restore SA1600 // Elements should be documented
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var subscriptions = await this.sut.FulfillmentOperations.ListSubscriptionsAsync();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            Assert.IsTrue(subscriptions.Any());

            var firstActiveSubscription = subscriptions.First(s => s.SaasSubscriptionStatus.Value == SubscriptionStatusEnum.Subscribed);

            var plan = firstActiveSubscription.PlanId == "silver" ? "gold" : "silver";
            var quantity = 5;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var result = await this.sut.FulfillmentOperations.UpdateSubscriptionAsync(firstActiveSubscription.Id.Value, null, null, planId: plan, quantity);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            Assert.Fail("Should never come here");
        }
    }
}
