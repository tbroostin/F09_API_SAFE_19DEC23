// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{   
   /// <summary>
    /// Type of the financial aid fund (loan, grant, scholarship, work).
    /// </summary>
    [Serializable]
    public enum FinancialAidFundsAidType
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
       
                           
         /// <summary>
        /// loan
        /// </summary>
        Loan,
                     
         /// <summary>
        /// grant
        /// </summary>
        Grant,
                     
         /// <summary>
        /// scholarship
        /// </summary>
        Scholarship,
                     
         /// <summary>
        /// work
        /// </summary>
        Work,
    }
} 


