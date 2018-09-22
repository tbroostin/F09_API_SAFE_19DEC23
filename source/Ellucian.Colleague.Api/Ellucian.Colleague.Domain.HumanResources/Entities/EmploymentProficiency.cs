using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// EmploymentProficiency
    /// </summary>
    [Serializable]
    public class EmploymentProficiency : GuidCodeItem
    {
        /// <summary>
        /// The licensing certification
        /// </summary>
        public string Certification { get; set; }

        /// <summary>
        /// The comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The authority
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
         /// Initializes a new instance of the <see cref="EmploymentProficiency"/> class.
        /// </summary>
         public EmploymentProficiency(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
