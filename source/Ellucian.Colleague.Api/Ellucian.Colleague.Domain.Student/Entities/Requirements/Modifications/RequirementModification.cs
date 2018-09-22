// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications
{
    [Serializable]
    public abstract class RequirementModification
    {
        public readonly string blockId;
        public readonly string modificationMessage;

        protected RequirementModification(string blockid, string modificationmessage)
        {
            blockId = blockid;
            modificationMessage = modificationmessage;
        }

        /// <summary>
        /// Modifies ProgramRequirements if block id is null; Requirement, Sub-Requirement, or Group identified by block id otherwise
        /// </summary>
        /// <param name="programrequirements">The program's requirements</param>
        /// <param name="additionalRequirements">Additional requirements added to a student's program</param>
        public abstract void Modify(ProgramRequirements programrequirements, List<Requirement> additionalRequirements);

    }
}
