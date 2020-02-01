// Copyright 2017 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Method to use in determining the catalog year to be used in program evaluation "what-if" scenarios and when selecting
    /// sample degree plans for programs outside the student's current program.
    /// </summary>
    public enum CatalogPolicy
    {
        /// <summary>
        /// The most recent catalog year on the program that does not start in the future
        /// </summary>
        CurrentCatalogYear,

        /// <summary>
        /// The catalog year on the first active StudentProgram for the student
        /// </summary>
        StudentCatalogYear
    }
}
