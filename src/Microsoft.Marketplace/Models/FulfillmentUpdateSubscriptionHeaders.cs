// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.Marketplace.Models
{
    public partial class FulfillmentUpdateSubscriptionHeaders
    {
        [JsonIgnore]
        public Guid OperationLocation
        {
            get
            {
                // Operation-Location header returns a URI like the followign
                // https://marketplaceapi.microsoft.com/api/saas/subscriptions/4c49739f-217f-5d1b-5722-69278d4015c1/operations/749eb64f-4d7a-4f1d-9bcd-7629b97a2d38?api-version=2018-08-31

                if (!Uri.TryCreate(OperationLocationUri, UriKind.Absolute, out var uri) || uri.Segments.Length != 7)
                    throw new ApplicationException(
                        $"Operation location received {OperationLocationUri} does not have 7 segments.");

                if (Guid.TryParse(uri.Segments[6], out var operationId)) return operationId;

                throw new ApplicationException($"Cannot parse the operation ID {uri.Segments[6]} as a  Guid");
            }
        }
    }
}