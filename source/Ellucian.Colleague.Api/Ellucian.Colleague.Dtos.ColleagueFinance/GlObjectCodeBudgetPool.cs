// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Describes the members of a GL Budget Pool belonging to a GL object code. 
    /// The umbrella GL account and the list of poolee GL accounts.
    /// </summary>
    public class GlObjectCodeBudgetPool
    {
        /// <summary>
        /// The umbrella GL account for the GL object code budget pool.
        /// </summary>
        public GlObjectCodeGlAccount Umbrella { get; set; }

        /// <summary>
        /// The list of poolee GL accounts for the budget pool that the user
        /// has access to given their GL account security setup.
        /// </summary>
        public List<GlObjectCodeGlAccount> Poolees { get; set; }

        /// <summary>
        /// Whether the umbrella GL account would be visible to the user
        /// given their GL account security setup.
        /// </summary>
        public bool IsUmbrellaVisible { get; set; }

    }
}
