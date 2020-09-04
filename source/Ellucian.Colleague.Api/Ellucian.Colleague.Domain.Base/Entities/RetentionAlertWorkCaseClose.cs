// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseClose
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Reason for closing the Case
        /// </summary>
        public string ClosureReason { get; set; }

        /// <summary>
        /// Summary to add to the Case Item being recorded (added) to the the case
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Internal/Detailed Notes to add to the Case Item being recorded (added) to the the case
        /// </summary>
        public IEnumerable<string> Notes { get; set; }

        public RetentionAlertWorkCaseClose(string updatedBy, string closureReason, string summary, List<string> notes) 
        {

            if (string.IsNullOrEmpty(closureReason))
            {
                throw new ArgumentNullException("closureReason");
            }
            if (string.IsNullOrEmpty(summary))
            {
                throw new ArgumentNullException("summary");
            }
            if (notes == null)
            {
                notes = new List<string>();
            }
            if (string.IsNullOrEmpty(updatedBy))
            {
                UpdatedBy = updatedBy;
            }

            ClosureReason = closureReason;
            Summary = summary;
            Notes = notes;
            
        }
    }
}
