//Copyright 2018 Ellucian Company L.P. and its affiliates*/

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// An AwardCategoryType generalizes groups of awards even further than 
    /// an award's category.
    /// </summary>
    [Serializable]
    public enum AwardCategoryType
    {
        Loan,
        Grant,
        Scholarship,
        Work
    }
}