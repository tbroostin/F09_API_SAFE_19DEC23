// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class BlockReplacement : RequirementModification
    {
        public Requirement NewRequirement;


        public BlockReplacement(string blockid, Requirement newRequirement, string message)
            : base(blockid, message)
        {
            if (blockid == null)
            {
                throw new ArgumentNullException("blockid");
            }
            NewRequirement = newRequirement;
        }

        /// <summary>
        /// Adds waives a block or replaces a block with a new additional requirement
        /// </summary>
        /// <param name="programRequirements">The ProgramRequirements object to be modified</param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {

            // Performing a block replacement is easy! First, find and mark the waived/replaced block
            // NewRequirement is null in the case of a waiver.
            bool blockfound = false;
            Requirement currentreq = null;
            bool waived = (NewRequirement == null);
            bool groupreplace = false;
            Group grouptoreplace = null;

            if (programRequirements != null && programRequirements.Requirements != null)
            {
                foreach (var req in programRequirements.Requirements.Union(additionalRequirements ?? new List<Requirement>()))
                {
                    if (blockfound) continue;
                    currentreq = req;
                    if (req.Id == blockId)
                    {
                        if (waived) { req.IsWaived = true; } else { req.IsModified = true; }
                        blockfound = true;
                        req.ModificationMessages.Add(this.modificationMessage);
                        // Set backpointer
                        if (NewRequirement != null)
                        {
                            NewRequirement.Description = req.Description;
                            NewRequirement.SubRequirements.First().Requirement = req;
                            req.SubRequirements = NewRequirement.SubRequirements;
                        }
                        continue;
                    }
                    if (req.SubRequirements != null)
                    {
                        foreach (var sub in req.SubRequirements)
                        {
                            if (blockfound) continue;
                            if (sub.Id == blockId)
                            {
                                sub.ModificationMessages.Add(this.modificationMessage);
                                if (waived)
                                {
                                    sub.IsWaived = true;
                                }
                                else
                                {
                                    sub.IsModified = true;
                                    if (NewRequirement != null && NewRequirement.SubRequirements != null && NewRequirement.SubRequirements.Any() && NewRequirement.SubRequirements.First() != null)
                                    {
                                        // Set new requirement's groups(s)' backpointer(s) to this subrequirement
                                        foreach (var grp in NewRequirement.SubRequirements.First().Groups)
                                        {
                                            grp.SubRequirement = sub;
                                        }
                                        // Set forward pointer(s) from this subrequirement to the new group(s)
                                        sub.Groups = NewRequirement.SubRequirements.First().Groups;
                                    }
                                }

                                blockfound = true;
                                continue;
                            }
                            if (sub.Groups != null)
                            {
                                foreach (var grp in sub.Groups)
                                {
                                    if (grp.Id == blockId)
                                    {
                                        if (waived)
                                        {
                                            grp.IsWaived = true;
                                            grp.ModificationMessages.Add(this.modificationMessage);
                                        }
                                        else
                                        {
                                            grp.IsModified = true;
                                        }
                                        groupreplace = true;
                                        grouptoreplace = grp;
                                        blockfound = true;
                                        continue;
                                    }
                                }
                                if (groupreplace && !waived)
                                {
                                    if (NewRequirement != null && NewRequirement.SubRequirements != null && NewRequirement.SubRequirements.First() != null && NewRequirement.SubRequirements.First().Groups != null)
                                    {
                                        sub.Groups.Remove(grouptoreplace);
                                        foreach (var newgrp in NewRequirement.SubRequirements.First().Groups)
                                        {
                                            newgrp.ModificationMessages.Add(this.modificationMessage);
                                            // Set backpointer
                                            newgrp.SubRequirement = sub;
                                            // Set pointer
                                            sub.Groups.Add(newgrp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // Did not find a matching block (calling routine now catches this)
            if (!blockfound) throw new KeyNotFoundException("requirements");

        }
    }
}
