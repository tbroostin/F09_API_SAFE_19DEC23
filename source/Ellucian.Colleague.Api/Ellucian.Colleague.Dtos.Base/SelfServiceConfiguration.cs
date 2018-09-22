// Copyright 2018 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Colleague Self-Service configuration information
    /// </summary>
    public class SelfServiceConfiguration
    {
        /// <summary>
        /// Flag indicating whether or not 'copy to clipboard' functionality should always be used on bulk mail-to operations
        /// </summary>
        public bool AlwaysUseClipboardForBulkMailToLinks { get; set; }
    }
}
