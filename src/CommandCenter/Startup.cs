// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter
{
    using System.Threading.Tasks;
    using CommandCenter.Authorization;
    using CommandCenter.AzureQueues;
    using CommandCenter.Mail;
    using CommandCenter.Marketplace;
    using CommandCenter.OperationsStore;
    using CommandCenter.Webhook;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
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

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.None, // if in debug mode
                MinimumSameSitePolicy = SameSiteMode.Unspecified,
            });
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
            // Enable JwtBerar auth for the webhook to validate the incoming token with the WebHookTokenParameters section, since this call will be
            // related with our AAD App regisration details on the partner center.
            services.AddMicrosoftIdentityWebApiAuthentication(this.configuration, "WebHookTokenParameters");

            // Now configure the custom token validation logic for WebHook
            services.Configure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    // Need to override the ValidAudience, since the incoming token has the app ID as the aud claim. 
                    // Library expects it to be api://<appId> format.
                                        options.TokenValidationParameters.ValidAudience = this.configuration["WebHookTokenParameters:ClientId"];
                    options.TokenValidationParameters.ValidIssuer = $"https://sts.windows.net/{this.configuration["WebHookTokenParameters:TenantId"]}/";
                });

            // Enable AAD sign on on the landing page.
            services.AddMicrosoftIdentityWebAppAuthentication(this.configuration, "AzureAd");
            services.Configure<OpenIdConnectOptions>(options =>
            {
                options.Events.OnSignedOutCallbackRedirect = (context) =>
                {
                    context.Response.Redirect("/Subscriptions/Index");
                    context.HandleResponse();

                    return Task.CompletedTask;
                };
            });

            services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.AccessDeniedPath = new PathString("/Subscriptions/NotAuthorized");
                });

            services.AddDistributedMemoryCache();

            services.Configure<CommandCenterOptions>(this.configuration.GetSection("CommandCenter"));

            services.TryAddScoped<IMarketplaceSaaSClient>(sp =>
            {
                var marketplaceClientOptions = new MarketplaceClientOptions();
                this.configuration.GetSection(MarketplaceClientOptions.MarketplaceClient).Bind(marketplaceClientOptions);
                return new MarketplaceSaaSClient(marketplaceClientOptions.TenantId, marketplaceClientOptions.ClientId, clientSecret: marketplaceClientOptions.ClientSecret);
            });

            services.TryAddScoped<IOperationsStore>(sp =>
                new AzureTableOperationsStore(this.configuration["CommandCenter:OperationsStoreConnectionString"]));

            // Hack to save the host name and port during the handling the request. Please see the WebhookController and ContosoWebhookHandler implementations
            services.AddSingleton<ContosoWebhookHandlerOptions>();

            services.TryAddScoped<IWebhookHandler, ContosoWebhookHandler>();
            services.TryAddScoped<IMarketplaceProcessor, MarketplaceProcessor>();

            var notificationHandler = this.configuration.GetSection("CommandCenter").Get<CommandCenterOptions>().ActiveNotificationHandler;

            if (notificationHandler == NotificationHandlerEnum.EmailNotifications)
            {
                // It is email in this sample, but you can plug in anything that implements the interface and communicate with an existing API.
                // In the email case, the existing API is the SendGrid API...
                services.TryAddScoped<IMarketplaceNotificationHandler, CommandCenterEMailHelper>();
            }
            else
            {
                services.TryAddScoped<IMarketplaceNotificationHandler, AzureQueueNotificationHandler>();
            }

            services.AddAuthorization(
                options => options.AddPolicy(
                    "CommandCenterAdmin",
                    policy =>
                    {
                        policy.AuthenticationSchemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(
                        new CommandCenterAdminRequirement(
                            this.configuration.GetSection("CommandCenter").Get<CommandCenterOptions>()
                                .CommandCenterAdmin));
                    }));

            services.AddSingleton<IAuthorizationHandler, CommandCenterAdminHandler>();

            services.AddControllersWithViews();

            services.AddRazorPages()
                 .AddMicrosoftIdentityUI();
        }
    }
}