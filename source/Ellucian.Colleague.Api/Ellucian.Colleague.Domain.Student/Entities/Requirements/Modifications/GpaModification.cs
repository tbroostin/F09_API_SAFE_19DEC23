// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class GpaModification : RequirementModification
    {
        public readonly decimal? Gpa;

        public GpaModification(string blockid, decimal? gpa, string message)
            :base(blockid, message)
        {
            Gpa = gpa;
        }
        /// <summary>
        /// Modifies ProgramRequirements, Requirement, SubRequirement, or Group minimum GPA
        /// </summary>
        /// <param name="programRequirements"></param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {
            if (string.IsNullOrEmpty(blockId))
            {
                programRequirements.MinOverallGpa = Gpa;
                return;
            }
            else
            {
                foreach (var req in programRequirements.Requirements.Union(additionalRequirements))
                {
                    if (req.Id == blockId)
                    {
                        req.ModificationMessages.Add(this.modificationMessage);
                        req.IsModified = true;
                        req.MinGpa = Gpa;
                        return;
                    }
                    foreach (var sub in req.SubRequirements)
                    {
                        if (sub.Id == blockId)
                        {
                            sub.ModificationMessages.Add(this.modificationMessage);
                            sub.IsModified = true;
                            sub.MinGpa = Gpa;
                            return;
                        }
                        foreach (var grp in sub.Groups)
                        {
                            if (grp.Id == blockId)
                            {
                                grp.ModificationMessages.Add(this.modificationMessage);
                                grp.IsModified = true;
                                grp.MinGpa = Gpa;
                                return;
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
