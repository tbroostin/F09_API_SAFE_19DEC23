using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentType : GuidCodeItem
    {
        public StudentType(string guid, string code, string desc)
            : base(guid, code, desc)
        {

        }
    }
}