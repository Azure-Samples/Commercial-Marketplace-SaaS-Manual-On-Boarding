using System.Threading;
using System.Threading.Tasks;
using CommandCenter.Marketplace;
using CommandCenter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Marketplace;
using Microsoft.Marketplace.Models;

namespace CommandCenter.Controllers
{
    [Authorize("CommandCenterAdmin")]
    public class MailLinkController : Controller
    {
        private readonly IMarketplaceProcessor marketplaceProcessor;
        private readonly IMarketplaceClient marketplaceClient;

        public MailLinkController(IMarketplaceProcessor marketplaceProcessor, IMarketplaceClient marketplaceClient)
        {
            this.marketplaceProcessor = marketplaceProcessor;
            this.marketplaceClient = marketplaceClient;
        }

        [HttpGet]
        public async Task<IActionResult> Activate(
            NotificationModel notificationModel,
            CancellationToken cancellationToken)
        {
            await this.marketplaceProcessor.ActivateSubscriptionAsync(notificationModel.SubscriptionId, notificationModel.PlanId, cancellationToken);

            return View(
                new ActivateActionViewModel
                {
                    SubscriptionId = notificationModel.SubscriptionId, PlanId = notificationModel.PlanId
                });
        }

        [HttpGet]
        public async Task<IActionResult> QuantityChange(
            NotificationModel notificationModel,
            CancellationToken cancellationToken)
        {
            await OperationAckAsync(notificationModel, cancellationToken);

            return View("OperationUpdate", notificationModel);
        }

        [HttpGet]
        public async Task<IActionResult> Reinstate(
            NotificationModel notificationModel,
            CancellationToken cancellationToken)
        {
            await OperationAckAsync(notificationModel, cancellationToken);

            return View("OperationUpdate", notificationModel);
        }

        [HttpGet]
        public async Task<IActionResult> SuspendSubscription(
            NotificationModel notificationModel,
            CancellationToken cancellationToken)
        {
            await OperationAckAsync(notificationModel, cancellationToken);

            return View("OperationUpdate", notificationModel);
        }

        [HttpGet]
        public async Task<IActionResult> Unsubscribe(
            NotificationModel notificationModel,
            CancellationToken cancellationToken)
        {
            await OperationAckAsync(notificationModel, cancellationToken);

            return View("OperationUpdate", notificationModel);
        }

        [HttpGet]
        public async Task<IActionResult> Update(NotificationModel notificationModel)
        {
            var result = await marketplaceClient.Fulfillment.UpdateSubscriptionAsync(
                notificationModel.SubscriptionId,
                null,
                null,
                notificationModel.PlanId,
                null,
                CancellationToken.None);

            return View(
                new ActivateActionViewModel
                {
                    SubscriptionId = notificationModel.SubscriptionId, PlanId = notificationModel.PlanId
                });
        }

        private async Task OperationAckAsync(
            NotificationModel payload,
            CancellationToken cancellationToken)
        {
            await this.marketplaceProcessor.OperationAckAsync(payload.SubscriptionId, payload.OperationId, payload.PlanId, payload.Quantity, cancellationToken);
        }
    }
}