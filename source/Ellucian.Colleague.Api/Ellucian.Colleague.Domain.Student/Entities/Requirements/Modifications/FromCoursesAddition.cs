// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class FromCoursesAddition : RequirementModification
    {
        public readonly List<string> AdditionalFromCourses;

        public FromCoursesAddition(string blockid, List<string> additionalfromcourses, string message)
            : base(blockid, message)
        {
            if (string.IsNullOrEmpty(blockid))
            {
                throw new NotSupportedException("From courses addition must apply to a block.");
            }
            AdditionalFromCourses = additionalfromcourses;
        }

        /// <summary>
        /// Adds additional courses to a Group's "FromCourses" list
        /// </summary>
        /// <param name="programRequirements">The ProgramRequirements object to be modified</param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {
            if (programRequirements != null && programRequirements.Requirements != null)
            {
                var allRequirements = programRequirements.Requirements;
                if (additionalRequirements != null)
                {
                    allRequirements = allRequirements.Union(additionalRequirements).ToList();
                }
                foreach (var req in allRequirements)
                {
                    if (req != null && req.SubRequirements != null)
                    {
                        foreach (var sub in req.SubRequirements)
                        {
                            if (sub != null && sub.Groups != null)
                            {
                                foreach (var grp in sub.Groups)
                                {
                                    if (grp != null)
                                    {
                                        if (grp.Id == blockId)
                                        {
                                            if (AdditionalFromCourses != null && AdditionalFromCourses.Any())
                                            {
                                                grp.IsModified = true;
                                                grp.IsWaived = false; 

                                                // The modification message applies to all additional courses and should only be added once
                                                if (grp.ModificationMessages != null)
                                                {
                                                    grp.ModificationMessages.Add(this.modificationMessage);
                                                }
                                                else
                                                {
                                                    grp.ModificationMessages = new List<string>() { this.modificationMessage };
                                                }
                                                foreach (var crse in AdditionalFromCourses)
                                                {
                                                    if (grp.FromCoursesException != null)
                                                    {
                                                        grp.FromCoursesException.Add(crse);
                                                    }
                                                    else
                                                    {
                                                        grp.FromCoursesException = new List<string>() { crse };
                                                    }
                                                }
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // did not find a matching block
            throw new KeyNotFoundException("requirements");
        }
    }
}
