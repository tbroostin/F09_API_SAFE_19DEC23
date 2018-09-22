// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Colleague Self-Service configuration information
    /// </summary>
    [Serializable]
    public class SelfServiceConfiguration
    {
        /// <summary>
        /// Flag indicating whether or not 'copy to clipboard' functionality should always be used on bulk mail-to operations
        /// </summary>
        private bool _alwaysUseClipboardForBulkMailToLinks;
        public bool AlwaysUseClipboardForBulkMailToLinks { get { return _alwaysUseClipboardForBulkMailToLinks; } }

        /// <summary>
        /// Creates a new <see cref="SelfServiceConfiguration"/> object.
        /// </summary>
        /// <param name="alwaysCopy">Flag indicating whether or not 'copy to clipboard' functionality should always be used on bulk mail-to operations</param>
        public SelfServiceConfiguration(bool alwaysCopy)
        {
            _alwaysUseClipboardForBulkMailToLinks = alwaysCopy;
        }
    }
}
