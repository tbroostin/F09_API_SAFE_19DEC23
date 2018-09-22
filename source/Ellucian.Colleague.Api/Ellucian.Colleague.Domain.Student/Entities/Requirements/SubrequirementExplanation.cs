// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// All the possible reasons (Explanations) that can be attached to a SubrequirementResult, indicating why it is not complete.
    /// </summary>
    [Serializable]
    public enum SubrequirementExplanation
    {
        Satisfied,
        PlannedSatisfied,
        MinGroups,
        MinGpa,
        MinInstitutionalCredits,
    }
}
