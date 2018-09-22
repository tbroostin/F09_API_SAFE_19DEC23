//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// LeaveTypes
    /// </summary>
    [Serializable]
    public class LeaveType : GuidCodeItem
    {
        /// <summary>
        /// The time type is used to determine if the leave is of type vacation, sick, or compensetory
        /// </summary>
       public LeaveTypeCategory TimeType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaveType"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public LeaveType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}