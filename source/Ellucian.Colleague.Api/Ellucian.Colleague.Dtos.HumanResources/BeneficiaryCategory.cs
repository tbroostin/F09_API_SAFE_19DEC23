using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{   

    /// <summary>
    /// 
    /// </summary>
    public class BeneficiaryCategory
    { 
        /// <summary>
        /// Beneficiary Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Beneficiary Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Flag to indicate if beneficary code is Primary
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Constructor for BeneficiaryCategory
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="processingCode"></param>
        public BeneficiaryCategory(string code, string description, string processingCode) 
        {
            Code = code;
            Description = description;
            IsPrimary = !string.IsNullOrEmpty(processingCode) && processingCode.Equals("P");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BeneficiaryCategory()
        {
        }
    }
}
