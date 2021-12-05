// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Identifying information for an academic requirement and (optionally) an associated sub-requirement and group 
    /// </summary>
    [Serializable]
    public class AcademicRequirementGroup
    {
        /// <summary>
        /// Optional code for an academic requirement
        /// </summary>
        public string AcademicRequirementCode { get; private set; }

        /// <summary>
        /// Optional identifier for a sub-requirement from an academic requirement
        /// </summary>
        public string SubrequirementId { get; private set; }

        /// <summary>
        /// Optional identifier for a group within a sub-requirement from an academic requirement
        /// </summary>
        public string GroupId { get; private set; }

        /// <summary>
        /// Creates a new <see cref="AcademicRequirementGroup"/>
        /// </summary>
        /// <param name="academicRequirementCode">Optional code for an academic requirement that is associated with the course placeholder</param>
        /// <param name="subrequirementId">Optional identifier for a sub-requirement from an academic requirement that is associated with the course placeholder</param>
        /// <param name="groupId">Optional identifier for a group within a sub-requirement from an academic requirement that is associated with the course placeholder</param>
        public AcademicRequirementGroup(string academicRequirementCode, string subrequirementId, string groupId)
        {
            if (string.IsNullOrEmpty(academicRequirementCode))
            {
                if (!string.IsNullOrEmpty(subrequirementId))
                {
                    throw new ArgumentNullException("academicRequirementCode", "An academic requirement code is required when specifying a sub-requirement.");
                }
                if (!string.IsNullOrEmpty(groupId))
                {
                    throw new ArgumentNullException("academicRequirementCode", "An academic requirement code is required when specifying a sub-requirement group.");
                }
            }
            if (string.IsNullOrEmpty(subrequirementId))
            {
                if (!string.IsNullOrEmpty(groupId))
                {
                    throw new ArgumentNullException("subrequirementId", "An sub-requirement ID is required when specifying a sub-requirement group.");
                }
            }
            AcademicRequirementCode = academicRequirementCode;
            SubrequirementId = subrequirementId;
            GroupId = groupId;
        }
    }
}
