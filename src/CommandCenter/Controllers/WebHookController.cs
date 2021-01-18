// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Controllers
{
    using System;
    using System.IO;
    using System.Text;
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

    // Specify the auth scheme to be used for logging on users. This is for supporting WebAPI auth
    [RequireHttps]
    [Route("api/[controller]")]
    [ApiController]

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
        /// Webhook endpoint.
        /// </summary>
        /// <returns>Http OK.</returns>
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var payload = string.Empty;
            using (var reader = new StreamReader(this.Request.Body, Encoding.UTF8))
            {
                payload = await reader.ReadToEndAsync().ConfigureAwait(false);
                this.logger.LogInformation($"{payload}");
            }

            await this.marketplaceProcessor.ProcessWebhookNotificationAsync(JsonConvert.DeserializeObject<WebhookPayload>(payload), CancellationToken.None).ConfigureAwait(false);

            return this.Ok();
        }
    }
}