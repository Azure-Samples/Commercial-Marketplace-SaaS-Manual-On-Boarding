using System;
using Microsoft.Marketplace.Models;
using Newtonsoft.Json;

namespace CommandCenter.Marketplace
{
    public class WebhookPayload
    {
        [JsonProperty("id")]
        public Guid OperationId { get; set; }

        [JsonProperty("activityId")]
        public Guid ActivityId { get; set; }

        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("planId")]
        public string PlanId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("subscriptionId")]
        public Guid SubscriptionId { get; set; }

        [JsonProperty("timeStamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [JsonProperty("action")]
        public WebhookAction Action { get; set; }

        [JsonProperty("status")]
        public OperationStatusEnum Status { get; set; }

        [JsonProperty("operationRequestSource")]
        public string OperationRequestSource { get; set; }

        [JsonProperty("subscription")]
        public object Subscription { get; set; }
    }
}