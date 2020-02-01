// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// Method to use in determining the catalog year to be used in program evaluation "what-if" scenarios and when selecting
    /// sample degree plans for programs outside the student's current program.
    /// - CurrentCatalogYear uses the most recent catalog year on the program (that doesn't start in the future)
    /// - StudentCatalogYear uses the catalog year on the first active StudentProgram for the student.
    /// </summary>
    [Serializable]
    public enum CatalogPolicy
    {
        CurrentCatalogYear, StudentCatalogYear
    }
}
