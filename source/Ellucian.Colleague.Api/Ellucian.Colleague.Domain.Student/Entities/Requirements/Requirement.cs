// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class Requirement : RequirementBlock
    {
        // ** REQUIRED **

        // Requirements have two keys - the block id in the ACAD.REQMT.BLOCKS and the code in ACAD.REQMTS
        // To make things clearer, I have changed this Id field to be the block id, same as for SubRequirements
        // and Groups.  The code from ACAD.REQMTS, like "HIST.CORE" will be stored in Code.  These are in the 
        // BlockBase class.

        // However, if the requirement is a prerequisite, the code can still be numeric (generated
        // by Colleague.)

        // If this requirement is part of the main set of requirements for a program
        // this will be a reference to the parent ProgramRequirements.  If this requirement
        // is a prerequisite or an additional requirment, this will be null.

        public ProgramRequirements ProgramRequirements { get; set; }


        // History Gen Ed Requirement
        private string _Description { get; set; }
        public string Description { get { return _Description; } set { _Description = value; } }

        public string GradeSchemeCode { get; set; }
        public RequirementType RequirementType { get; set; }
        public int? MinSubRequirements { get; set; }
        public bool CustomUse { get; set; }

        //requirement type codes
        // CODE DESCRIPTION    MIN ACTION1
        // MAJ  Major          MAJ 1
        // MIN  Minor          MIN 2
        // SPC  Specialization SPC 3
        // Ccd  Ccd            Ccd 4
        // OTH  Other          OTH 5
        // GEN  General (Core) GEN 6
        // ELE  Elective       ELE 7

        // PRE Prerequisite
        // ** OPTIONAL **
        
        public List<Subrequirement> SubRequirements { get; set; }

        /// <summary>
        /// This is the list of all the requirment exclusions.
        ///Says whether a requirement satisfying one requirement type can be
        ///used in this requirement, and whether or not the exclusion only
        ///applies to the first occurrence of that type or to all.
        /// </summary>
        public List<RequirementBlockExclusion> RequirementExclusions { get; set; }

        public Requirement(string id, string requirementCode, string description, string gradeSchemeCode, RequirementType requirementType, ProgramRequirements programRequirements = null)
            :base(id, requirementCode)
        {
            
            _Description = description;
            
            GradeSchemeCode = gradeSchemeCode;
            RequirementType = requirementType;
            ProgramRequirements = programRequirements;
            SubRequirements = new List<Subrequirement>();
            RequirementExclusions = new List<RequirementBlockExclusion>();
                        
        }

        public RequirementResult Evaluate(List<SubrequirementResult> subrequirementresults)
        {
            RequirementResult rr = new RequirementResult(this);

            rr.SubRequirementResults.AddRange(subrequirementresults);
            int numsubrequirements = SubRequirements.Count();
            int numNeededSubrequrements = numsubrequirements;

            if (MinSubRequirements.HasValue && MinSubRequirements != 0)
            {
                numNeededSubrequrements = MinSubRequirements.GetValueOrDefault();
            }
            else
            {
                numNeededSubrequrements = numsubrequirements;
            }

            int satisfiedSubrequirements = rr.SubRequirementResults.Where(srr => srr.IsSatisfied()).Count();
            int plannedSatisfiedSubrequirements = rr.SubRequirementResults.Where(srr => srr.IsPlannedSatisfied()).Count();
            if (satisfiedSubrequirements < numNeededSubrequrements)
            {
                rr.Explanations.Add(RequirementExplanation.MinSubRequirements);
            }

            if (MinInstitutionalCredits.HasValue)
            {
                if (MinInstitutionalCredits.Value > rr.GetAppliedInstitutionalCredits())
                {
                    rr.Explanations.Add(RequirementExplanation.MinInstitutionalCredits);
                }
            }

            // If all subrequirements are not completed or the minimum institutional credits has not been met, determine if we can call this "fully planned"
            if (rr.Explanations.Contains(RequirementExplanation.MinSubRequirements) || rr.Explanations.Contains(RequirementExplanation.MinInstitutionalCredits))
            {
                // Consider planned satisfied if the minimum number of subrequirements is satisfied and the number of institutional credits applied + planned meets the minimum
                if (satisfiedSubrequirements + plannedSatisfiedSubrequirements >= numNeededSubrequrements && (!MinInstitutionalCredits.HasValue || rr.GetAppliedInstitutionalCredits() + rr.GetPlannedAppliedCredits() >= MinInstitutionalCredits.Value ))
                    rr.Explanations.Add(RequirementExplanation.PlannedSatisfied);
            }

            if (MinGpa.HasValue)
            {
                if (rr.Gpa < MinGpa.Value)
                {
                    rr.Explanations.Add(RequirementExplanation.MinGpa);
                }
            }

            if (rr.Explanations.Count() == 0)
            {
                rr.Explanations.Add(RequirementExplanation.Satisfied);
            }   


            return rr;
        }

        /// <summary>
        /// Get all the rules from a list of requirements
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        public List<RequirementRule> GetAllRules()
        {
            List<RequirementRule> reqrules = new List<RequirementRule>();

            reqrules.AddRange(this.GetRules());

            if (SubRequirements != null)
            {
                foreach (var sub in SubRequirements)
                {
                    reqrules.AddRange(sub.GetRules());

                    if (sub.Groups != null)
                    {
                        foreach (var grp in sub.Groups)
                        {
                            reqrules.AddRange(grp.GetRules());
                        }
                    }

                }
            }
            return reqrules.ToList();
        }

        /// <summary>
        ///  Ensure that subrequirements inherit sort specifications from the parent requirement when appropriate,
        ///  and then propagate that to all groups on each subrequirement.  The requirement's sort spec will
        ///  only be applied to subrequirements that do not have a sort spec, and the sort spec will only be
        ///  applied to groups that do not have a sort spec.
        /// </summary>
        /// <param name="requirement">Requirement object to update</param>
        public void CascadeSortSpecificationWhenNecessary()
        {
            foreach (var subReq in SubRequirements)
            {
                if (string.IsNullOrEmpty(subReq.SortSpecificationId))
                {
                    subReq.SortSpecificationId = SortSpecificationId;
                }
                // In any case, need to also make sure that the sub-requirement's sort spec is
                // applied to any of it's groups without a sort spec.
                if (!string.IsNullOrEmpty(subReq.SortSpecificationId))
                {
                    foreach (var group in subReq.Groups)
                    {
                        if (string.IsNullOrEmpty(group.SortSpecificationId))
                        {
                            group.SortSpecificationId = subReq.SortSpecificationId;
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Requirement other = obj as Requirement;
            if ((Requirement)other == null)
            {
                return false;
            }
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            if (Id == null)
            {
                return base.GetHashCode();
            }
            return Id.GetHashCode();
        }
    }
}
