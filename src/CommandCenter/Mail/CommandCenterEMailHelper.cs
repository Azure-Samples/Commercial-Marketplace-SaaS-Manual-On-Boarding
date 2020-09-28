using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandCenter.Marketplace;
using CommandCenter.Models;
using Microsoft.Extensions.Options;
using Microsoft.Marketplace;
using Microsoft.Marketplace.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CommandCenter.Mail
{
    public class CommandCenterEMailHelper : IMarketplaceNotificationHandler
    {
        private const string MailLinkControllerName = "MailLink";

        private readonly IMarketplaceClient marketplaceClient;

        private readonly CommandCenterOptions options;

        public CommandCenterEMailHelper(
            IOptionsMonitor<CommandCenterOptions> optionsMonitor,
            IMarketplaceClient marketplaceClient)
        {
            this.marketplaceClient = marketplaceClient;
            options = optionsMonitor.CurrentValue;
        }

        public async Task NotifyChangePlanAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            await SendWebhookNotificationEmailAsync(
                "Plan change request complete",
                $"Plan change request complete. Please take the required action.",
                string.Empty,
                notificationModel,
                cancellationToken);
        }

        public async Task ProcessNewSubscriptionAsyc(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(
                    "subscriptionId",
                    provisionModel.SubscriptionId.ToString()),
                new Tuple<string, string>("planId", provisionModel.PlanId)
            };

            var emailText =
                "<p>New subscription. Please take the required action, then return to this email and click the following link to confirm. ";
            emailText += $"{BuildALink("Activate", queryParams, "Click here to activate subscription")}.</p>";
            emailText +=
                $"<div> <p> Details are</p> <div> {BuildTable(JObject.Parse(JsonConvert.SerializeObject(provisionModel)))}</div></div>";

            await SendEmailAsync(
                () => $"New subscription, {provisionModel.SubscriptionName}",
                () => emailText,
                cancellationToken);
        }

        public async Task ProcessChangePlanAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(
                    "subscriptionId",
                    provisionModel.SubscriptionId.ToString()),
                new Tuple<string, string>("planId", provisionModel.NewPlanId)
            };

            var emailText = $"<p>Updated subscription from {provisionModel.PlanId} to {provisionModel.NewPlanId}.";

            emailText +=
                "Please take the required action, then return to this email and click the following link to confirm. ";
            emailText += $"{BuildALink("Update", queryParams, "Click here to update subscription")}.</p>";
            emailText +=
                $"<div> <p> Details are</p> <div> {BuildTable(JObject.Parse(JsonConvert.SerializeObject(provisionModel)))}</div></div>";

            await SendEmailAsync(
                () => $"Update subscription, {provisionModel.SubscriptionName}",
                () => emailText,
                cancellationToken);
        }

        public async Task ProcessChangeQuantityAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            await SendWebhookNotificationEmailAsync(
                "Quantity change request",
                "Quantity change request. Please take the required action.",
                string.Empty,
                notificationModel,
                cancellationToken);
        }

        public async Task ProcessOperationFailOrConflictAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(
                    "subscriptionId",
                    notificationModel.SubscriptionId.ToString())
            };

            var subscriptionDetails = await marketplaceClient.Fulfillment.GetSubscriptionAsync(
                notificationModel.SubscriptionId,
                Guid.Empty,
                Guid.Empty,
                cancellationToken);

            await SendEmailAsync(
                () => $"Operation failure, {subscriptionDetails.Name}",
                () =>
                    $"<p>Operation failure. {BuildALink("Operations", queryParams, "Click here to list all operations for this subscription", "Subscriptions")}</p>. "
                    + $"<p> Details are {BuildTable(JObject.Parse(JsonConvert.SerializeObject(subscriptionDetails)))}</p>",
                cancellationToken);
        }

        public async Task ProcessReinstatedAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            await SendWebhookNotificationEmailAsync(
                "Reinstate subscription request",
                "Reinstate subscription request. Please take the required action, then return to this email and click the following link to confirm.",
                "Reinstate",
                notificationModel,
                cancellationToken);
        }

        public async Task ProcessSuspendedAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            await SendWebhookNotificationEmailAsync(
                "Suspend subscription request",
                "Suspend subscription request. Please take the required action.",
                string.Empty,
                notificationModel,
                cancellationToken);
        }

        public async Task ProcessUnsubscribedAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default)
        {
            await SendWebhookNotificationEmailAsync(
                "Cancel subscription request",
                "Cancel subscription request. Please take the required action.",
                string.Empty,
                notificationModel,
                cancellationToken);
        }

        private string BuildALink(
            string controllerAction,
            IEnumerable<Tuple<string, string>> queryParams,
            string innerText,
            string controllerName = MailLinkControllerName)
        {
            var uriStart = FluentUriBuilder.Start(options.BaseUrl.Trim()).AddPath(controllerName)
                .AddPath(controllerAction);

            foreach (var (item1, item2) in queryParams) uriStart.AddQuery(item1, item2);

            var href = uriStart.Uri.ToString();

            return $"<a href=\"{href}\">{innerText}</a>";
        }

        private string BuildTable(JObject parsed)
        {
            var tableContents = parsed.Properties().AsEnumerable()
                .Select(p => $"<tr><th align=\"left\"> {p.Name} </th><th align=\"left\"> {p.Value}</th></tr>")
                .Aggregate((head, tail) => head + tail);
            return $"<table border=\"1\" align=\"left\">{tableContents}</table>";
        }

        private async Task SendEmailAsync(
            Func<string> subjectBuilder,
            Func<string> contentBuilder,
            CancellationToken cancellationToken = default)
        {
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(options.Mail.FromEmail, "Marketplace command center"));

            var recipients = new List<EmailAddress> { new EmailAddress(options.Mail.OperationsTeamEmail) };

            msg.AddTos(recipients);

            msg.SetSubject(subjectBuilder());

            msg.AddContent(MimeType.Html, contentBuilder());

            var client = new SendGridClient(options.Mail.ApiKey);
            var response = await client.SendEmailAsync(msg, cancellationToken);
        }

        private async Task SendWebhookNotificationEmailAsync(
            string subject,
            string mailBody,
            string actionName,
            NotificationModel notificationModel,
            CancellationToken cancellationToken)
        {
            var queryParams = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(
                    "subscriptionId",
                    notificationModel.SubscriptionId.ToString()),
                new Tuple<string, string>("publisherId", notificationModel.PublisherId),
                new Tuple<string, string>("offerId", notificationModel.OfferId),
                new Tuple<string, string>("planId", notificationModel.PlanId),
                new Tuple<string, string>("quantity", notificationModel.Quantity.ToString()),
                new Tuple<string, string>("operationId", notificationModel.OperationId.ToString())
            };

            var subscriptionDetails = await marketplaceClient.Fulfillment.GetSubscriptionAsync(
                notificationModel.SubscriptionId,
                Guid.Empty,
                Guid.Empty,
                cancellationToken);

            var actionLink = !string.IsNullOrEmpty(actionName)
                ? BuildALink(actionName, queryParams, "Click here to confirm.")
                : string.Empty;

            await SendEmailAsync(
                () => $"{subject}, {subscriptionDetails.Name}",
                () => $"<p>{mailBody}" + $"{actionLink}</p>"
                                       + $"<br/><div> Details are {BuildTable(JObject.Parse(JsonConvert.SerializeObject(subscriptionDetails)))}</div>",
                cancellationToken);
        }
    }
}