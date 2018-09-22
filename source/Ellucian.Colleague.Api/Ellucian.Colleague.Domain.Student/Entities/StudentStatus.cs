using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentStatus : GuidCodeItem
    {
        public StudentStatus(string guid, string code, string desc)
            : base(guid, code, desc)
        {

        }
    }
}