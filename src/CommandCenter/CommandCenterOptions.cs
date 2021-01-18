// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace CommandCenter
{
    using System;
    using CommandCenter.Mail;

    /// <summary>
    /// Options for the command center.
    /// </summary>
    public record CommandCenterOptions
    {
        /// <summary>
        /// Gets or sets base URL of the Command center.
        /// </summary>
        public Uri BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets command center admin.
        /// </summary>
        public string CommandCenterAdmin { get; set; }

        /// <summary>
        /// Gets or sets command center's email options.
        /// </summary>
        public MailOptions Mail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unsubcribed subscriptions are shown.
        /// </summary>
        public bool ShowUnsubscribed { get; set; }

        /// <summary>
        /// Gets or sets the connection string for the operations store.
        /// </summary>
        public string OperationsStoreConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Queue notifiction options.
        /// </summary>
        public AzureQueueOptions AzureQueue { get; set; }

        /// <summary>
        /// Gets or sets the active notification handler.
        /// </summary>
        public NotificationHandlerEnum ActiveNotificationHandler { get; set; }
    }
}