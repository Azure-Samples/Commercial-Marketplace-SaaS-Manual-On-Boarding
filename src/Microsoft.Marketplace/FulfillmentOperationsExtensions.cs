// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Marketplace.Models;

namespace Microsoft.Marketplace
{
    public static partial class FulfillmentOperationsExtensions
    {
        /// <summary>
        ///     List all of the subscriptions by iteratively calling the List on subsequent pages
        /// </summary>
        /// <param name="fulfillment">
        ///     Type being extended
        /// </param>
        /// <param name="cancellationToken">
        ///     Cancellation token
        /// </param>
        /// <returns></returns>
        public static async Task<IEnumerable<Subscription>> ListAllSubscriptionsAsync(
            this IFulfillmentOperations fulfillment, CancellationToken cancellationToken = default)
        {
            var subscriptions = new List<Subscription>();
            var page = await fulfillment.ListSubscriptionsAsync(null, null, null, cancellationToken);

            subscriptions.AddRange(page);
            while (page.NextPageLink != default)
            {
                page = await fulfillment.ListSubscriptionsNextAsync(page.NextPageLink, null, null, cancellationToken);
                subscriptions.AddRange(page);
            }

            return subscriptions;
        }
    }
}