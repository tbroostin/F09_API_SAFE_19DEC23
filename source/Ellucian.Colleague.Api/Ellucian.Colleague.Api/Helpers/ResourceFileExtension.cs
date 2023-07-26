using Ellucian.Logging;
using Ellucian.Web.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ellucian.Colleague.Api.Helpers
{
    /// <summary>
    /// Extensions for <see cref="ResourceFile"/>.
    /// </summary>
    public static class ResourceFileExtension
    {
        /// <summary>
        /// Audit log configuration changes for a <see cref="ResourceFile"/>.
        /// </summary>
        /// <param name="resourceFile">Current <see cref="ResourceFile"/>.</param>
        /// <param name="oldResourceFile">Old <see cref="ResourceFile"/>.</param>
        /// <param name="username">Current user name.</param>
        /// 
        public static void AuditLogConfigurationChanges(this ResourceFile resourceFile, ResourceFile oldResourceFile, string username)
        {
            var auditLog = new AuditLoggingAdapter();
            using (Serilog.Context.LogContext.PushProperty("category", "Configuration"))
            using (Serilog.Context.LogContext.PushProperty("subCategory", "Update"))
            using (Serilog.Context.LogContext.PushProperty("action", "update"))
            using (Serilog.Context.LogContext.PushProperty("user", username))
            using (Serilog.Context.LogContext.PushProperty("status", "success"))
            {
                if (ResourceFileEntriesHeadersHaveChanged(resourceFile.ResourceFileEntries, oldResourceFile.ResourceFileEntries, out string fileEntries, out string oldFileEntries))
                {
                    auditLog.Info($"Web API Admin API Resource Editor File, {resourceFile.ResourceFileName}: Property {nameof(resourceFile.ResourceFileEntries)} changed from [{oldFileEntries}] to [{fileEntries}].");
                }
            }
        }

        private static bool ResourceFileEntriesHeadersHaveChanged(IEnumerable<ResourceFileEntry> currentEntries, IEnumerable<ResourceFileEntry> oldEntries, out string currentFileEntryValues, out string oldFileEntryValues)
        {
            oldFileEntryValues = null;
            currentFileEntryValues = null;
            var oldEntryBuilder = new StringBuilder();
            var currentEntryBuilder = new StringBuilder();

            foreach (var entry in oldEntries)
            {
                var updatedEntry = currentEntries.FirstOrDefault(x => x.Key.Equals(entry.Key));

                if (!(updatedEntry is null) && !updatedEntry.Value.Equals(entry.Value))
                {
                    if (oldEntryBuilder.Length > 0)
                    {
                        oldEntryBuilder.Append(", ");
                        currentEntryBuilder.Append(", ");
                    }

                    oldEntryBuilder.Append($"{{{entry.Key}: '{entry.Value}'}}");
                    currentEntryBuilder.Append($"{{{updatedEntry.Key}: '{updatedEntry.Value}'}}");
                }
            }

            oldFileEntryValues = oldEntryBuilder.ToString();
            currentFileEntryValues = currentEntryBuilder.ToString();

            return !string.IsNullOrEmpty(oldFileEntryValues);
        }
    }
}