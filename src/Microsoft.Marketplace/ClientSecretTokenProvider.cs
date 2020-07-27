// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace Microsoft.Marketplace
{
    internal class ClientSecretTokenProvider : ITokenProvider
    {
        private readonly string authenticationEndpoint;
        private readonly string marketplaceResourceId;
        private readonly Guid appId;
        private readonly string clientSecret;
        private readonly Guid tenantId;

        internal ClientSecretTokenProvider(Guid tenantId, Guid appId, string clientSecret, string authenticationEndpoint,
            string marketplaceResourceId)
        {
            this.tenantId = tenantId;
            this.appId = appId;
            this.clientSecret = clientSecret;
            this.authenticationEndpoint = authenticationEndpoint;
            this.marketplaceResourceId = marketplaceResourceId;
        }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
        {
            var authContext = new AuthenticationContext(authenticationEndpoint + tenantId, false);
            var result = await authContext.AcquireTokenAsync(marketplaceResourceId,
                new ClientCredential(appId.ToString(), clientSecret));
            return new AuthenticationHeaderValue("Bearer", result.AccessToken);
        }
    }
}