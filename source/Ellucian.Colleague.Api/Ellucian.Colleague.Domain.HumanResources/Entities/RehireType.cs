using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// RehireType
    /// </summary>
    [Serializable]
    public class RehireType : GuidCodeItem
    {
        /// <summary>
        /// Category of Eligible or Ineligible
        /// </summary>
        public RehireTypeCategory? Category { get; set; }

        /// <summary>
         /// Initializes a new instance of the <see cref="RehireType"/> class.
        /// </summary>
        /// <param name="guid">Uniquie ID for the rehire type</param>
        /// <param name="code">Code representation of the rehire type</param>
        /// <param name="description">Description of the rehire type</param>
        /// <param name="category">Category of Eligible or Ineligible</param>
         public RehireType(string guid, string code, string description, string category)
            : base(guid, code, description)
        {
            if (!string.IsNullOrEmpty(category) && category.Substring(0, 1).ToUpperInvariant() == "I")
            {
                Category = RehireTypeCategory.Ineligible;
            }
            else
            {
                Category = RehireTypeCategory.Eligible;
            }
        }
    }
}
