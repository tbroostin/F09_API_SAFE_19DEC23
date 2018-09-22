// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class CountModification : RequirementModification
    {
        public readonly int? Count;

        public CountModification(string blockid, int? count, string message)
            : base(blockid, message)
        {
            Count = count;
        }

        /// <summary>
        /// Changes the minimum requirements for either the overall program, or an individual group if the block id is not null.
        /// </summary>
        /// <param name="programRequirements">The ProgramRequirements object to be modified</param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {
            if (string.IsNullOrEmpty(blockId))
            {
                throw new NotSupportedException("This kind of modification must be specific to a requirement block.");
            }
            else
            {
                foreach (var req in programRequirements.Requirements.Union(additionalRequirements))
                {
                    if (req.Id == blockId)
                    {
                        req.MinSubRequirements = Count;
                        req.ModificationMessages.Add(this.modificationMessage);
                        return;
                    }
                    foreach (var sub in req.SubRequirements)
                    {
                        if (sub.Id == blockId)
                        {
                            sub.ModificationMessages.Add(this.modificationMessage);
                            sub.MinGroups = Count;
                            return;
                        }
                        foreach (var grp in sub.Groups)
                        {
                            if (grp.Id == blockId)
                            {
                                if (Count == null)
                                {
                                    throw new NotSupportedException("Cannot change the MinCourses to null.  This should be a waiver.");
                                }
                                grp.ModificationMessages.Add(this.modificationMessage);
                                grp.MinCourses = Count;
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
