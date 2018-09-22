// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Criteria for looking up backup config data
    /// </summary>
    public class BackupConfigurationQueryCriteria
    {
        /// <summary>
        /// List of backup configuration records to retrieve.
        /// </summary>
        public IEnumerable<string> ConfigurationIds { get; set; }
        
        /// <summary>
        /// Namespace of the backup configuration records to retrieve
        /// </summary>
        public string Namespace { get; set; }
        
    }
}
