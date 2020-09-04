// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Group of Retention Alert Cases
    /// </summary>
    [Serializable]
    public class RetentionAlertGroupOfCases
    {
        /// <summary>
        /// Org Entity ID or Org Role ID of the owner of the group of Cases
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the Group of Retention Alert Cases
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Retention Alert Case Ids
        /// </summary>
        public List<string> CaseIds { get; set; }

        /// <summary>
        /// Number of Retention Alert Cases
        /// </summary>
        public int NumberOfCases { get { return CaseIds.Count;  } }
        
        public RetentionAlertGroupOfCases()
        {
            CaseIds = new List<string>();
        }
    }
}
