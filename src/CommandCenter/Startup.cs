// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter
{
    using CommandCenter.Authorization;
    using CommandCenter.Mail;
    using CommandCenter.Marketplace;
    using CommandCenter.OperationsStore;
    using CommandCenter.Webhook;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Web;
    using Microsoft.Identity.Web.UI;
    using Microsoft.Marketplace.SaaS;
    using Serilog;

    /// <summary>
    /// ASP.NET core startup class.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">ASP.NET core configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="env">Web host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable following to capture the webhook request. Not enabled by default for not blocking the processing of webhook
            // app.UseMiddleware<WebhookRequestLogger>();
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Subscriptions}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMicrosoftIdentityWebAppAuthentication(this.configuration, "AzureAd")
            .EnableTokenAcquisitionToCallDownstreamApi(initialScopes: new string[] { "user.read" })
            .AddInMemoryTokenCaches();

            services.Configure<CommandCenterOptions>(this.configuration.GetSection("CommandCenter"));

            services.TryAddScoped<IMarketplaceSaaSClient>(sp =>
            {
                var marketplaceClientOptions = new MarketplaceClientOptions();
                this.configuration.GetSection(MarketplaceClientOptions.MarketplaceClient).Bind(marketplaceClientOptions);
                return new MarketplaceSaaSClient(marketplaceClientOptions.TenantId, marketplaceClientOptions.ClientId, clientSecret: marketplaceClientOptions.AppKey);
            });

            services.TryAddScoped<IOperationsStore>(sp =>
                new AzureTableOperationsStore(this.configuration["CommandCenter:OperationsStoreConnectionString"]));

            // Hack to save the host name and port during the handling the request. Please see the WebhookController and ContosoWebhookHandler implementations
            services.AddSingleton<ContosoWebhookHandlerOptions>();

            services.TryAddScoped<IWebhookHandler, ContosoWebhookHandler>();
            services.TryAddScoped<IMarketplaceProcessor, MarketplaceProcessor>();

            // It is email in this sample, but you can plug in anything that implements the interface and communicate with an existing API.
            // In the email case, the existing API is the SendGrid API...
            services.TryAddScoped<IMarketplaceNotificationHandler, CommandCenterEMailHelper>();

            services.AddAuthorization(
                options => options.AddPolicy(
                    "CommandCenterAdmin",
                    policy => policy.Requirements.Add(
                        new CommandCenterAdminRequirement(
                            this.configuration.GetSection("CommandCenter").Get<CommandCenterOptions>()
                                .CommandCenterAdmin))));

            services.AddSingleton<IAuthorizationHandler, CommandCenterAdminHandler>();

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            services.AddRazorPages();
        }
    }
}