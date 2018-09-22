//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Deduction type.
    /// </summary>
    [Serializable]
    public class DeductionType: GuidCodeItem
    {
        /// <summary>
        /// The cost calculation method associated with the deduction type.
        /// </summary>
        public string CostCalculationMethod { get { return costCalculationMethod; } }
        private string costCalculationMethod;

        /// <summary>
        /// The category of deduction associated with the deduction type.
        /// </summary>
        public string Category { get { return category; } }
        private string category;

        /// <summary>
        /// The withholding frequency associated with the deduction type.
        /// </summary>
        public int? WithholdingFrequency { get { return withholdingFrequency; } }
        private int? withholdingFrequency;

        /// <summary>
        /// An indicator if the deduction should be applied before or after taxes are withheld.
        /// </summary>
        public List<string> BD_TXABL_TAX_CODES { get { return bd_txabl_tax_codes; } }
        private List<string> bd_txabl_tax_codes;

        /// <summary>
        /// An indicator if the deduction should be applied before or after taxes are withheld.
        /// </summary>
        public List<string> BD_DEFER_TAX_CODES { get { return bd_defer_tax_codes; } }
        private List<string> bd_defer_tax_codes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeductionType"/> class.
        /// </summary>
        public DeductionType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeductionType"/> class.
        /// </summary>
        public DeductionType(string guid, string code, string description, string category, string ccm, int? whf, List<string> ttc, List<string> dtc)
            : base(guid, code, description)
        {
            if (string.IsNullOrEmpty(ccm))
            {
                throw new KeyNotFoundException("Cost Calculation Method was not found.");
            }
            this.category = category;
            this.costCalculationMethod = ccm;
            this.withholdingFrequency = whf;
            this.bd_txabl_tax_codes = ttc;
            this.bd_defer_tax_codes = dtc;
        }
    }
}
