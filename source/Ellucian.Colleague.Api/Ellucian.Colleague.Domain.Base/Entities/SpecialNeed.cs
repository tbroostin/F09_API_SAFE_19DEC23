using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Special Needs Codes for individuals with disabilities or other special needs
    /// </summary>
    [Serializable]
    public class SpecialNeed : CodeItem
    {
        public SpecialNeed(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}
