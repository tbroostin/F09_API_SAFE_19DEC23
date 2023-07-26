﻿using Ellucian.Colleague.Api.Models;
using Ellucian.Logging;
using Ellucian.Web.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ellucian.Colleague.Api.Helpers
{
    /// <summary>
    /// Extensions for <see cref="ApiSettings"/>.
    /// </summary>
    public static class ApiSettingsExtensions
    {
        /// <summary>
        /// Audit log configuration changes for <see cref="ApiSettings"/>.
        /// </summary>
        /// <param name="settings">Current <see cref="ApiSettings"/>.</param>
        /// <param name="oldSettings">Old <see cref="ApiSettings"/>.</param>
        /// <param name="username">Current user name.</param>
        /// 
        public static void AuditLogConfigurationChanges(this ApiSettings settings, ApiSettings oldSettings, string username)
        {
            var auditLog = new AuditLoggingAdapter();

            using (Serilog.Context.LogContext.PushProperty("category", "Configuration"))
            using (Serilog.Context.LogContext.PushProperty("subCategory", "Update"))
            using (Serilog.Context.LogContext.PushProperty("action", "update"))
            using (Serilog.Context.LogContext.PushProperty("user", username))
            using (Serilog.Context.LogContext.PushProperty("status", "success"))
            {
                // Photo settings
                if ((settings.PhotoURL ?? string.Empty) != (oldSettings.PhotoURL ?? string.Empty))
                    auditLog.Info($"Web API Admin API Photo Settings: Property {nameof(settings.PhotoURL)} changed from '{oldSettings.PhotoURL}' to '{settings.PhotoURL}'.");
                if ((settings.PhotoType ?? string.Empty) != (oldSettings.PhotoType ?? string.Empty))
                    auditLog.Info($"Web API Admin API Photo Settings: Property {nameof(settings.PhotoType)} changed from '{oldSettings.PhotoType}' to '{settings.PhotoType}'.");
                if (settings.PhotoConfiguration != oldSettings.PhotoConfiguration)
                    auditLog.Info($"Web API Admin API Photo Settings: Property {nameof(settings.PhotoConfiguration)} changed from '{oldSettings.PhotoConfiguration}' to '{settings.PhotoConfiguration}'.");
                if (PhotoHeadersHaveChanged(settings.PhotoHeaders, oldSettings.PhotoHeaders))
                    auditLog.Info($"Web API Admin API Photo Settings: Property {nameof(settings.PhotoHeaders)} changed from [{FormatPhotoHeaders(oldSettings.PhotoHeaders)}] to [{FormatPhotoHeaders(settings.PhotoHeaders)}]'.");

                // Report settings
                if ((settings.ReportLogoPath ?? string.Empty) != (oldSettings.ReportLogoPath ?? string.Empty))
                    auditLog.Info($"Web API Admin API Repport Settings: Property {nameof(settings.ReportLogoPath)} changed from '{oldSettings.ReportLogoPath}' to '{settings.ReportLogoPath}'.");
                if ((settings.UnofficialWatermarkPath ?? string.Empty) != (oldSettings.UnofficialWatermarkPath ?? string.Empty))
                    auditLog.Info($"Web API Admin API Repport Settings: Property {nameof(settings.UnofficialWatermarkPath)} changed from '{oldSettings.UnofficialWatermarkPath}' to '{settings.UnofficialWatermarkPath}'.");
            }
        }

        private static bool PhotoHeadersHaveChanged(Dictionary<string, string> currentHeaders, Dictionary<string, string> oldHeaders)
        {
            if (currentHeaders.Count != oldHeaders.Count) return true;

            if (currentHeaders.Intersect(oldHeaders).Count() != currentHeaders.Count) return true;

            return false;
        }

        private static string FormatPhotoHeaders(Dictionary<string, string> headers)
        {
            var builder = new StringBuilder();

            foreach (var pair in headers)
            {
                if (builder.Length > 0) builder.Append(", ");

                builder.Append($"{{{pair.Key}: '{pair.Value}'}}");
            }

            return builder.ToString();
        }
    }
}