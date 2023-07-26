// Copyright 2022-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Models;
using Ellucian.Colleague.Dtos;
using Ellucian.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Ellucian.Colleague.Api.Helpers
{
    /// <summary>
    /// Extensions for <see cref="WebApiSettings"/>.
    /// </summary>
    public static class WebApiSettingsExtensions
    {
        /// <summary>
        /// Audit log configuration changes for <see cref="WebApiSettings"/>.
        /// </summary>
        /// <param name="settings">Current <see cref="WebApiSettings"/>.</param>
        /// <param name="oldSettings">Old <see cref="WebApiSettings"/>.</param>
        /// <param name="username">Current user name.</param>
        /// 
        public static void AuditLogConfigurationChanges(this WebApiSettings settings, WebApiSettings oldSettings, string username)
        {
            var auditLog = new AuditLoggingAdapter();

            using (Serilog.Context.LogContext.PushProperty("category", "Configuration"))
            using (Serilog.Context.LogContext.PushProperty("subCategory", "Update"))
            using (Serilog.Context.LogContext.PushProperty("action", "update"))
            using (Serilog.Context.LogContext.PushProperty("user", username))
            using (Serilog.Context.LogContext.PushProperty("status", "success"))
            {
                // DMI Settings
                if ((settings.AccountName ?? string.Empty) != (oldSettings.AccountName ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.AccountName)} changed from '{oldSettings.AccountName}' to '{settings.AccountName}'.");
                if ((settings.IpAddress ?? string.Empty) != (oldSettings.IpAddress ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.IpAddress)} changed from '{oldSettings.IpAddress}' to '{settings.IpAddress}'.");
                if (settings.Port != oldSettings.Port)
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.Port)} changed from '{oldSettings.Port}' to '{settings.Port}'.");
                if (settings.Secure != oldSettings.Secure)
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.Secure)} changed from '{oldSettings.Secure}' to '{settings.Secure}'.");
                if ((settings.HostNameOverride ?? string.Empty) != (oldSettings.HostNameOverride ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.HostNameOverride)} changed from '{oldSettings.HostNameOverride}' to '{settings.HostNameOverride}'.");
                if (settings.ConnectionPoolSize != oldSettings.ConnectionPoolSize)
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.ConnectionPoolSize)} changed from '{oldSettings.ConnectionPoolSize}' to '{settings.ConnectionPoolSize}'.");

                // DAS Settings
                if (settings.UseDasDatareader != oldSettings.UseDasDatareader)
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.UseDasDatareader)} changed from '{oldSettings.UseDasDatareader}' to '{settings.UseDasDatareader}'.");
                if ((settings.DasAccountName ?? string.Empty) != (oldSettings.DasAccountName ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasAccountName)} changed from '{oldSettings.DasAccountName}' to '{settings.DasAccountName}'.");
                if ((settings.DasIpAddress ?? string.Empty) != (oldSettings.DasIpAddress ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasIpAddress)} changed from '{oldSettings.DasIpAddress}' to '{settings.DasIpAddress}'.");
                if ((settings.DasPort ?? 0) != (oldSettings.DasPort ?? 0))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasPort)} changed from '{oldSettings.DasPort}' to '{settings.DasPort}'.");
                if (settings.DasSecure != oldSettings.DasSecure)
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasSecure)} changed from '{oldSettings.DasSecure}' to '{settings.DasSecure}'.");
                if ((settings.DasHostNameOverride ?? string.Empty) != (oldSettings.DasHostNameOverride ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasHostNameOverride)} changed from '{oldSettings.DasHostNameOverride}' to '{settings.DasHostNameOverride}'.");
                if (settings.DasConnectionPoolSize != oldSettings.DasConnectionPoolSize)
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasConnectionPoolSize)} changed from '{oldSettings.DasConnectionPoolSize}' to '{settings.DasConnectionPoolSize}'.");
                if ((settings.DasUsername ?? string.Empty) != (oldSettings.DasUsername ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasUsername)} changed.");
                if ((settings.DasPassword ?? string.Empty) != (oldSettings.DasPassword ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.DasPassword)} changed.");

                // Oauth Proxy
                if ((settings.OauthProxyUsername ?? string.Empty) != (oldSettings.OauthProxyUsername ?? string.Empty))
                    auditLog.Info($"Web API Admin Oauth Settings: Property {nameof(settings.OauthProxyUsername)} changed.");
                if ((settings.OauthProxyPassword ?? string.Empty) != (oldSettings.OauthProxyPassword ?? string.Empty))
                    auditLog.Info($"Web API Admin Oauth Settings: Property {nameof(settings.OauthProxyPassword)} changed.");
                if ((settings.OauthIssuerUrl ?? string.Empty) != (oldSettings.OauthIssuerUrl ?? string.Empty))
                    auditLog.Info($"Web API Admin Oauth Settings: Property {nameof(settings.OauthIssuerUrl)} changed.");

                // Shared Secret
                if ((settings.SharedSecret1 ?? string.Empty) != (oldSettings.SharedSecret1 ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.SharedSecret1)} changed.");
                if ((settings.SharedSecret2 ?? string.Empty) != (oldSettings.SharedSecret2 ?? string.Empty))
                    auditLog.Info($"Web API Admin Connection Settings: Property {nameof(settings.SharedSecret2)} changed.");

                // Logging
                if ((settings.LogLevel ?? string.Empty) != (oldSettings.LogLevel ?? string.Empty))
                    auditLog.Info($"Web API Admin Logging Settings: Property {nameof(settings.LogLevel)} changed from '{oldSettings.LogLevel}' to '{settings.LogLevel}'.");
            }
        }
    }
}