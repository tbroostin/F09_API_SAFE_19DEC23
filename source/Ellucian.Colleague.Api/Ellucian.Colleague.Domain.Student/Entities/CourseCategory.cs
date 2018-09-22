using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CourseCategory : GuidCodeItem
    {
        public CourseCategory(string guid, string code, string desc)
            : base(guid, code, desc)
        {

        }
    }
}