using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Advisor Types Code table
    /// </summary>
    [Serializable]
    public class AdvisorType : GuidCodeItem
    {

        /// <summary>
        /// Rank of advisor type.  Sequential with "1" as most important
        /// </summary>
        public string Rank { get; set; }

        public AdvisorType(string guid, string code, string description, string rank)
            : base(guid, code, description)
        {
            Rank = rank;
        }
    }
}
