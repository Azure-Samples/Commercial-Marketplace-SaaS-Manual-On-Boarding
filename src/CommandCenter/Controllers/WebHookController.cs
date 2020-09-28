// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandCenter.Marketplace;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    /// <summary>
    /// Webhook controller.
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequireHttps]

    // [AllowAnonymous]
    public class WebHookController : Controller
    {
        private readonly IMarketplaceProcessor marketplaceProcessor;
        private readonly ILogger<WebHookController> logger;

        private readonly CommandCenterOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookController"/> class.
        /// </summary>
        /// <param name="optionsMonitor">Options.</param>
        /// <param name="marketplaceProcessor">Marketplace processor.</param>
        /// <param name="logger">Logger.</param>
        public WebHookController(
            IOptionsMonitor<CommandCenterOptions> optionsMonitor,
            IMarketplaceProcessor marketplaceProcessor,
            ILogger<WebHookController> logger)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            this.marketplaceProcessor = marketplaceProcessor;
            this.logger = logger;
            this.options = optionsMonitor.CurrentValue;
        }

        /// <summary>
        /// Webhook post.
        /// </summary>
        /// <param name="payload">Payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Action result.</returns>
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] WebhookPayload payload, CancellationToken cancellationToken)
        {
            // Options is injected as a singleton. This is not a good hack, but need to pass the host name and port
            this.logger.LogInformation($"Received webhook request: {JsonConvert.SerializeObject(payload)}");

            await this.marketplaceProcessor.ProcessWebhookNotificationAsync(payload, cancellationToken).ConfigureAwait(false);

            return this.Ok();
        }
    }
}