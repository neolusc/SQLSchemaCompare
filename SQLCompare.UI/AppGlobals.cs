﻿using System;
using System.IO;
using SQLCompare.Core.Interfaces;
using SQLCompare.Services;

namespace SQLCompare.UI
{
    /// <summary>
    /// Global configuration of the application
    /// </summary>
    internal class AppGlobals : IAppGlobals
    {
#if DEBUG
        private const string MyAccountBaseEndpoint = "https://localhost:44349";
#else
        private const string MyAccountBaseEndpoint = "https://myaccount-ticodex.azurewebsites.net";
#endif

        /// <inheritdoc/>
        public string CompanyName => "TiCodeX SA";

        /// <inheritdoc/>
        public string ProductName => "SQL Compare";

        /// <inheritdoc/>
        public bool IsDevelopment
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc/>
        public string AuthorizationHeaderName => "CustomAuthToken";

        /// <inheritdoc/>
        public string AppSettingsFullFilename => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".SQLCompare",
            "SQLCompare.conf");

        /// <inheritdoc/>
        public string LoggerLayout =>
            "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}";

        /// <inheritdoc/>
        public string LoggerFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".SQLCompare",
            "log",
            @"SQLCompare-${shortdate}-service.log");

        /// <inheritdoc/>
        public int LoggerMaxArchiveFiles => 9;

        /// <inheritdoc/>
        public string ElectronAuthAppId => "queieimiugrepqueieimiucrap";

        /// <inheritdoc/>
        public string ProductCode => "SQLCMP";

        /// <inheritdoc/>
        public string MyAccountEndpoint => $"{MyAccountBaseEndpoint}/Login?culture={Localization.Culture.Name}&product={this.ProductCode}&returnUrl={Uri.EscapeDataString("/")}";

        /// <inheritdoc/>
        public string LoginEndpoint => $"{MyAccountBaseEndpoint}/Login?culture={Localization.Culture.Name}&product={this.ProductCode}&appId={this.ElectronAuthAppId}";

        /// <inheritdoc/>
        public string SubscribeEndpoint => $"{MyAccountBaseEndpoint}/Login?culture={Localization.Culture.Name}&product={this.ProductCode}&returnUrl={Uri.EscapeDataString("/Subscribe")}";

        /// <inheritdoc/>
        public string VerifySessionEndpoint => $"{MyAccountBaseEndpoint}/api/VerifySession";
    }
}
