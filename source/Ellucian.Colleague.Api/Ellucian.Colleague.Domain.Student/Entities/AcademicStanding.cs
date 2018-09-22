using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Academic Standing Code table
    /// </summary>
    [Serializable]
    public class AcademicStanding : CodeItem
    {
        public AcademicStanding(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}
