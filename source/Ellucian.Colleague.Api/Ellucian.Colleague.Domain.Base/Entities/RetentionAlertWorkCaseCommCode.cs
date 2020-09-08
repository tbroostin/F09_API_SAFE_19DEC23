// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseCommCode
    {

        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Communication Code to add to the Case
        /// </summary>
        public string CommunicationCode { get; set; }

        public RetentionAlertWorkCaseCommCode(string updatedBy, string communicationCode)
        {
            if (string.IsNullOrEmpty(communicationCode))
            {
                throw new ArgumentNullException("communicationCode");
            }
            if (!string.IsNullOrEmpty(updatedBy))
            {
                UpdatedBy = updatedBy;
            }

            CommunicationCode = communicationCode;
            
        }
    }
}
