// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class CourseWaiver : RequirementModification
    {
        public readonly List<string> WaivedCourses;

        public CourseWaiver(string blockid, List<string> waivedcourses, string message)
            : base(blockid, message)
        {
            if (blockid == null)
            {
                throw new NotSupportedException("Course waiver must apply to a block.");
            }
            WaivedCourses = waivedcourses;
        }

        /// <summary>
        /// Removes Waived courses from a Group
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
                            grp.ModificationMessages.Add(this.modificationMessage);
                            grp.IsModified = true;
                            foreach (var crse in WaivedCourses)
                            {
                                grp.Courses.Remove(crse);
                            }
                            // Colleague EVAL can deal with no courses on the list.  I think this is simpler.
                            // If no courses left, treat as waiver.
                            if (grp.Courses.Count == 0)
                            {
                                grp.IsWaived = true;
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
