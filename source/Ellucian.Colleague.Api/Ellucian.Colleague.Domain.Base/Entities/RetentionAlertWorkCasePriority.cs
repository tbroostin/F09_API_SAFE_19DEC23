// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCasePriority
    {

        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Priority to change the Priority of the Case
        /// </summary>
        public string Priority { get; set; }

        public RetentionAlertWorkCasePriority(string updatedBy, string priority)
        {
            if (string.IsNullOrEmpty(priority))
            {
                throw new ArgumentNullException("priority");
            }
            if (!string.IsNullOrEmpty(updatedBy))
            {
                UpdatedBy = updatedBy;
            }

            Priority = priority;

        }
    }
}
