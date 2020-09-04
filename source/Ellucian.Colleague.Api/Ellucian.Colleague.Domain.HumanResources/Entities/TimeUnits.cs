//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// HrStatuses
    /// </summary>
    [Serializable]
    public class TimeUnits : GuidCodeItem
    {        
        /// <summary>
        /// Category of Full Time, Part Time or Contractual
        /// </summary>
        public TimeUnitCategory? Category { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HrStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public TimeUnits(string guid, string code, string description)
            : base(guid, code, description)
        {
        }       

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonStatuses"/> class.
        /// </summary>
        public TimeUnits(string guid, string code, string description, string category)
            : base(guid, code, description)
        {

            if (!string.IsNullOrEmpty(category))
            {
                switch (category)
                {
                    case "1":
                        Category = TimeUnitCategory.Days;
                        break;
                    case "2":
                        Category = TimeUnitCategory.Weeks;
                        break;
                    case "3":
                        Category = TimeUnitCategory.Months;
                        break;
                    case "4":
                        Category = TimeUnitCategory.Years;
                        break;
                }
            } 
            else
            {
                Category = TimeUnitCategory.Hours;
            }           
        }

    }
}
