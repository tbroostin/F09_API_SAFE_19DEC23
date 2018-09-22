// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Institution-defined Privacy options
    /// </summary>
    [Serializable]
    public class PrivacyConfiguration
    {
        /// <summary>
        /// Message to be displayed when Privacy Code values deny access
        /// </summary>
        public string RecordDenialMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivacyConfiguration"/> class.
        /// </summary>
        /// <param name="recordDenialMessage">Record Denial Message</param>
        public PrivacyConfiguration(string recordDenialMessage)
        {
            RecordDenialMessage = recordDenialMessage;
        }
    }
}
