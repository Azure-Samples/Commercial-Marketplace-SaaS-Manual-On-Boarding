// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter.Marketplace
{
    /// <summary>
    /// Action on webhook.
    /// </summary>
    public enum WebhookAction
    {
        /// <summary>
        /// (When the resource has been deleted)
        /// </summary>
        Unsubscribe,

        /// <summary>
        /// (When the change plan operation has completed)
        /// </summary>
        ChangePlan,

        /// <summary>
        /// (When the change quantity operation has completed),
        /// </summary>
        ChangeQuantity,

        /// <summary>
        /// (When resource has been suspended)
        /// </summary>
        Suspend,

        /// <summary>
        /// (When resource has been reinstated after suspension)
        /// </summary>
        Reinstate,

        /// <summary>
        /// Transfer
        /// </summary>
        Transfer,
    }
}