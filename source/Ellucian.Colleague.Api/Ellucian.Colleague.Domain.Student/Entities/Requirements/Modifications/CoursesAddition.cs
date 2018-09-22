// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class CoursesAddition : RequirementModification
    {
        public readonly List<string> AdditionalCourses;

        public CoursesAddition(string blockid, List<string> additionalcourses, string message)
            : base(blockid, message)
        {
            if (blockid == null)
            {
                throw new NotSupportedException("Course addition must apply to a block.");
            }
            AdditionalCourses = additionalcourses;
        }

        /// <summary>
        /// Adds additional courses to a Group
        /// </summary>
        /// <param name="programRequirements">The ProgramRequirements object to be modified</param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {
            foreach (var req in programRequirements.Requirements.Union(additionalRequirements))
            {
                foreach (var sub in req.SubRequirements)
                {
                    foreach (var grp in sub.Groups)
                    {
                        if (grp.Id == blockId)
                        {
                            foreach (var crse in AdditionalCourses)
                            {
                                grp.ModificationMessages.Add(this.modificationMessage);
                                grp.IsModified = true;
                                grp.Courses.Add(crse);
                                grp.IsWaived = false;  // in case this was set to true by a waiver 
                            }
                            return;
                        }
                    }
                }
            }
            // did not find a matching block
            throw new KeyNotFoundException("requirements");
        }
    }
}
