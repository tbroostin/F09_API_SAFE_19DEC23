using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Item conditions.
    /// </summary>
    [Serializable]
    public class ItemConditions : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the<see cref="ItemConditions"/> class.
        /// </summary>
        /// <param name = "code" > The code.</param>
        /// <param name = "description" > The description.</param>
        public ItemConditions(string code, string description)
             : base(code, description)
        {

        }
    }
}
