using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class OtherAward : CodeItem
    {
        public OtherAward(string code, string description)
            : base(code, description)
        {
            // No additional work to do
        }
    }
}
