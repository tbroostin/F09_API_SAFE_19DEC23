// Copyright 2018 Ellucian Company L.P. and its affiliates
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Drop Reason Code Table
    /// </summary>
    [Serializable]
    public class DropReason : CodeItem
    {
        /// <summary>
        /// to indicate if a drop reason code is displayed in SelfService, if it is "S" , it will be displayed in SelfService 
        /// </summary>
        public bool DisplayInSelfService { get; set; }
        /// <summary>
        /// Overloaded constructor for Drop Reason class
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public DropReason(string code, string description, string specialProcessingActionCode) : base(code, description)
        {
            if (!string.IsNullOrEmpty(specialProcessingActionCode) && string.Equals(specialProcessingActionCode, "S", StringComparison.OrdinalIgnoreCase))
            {
                DisplayInSelfService = true; 
            }
            else
            {
                DisplayInSelfService = false;
            }
        }
    }
}
