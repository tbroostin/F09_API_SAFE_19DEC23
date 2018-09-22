using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class DistributionMethod : GuidCodeItem
    {
        /// <summary>
        /// Constructor for Cash reciepts distribution method
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        
        public DistributionMethod(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
