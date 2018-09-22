// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public class InstitutionalGpaModification : RequirementModification
    {
        public readonly decimal? Gpa;

        public InstitutionalGpaModification(string blockid, decimal? gpa, string message)
            : base(blockid, message)
        {
            Gpa = gpa;
        }

        /// <summary>
        /// Changes the minimum institutional GPA requirement 
        /// </summary>
        /// <param name="programRequirements">The ProgramRequirements object to be modified</param>
        public override void Modify(ProgramRequirements programRequirements, List<Requirement> additionalRequirements)
        {
            programRequirements.MinInstGpa = Gpa;
            return;
        }
    }
}
