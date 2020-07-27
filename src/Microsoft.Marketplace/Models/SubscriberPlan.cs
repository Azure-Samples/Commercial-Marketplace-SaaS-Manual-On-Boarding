// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Marketplace.Models
{
    public partial class SubscriberPlan
    {
        /// <summary>
        ///     Check to make sure either planId or quantity before calling the API
        /// </summary>
        /// <param name="context"></param>
        [OnSerializing]
        internal void CheckPayload(StreamingContext context)
        {
            if (Quantity.HasValue && !string.IsNullOrEmpty(PlanId))
                throw new ApplicationException(
                    "Cannot set planId and quantity at the same time. Please use quantity only if the pricing plan is per user and use planId only if the pricing model is flat rate.");
        }
    }
}