// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{   
   /// <summary>
    /// The privacy level of the financial aid fund based on privacy concerns (restricted, non-restricted). This indicates whether the award of this fund to a student is restricted or not for view.
    /// </summary>
    [Serializable]
    public enum FinancialAidFundsPrivacy
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
       
                           
         /// <summary>
        /// restricted
        /// </summary>
        Restricted,
                     
         /// <summary>
        /// nonRestricted
        /// </summary>
        Nonrestricted,
    }
} 


