// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;
using System;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The detail account information for the line item.
    /// </summary>
    [DataContract]
    public class RequisitionsAccountDetailDtoProperty
    {
      /// <summary>
        /// The accounting string associated with the account detail item.
        /// </summary>
        [DataMember(Name = "accountingString", EmitDefaultValue = false)]
        public string AccountingString { get; set; }

        /// <summary>
        /// The allocation of line item values to the accounting string.
        /// </summary>
        [DataMember(Name = "allocation", EmitDefaultValue = false)]
        public RequisitionsAllocationDtoProperty Allocation { get; set; }

     
        /// <summary>
        /// An indication if budget checking should be enforced or overridden.
        /// </summary>
        [DataMember(Name = "budgetCheck", EmitDefaultValue = false)]
        public PurchaseOrdersAccountBudgetCheck? BudgetCheck { get; set; }

       
    }
}