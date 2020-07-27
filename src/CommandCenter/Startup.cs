using System;
using System.Threading;
using CommandCenter.Authorization;
using CommandCenter.Mail;
using CommandCenter.Marketplace;
using CommandCenter.Webhook;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Marketplace;
using Serilog;

namespace CommandCenter
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-3.1
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => configuration.Bind("AzureAd", options))
                .AddJwtBearer(options =>
                {
                    // Webhook endpoint receives a JWT from the marketplace
                    // When you decode the JWT you will see the following important claims
                    //"aud": <app ID calling the marketplace API, this is on "MarketplaceClient:ClientId">,
                    //"iss": "https://sts.windows.net/<tenant ID of the App calling the marketplace API, this is on "MarketplaceClient:TenantUd">/",

                    // this needs to be the Tenant ID registered on the offer technical configuration page
                    var tenantId = configuration["MarketplaceClient:TenantId"];

                    // First grab the openIdConfig for the signing keys 
                    var stsDiscoveryEndpoint =
                        $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";
                    IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
                        new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint,
                            new OpenIdConnectConfigurationRetriever());
                    var openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter()
                        .GetResult();

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.FromMinutes(5),
                        ValidIssuer = $"https://sts.windows.net/{tenantId}/",
                        ValidAudience = configuration["MarketplaceClient:ClientId"], // this needs to be the App ID registered on the offer technical configuration page
                        IssuerSigningKeys = openIdConfig.SigningKeys
                    };
                })
                ;

            services.Configure<CookieAuthenticationOptions>(
                AzureADDefaults.CookieScheme,
                options => options.AccessDeniedPath = "/Subscriptions/NotAuthorized");

            services.Configure<OpenIdConnectOptions>(
                AzureADDefaults.OpenIdScheme,
                options =>
                {
                    options.Authority = options.Authority + "/v2.0/"; // Azure AD v2.0
                    options.TokenValidationParameters.ValidateIssuer =
                        false; // accept several tenants (here simplified)
                });

            services.Configure<CommandCenterOptions>(configuration.GetSection("CommandCenter"));

            services.TryAddScoped<IMarketplaceClient>(sp =>
            {
                var marketplaceClientOptions = new MarketplaceClientOptions();
                configuration.GetSection(MarketplaceClientOptions.MarketplaceClient).Bind(marketplaceClientOptions);
                return new MarketplaceClient(marketplaceClientOptions.TenantId, marketplaceClientOptions.ClientId,
                    marketplaceClientOptions.AppKey);
            });

            services.TryAddScoped<IOperationsStore>(sp =>
                new AzureTableOperationsStore(configuration["CommandCenter:OperationsStoreConnectionString"]));

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
                            configuration.GetSection("CommandCenter").Get<CommandCenterOptions>()
                                .CommandCenterAdmin))));

            services.AddSingleton<IAuthorizationHandler, CommandCenterAdminHandler>();

            services.AddControllers(options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }
            ).AddNewtonsoftJson();
        }
    }
}