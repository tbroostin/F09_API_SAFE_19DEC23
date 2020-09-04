using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// BeneficiaryCategory Domain Entity
    /// </summary>
    [Serializable]
    public class BeneficiaryCategory : CodeItem
    {
        /// <summary>
        /// Processing code of the Beneficiary Category
        /// </summary>
        public string ProcessingCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeneficiaryCategory"/> class.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="processingCode"></param>
        public BeneficiaryCategory(string code, string description, string processingCode) : base(code, description)
        {
            ProcessingCode = processingCode;
        }
    }

}
