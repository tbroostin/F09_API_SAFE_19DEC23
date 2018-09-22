using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Classification of a course for Local Government Reporting (State or Local)
    /// </summary>
    [Serializable]
    public class LocalCourseClassification : CodeItem
    {
        public LocalCourseClassification(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}