// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Rest;

namespace Microsoft.Marketplace
{
    public partial class MarketplaceClient
    {
        private const string authenticationEndpoint = "https://login.microsoftonline.com/";

        // Please see https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/pc-saas-registration#get-a-token-based-on-the-azure-ad-app
        private const string marketplaceResourceId = "62d94f6c-d599-489b-a797-3e10e42fbe22";

        /// <summary>
        ///     Preferred constructor if the app is secured with a secret
        /// </summary>
        /// <param name="tenantId">
        ///     Tenant ID of the registered Azure Active Directory app
        /// </param>
        /// <param name="appId">
        ///     App ID (client ID) of the registered Azure Active Directory app
        /// </param>
        /// <param name="clientSecret">
        ///     Client secret/app secret of the registered Azure Active Directory app
        /// </param>
        /// <param name="handlers">
        ///     Custom DelegatingHandlers in case a custom action is required for the request and responses. Usually not a common
        ///     scenario.
        /// </param>
        public MarketplaceClient(Guid tenantId, Guid appId, string clientSecret, params DelegatingHandler[] handlers) :
            this(
                new TokenCredentials(new ClientSecretTokenProvider(tenantId, appId, clientSecret,
                    authenticationEndpoint, marketplaceResourceId)), handlers)
        {
        }

        /// <summary>
        ///     Preferred constructor if the app is secured with a secret
        /// </summary>
        /// <param name="tenantId">
        ///     Tenant ID of the registered Azure Active Directory app
        /// </param>
        /// <param name="appId">
        ///     App ID (client ID) of the registered Azure Active Directory app
        /// </param>
        /// <param name="certificate">
        ///     Client certificate of the registered Azure Active Directory app
        /// </param>
        /// <param name="handlers">
        ///     Custom DelegatingHandlers in case a custom action is required for the request and responses. Usually not a common
        ///     scenario.
        /// </param>
        public MarketplaceClient(Guid tenantId, Guid appId, X509Certificate2 certificate,
            params DelegatingHandler[] handlers) : this(
            new TokenCredentials(new CertificateTokenProvider(tenantId, appId, certificate, authenticationEndpoint,
                marketplaceResourceId)), handlers)
        {
        }
    }
}