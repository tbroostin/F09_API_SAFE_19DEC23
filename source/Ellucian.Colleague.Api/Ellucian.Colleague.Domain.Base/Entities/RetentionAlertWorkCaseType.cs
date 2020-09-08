// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseType
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Case Type to add to the Case
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// Internal/Detailed Notes to add to the Case Item being recorded (added) to the the case
        /// </summary>
        public IEnumerable<string> Notes { get; set; }

        public RetentionAlertWorkCaseType(string updatedBy, string caseType, List<string> notes)
        {
            if (string.IsNullOrEmpty(caseType))
            {
                throw new ArgumentNullException("caseType");
            }
            if (notes == null)
            {
                notes = new List<string>();
            }
            if (!string.IsNullOrEmpty(updatedBy))
            {
                UpdatedBy = updatedBy;
            }

            CaseType = caseType;
            Notes = notes;
            
        }
    }
}
