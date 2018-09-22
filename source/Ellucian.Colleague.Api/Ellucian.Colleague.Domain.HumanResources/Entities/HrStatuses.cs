//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// HrStatuses
    /// </summary>
    [Serializable]
    public class HrStatuses : GuidCodeItem
    {
        /// <summary>
        /// Contents based on special processing 1 in valcode table.
        /// </summary>
        public bool IsEmployeeStatus { get; set; }
        
        /// <summary>
        /// Category of Full Time, Part Time or Contractual
        /// </summary>
        public ContractType? Category { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HrStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public HrStatuses(string guid, string code, string description)
            : base(guid, code, description)
        {
        }

        public HrStatuses(string guid, string code, string description, string employeeFlag)
            : base(guid, code, description)
        {
            IsEmployeeStatus = employeeFlag.ToUpper() == "Y" ? true : false;
        }       

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonStatuses"/> class.
        /// </summary>
        public HrStatuses(string guid, string code, string description, string employeeFlag, string category)
            : base(guid, code, description)
        {
            IsEmployeeStatus = employeeFlag.ToUpper() == "Y" ? true : false;

            if (!string.IsNullOrEmpty(category))
            {
                switch (category)
                {
                    case "fullTime":
                        Category = ContractType.FullTime;
                        break;
                    case "partTime":
                        Category = ContractType.PartTime;
                        break;
                    case "contractual":
                        Category = ContractType.Contractual;
                        break;    
                }
            } 
            else
            {
                Category = ContractType.PartTime;
            }           
        }

    }
}
