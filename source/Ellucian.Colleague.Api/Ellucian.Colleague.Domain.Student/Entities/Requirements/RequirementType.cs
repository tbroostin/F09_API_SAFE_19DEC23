// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class RequirementType : CodeItem
    {
        /// <summary>
        /// Relative processing priority for this requirement. Priority is used, after group type, as a component of prioritized
        /// sorting of groups in preparation for program evaluation.
        /// </summary>
        public int Priority { get { return _priority; } }
        private int _priority;

        /// <summary>
        /// Constructs a requirement type, which provides a processing priority for requirements.
        /// </summary>
        /// <param name="code">Identifying code for requirement type, required</param>
        /// <param name="description">Description for requirement type</param>
        /// <param name="priority">String representation of an integer indicating processing priority</param>
        public RequirementType(string code, string description, string priority)
            : base(code, description)
        {
            int parsedPriority;
            bool isInteger = Int32.TryParse(priority, out parsedPriority);
            if (isInteger)
            {
                _priority = parsedPriority;
            }
            else
            {
                _priority = int.MaxValue;
            }
        }
    }
}
