using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Code Item of CredTypes
    /// </summary>
    [Serializable]
    public class CredType : CodeItem
    {
        /// <summary>
        /// Credit Type Category (Institutional, Transfer, Continuing Ed, Other)
        /// </summary>
        public CreditType Category { get; set; }

        public CredType(string code, string desc)
            : base(code, desc)
        {

        }
    }
}
