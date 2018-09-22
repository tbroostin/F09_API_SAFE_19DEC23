// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class CreditModification : RequirementModification
    {
        public readonly decimal? Credits;

        public CreditModification(string blockid, decimal? credits, string message)
            : base(blockid, message)
        {
            Credits = credits;
        }

        /// <summary>
        /// Changes the minimum requirements for either the overall program, or an individual group if the block id is not null.
        /// </summary>
        /// <param name="programRequirements">The ProgramRequirements object to be modified</param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {
            if (string.IsNullOrEmpty(blockId))
            {
                programRequirements.MinimumCredits = Credits;
                return;
            }
            else
            {
                foreach (var req in programRequirements.Requirements.Union(additionalRequirements))
                {
                    foreach (var sub in req.SubRequirements)
                    {
                        foreach (var grp in sub.Groups)
                        {
                            if (grp.Id == blockId)
                            {
                                grp.IsModified = true;
                                grp.ModificationMessages.Add(this.modificationMessage);
                                grp.MinCredits = Credits;
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
