// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class InstitutionalCreditModification : RequirementModification
    {
        public readonly decimal? institutionalCredits;

        public InstitutionalCreditModification(string blockid, decimal? institutionalcredits, string message)
            :base(blockid,message)
        {
            institutionalCredits = institutionalcredits;
        }
        /// <summary>
        /// Modifies ProgramRequirements, Requirement, SubRequirement, or Group minimum institutional credit
        /// </summary>
        /// <param name="programRequirements"></param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {
            if (string.IsNullOrEmpty(blockId))
            {
                programRequirements.MinimumInstitutionalCredits = institutionalCredits;
                return;
            }
            else
            {
                foreach (var req in programRequirements.Requirements.Union(additionalRequirements))
                {
                    if (req.Id == blockId)
                    {
                        req.IsModified = true;
                        req.ModificationMessages.Add(this.modificationMessage);
                        req.MinInstitutionalCredits = institutionalCredits;
                        return;
                    }
                    foreach (var sub in req.SubRequirements)
                    {
                        if (sub.Id == blockId)
                        {
                            sub.ModificationMessages.Add(this.modificationMessage);
                            sub.IsModified = true;
                            sub.MinInstitutionalCredits = institutionalCredits;
                            return;
                        }
                        foreach (var grp in sub.Groups)
                        {
                            if (grp.Id == blockId)
                            {
                                grp.IsModified = true;
                                grp.ModificationMessages.Add(this.modificationMessage);
                                grp.MinInstitutionalCredits = institutionalCredits;
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
