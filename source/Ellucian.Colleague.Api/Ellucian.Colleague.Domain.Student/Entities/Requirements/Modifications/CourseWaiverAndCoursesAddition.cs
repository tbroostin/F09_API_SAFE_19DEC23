// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class CourseWaiverAndCoursesAddition : RequirementModification
    {
        public readonly List<string> AdditionalCourses;
        public readonly List<string> WaivedCourses;

        public CourseWaiverAndCoursesAddition(string blockid, List<string> additionalcourses, List<string> waivedcourses, string message)
            : base(blockid, message)
        {
            if (blockid == null)
            {
                throw new NotSupportedException("Course waiver/addition must apply to a block.");
            }
            AdditionalCourses = additionalcourses;
            WaivedCourses = waivedcourses;
        }


        /// <summary>
        /// Removes Waived courses from a Group and adds new required courses in their stead
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

                            foreach (var crse in AdditionalCourses)
                            {
                                grp.Courses.Add(crse);
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
