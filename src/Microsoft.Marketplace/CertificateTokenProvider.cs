// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace Microsoft.Marketplace
{
    internal class CertificateTokenProvider : ITokenProvider
    {
        private readonly string authenticationEndpoint;
        private readonly string marketplaceResourceId;
        private readonly Guid appId;
        private readonly X509Certificate2 certificate;
        private readonly Guid tenantId;

        internal CertificateTokenProvider(Guid tenantId, Guid appId, X509Certificate2 certificate,
            string authenticationEndpoint, string marketplaceResourceId)
        {
            this.tenantId = tenantId;
            this.appId = appId;
            this.certificate = certificate;
            this.authenticationEndpoint = authenticationEndpoint;
            this.marketplaceResourceId = marketplaceResourceId;
        }


        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
        {
            var authContext = new AuthenticationContext(authenticationEndpoint + tenantId, false);
            var result = await authContext.AcquireTokenAsync(marketplaceResourceId,
                new ClientAssertionCertificate(appId.ToString(), certificate));
            return new AuthenticationHeaderValue("Bearer", result.AccessToken);
        }
    }
}