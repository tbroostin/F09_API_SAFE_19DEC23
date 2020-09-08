// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseActionResponse
    {
        /// <summary>
        ///  Key of the case record
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error messages
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        public RetentionAlertWorkCaseActionResponse()
        {
            ErrorMessages = new List<string>();
        }
    }
}
