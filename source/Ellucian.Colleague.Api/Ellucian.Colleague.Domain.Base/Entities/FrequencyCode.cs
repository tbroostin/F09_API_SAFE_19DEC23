using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Frequency for calendar repeat events, such as Weekly, Monthly, Hourly, etc.
    /// </summary>
    [Serializable]
    public class FrequencyCode : CodeItem
    {
        public FrequencyCode(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}