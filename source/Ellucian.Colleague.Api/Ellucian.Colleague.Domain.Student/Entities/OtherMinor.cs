using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class OtherMinor: CodeItem
    {       
        public OtherMinor(string code, string description)
            : base(code, description)
        {
            // No additional work to do
        }
    }
}
