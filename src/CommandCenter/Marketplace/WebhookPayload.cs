// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Marketplace
{
    using System;
    using Microsoft.Marketplace.SaaS.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Webhook payload.
    /// </summary>
    public class WebhookPayload
    {
        /// <summary>
        /// Gets or sets operation ID.
        /// </summary>
        [JsonProperty("id")]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Gets or sets activity ID.
        /// </summary>
        [JsonProperty("activityId")]
        public Guid ActivityId { get; set; }

        /// <summary>
        /// Gets or sets publisher ID.
        /// </summary>
        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        /// <summary>
        /// Gets or sets offer ID.
        /// </summary>
        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        /// <summary>
        /// Gets or sets plan ID.
        /// </summary>
        [JsonProperty("planId")]
        public string PlanId { get; set; }

        /// <summary>
        /// Gets or sets quantitiy.
        /// </summary>
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets subscription ID.
        /// </summary>
        [JsonProperty("subscriptionId")]
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets time stamp.
        /// </summary>
        [JsonProperty("timeStamp")]
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets action.
        /// </summary>
        [JsonProperty("action")]
        public WebhookAction Action { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        [JsonProperty("status")]
        public OperationStatusEnum Status { get; set; }
    }
}