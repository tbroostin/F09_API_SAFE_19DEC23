// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Attributes of Case Type related to Retention Alert
    /// </summary>
    [Serializable]
    public class CaseType : CodeItem
    {
        /// <summary>
        /// The CaseType Id
        /// </summary>
        public string CaseTypeId { get; set; }

        /// <summary>
        /// The Category to which the case belongs
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The Priority of the case
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// A flag to indicate if the case is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// A flag to indicate if manual contribution to this case is allowed
        /// </summary>
        public bool AllowCaseContribution { get; set; }

        /// <summary>
        /// A list of available communication codes
        /// </summary>
        public List<string> AvailableCommunicationCodes { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">case type code</param>
        /// <param name="description">case type code description</param>
        /// <param name="category">category to which the case type belongs</param>
        /// <param name="priority">priority of the case type</param>
        /// <param name="isActive">true if the case type is active</param>
        /// <param name="allowCaseContribution">true if manual contribution is allowed for case type</param>
        ///<param name="availableCommunicationCodes">list of available communication codes for the case type</param>
        public CaseType(string code, string description,string caseTypeId, string category, string priority, bool isActive, bool allowCaseContribution,List<string> availableCommunicationCodes)
            : base(code, description)
        {
            CaseTypeId = caseTypeId;
            Category = category;
            Priority = priority;          
            IsActive = isActive;           
            AllowCaseContribution = allowCaseContribution;
            AvailableCommunicationCodes = availableCommunicationCodes;
        }
    }
}

