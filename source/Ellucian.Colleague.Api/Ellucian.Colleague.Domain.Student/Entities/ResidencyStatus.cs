using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ResidencyStatus : GuidCodeItem
    {
        public ResidencyStatus(string guid, string code, string desc)
            : base(guid, code, desc)
        {

        }
    }
}
