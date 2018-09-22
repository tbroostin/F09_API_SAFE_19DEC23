using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Classification of a course for Federal Reporting
    /// </summary>
    [Serializable]
    public class FederalCourseClassification : CodeItem
    {
        public FederalCourseClassification(string code, string description)
            : base(code, description)
        {

        }
    }
}
