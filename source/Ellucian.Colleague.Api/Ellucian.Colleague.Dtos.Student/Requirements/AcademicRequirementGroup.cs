// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Identifying information for an academic requirement and (optionally) an associated sub-requirement and group 
    /// </summary>
    public class AcademicRequirementGroup
    {
        /// <summary>
        /// Optional code for an academic requirement
        /// </summary>
        public string AcademicRequirementCode { get; set; }

        /// <summary>
        /// Optional identifier for a sub-requirement from an academic requirement
        /// </summary>
        public string SubrequirementId { get; set; }

        /// <summary>
        /// Optional identifier for a group within a sub-requirement from an academic requirement
        /// </summary>
        public string GroupId { get; set; }
    }
}
