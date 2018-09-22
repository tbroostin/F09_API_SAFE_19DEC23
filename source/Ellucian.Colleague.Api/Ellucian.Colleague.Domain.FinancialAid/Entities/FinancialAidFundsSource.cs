// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;


namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{   
   /// <summary>
    /// The source of the financial aid fund (federal, state, institutional, other).
    /// </summary>
    [Serializable]
    public enum FinancialAidFundsSource
    {     
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
       
                           
         /// <summary>
        /// federal
        /// </summary>
        Federal,
                     
         /// <summary>
        /// institutional
        /// </summary>
        Institutional,
                     
         /// <summary>
        /// state
        /// </summary>
        State,
                     
         /// <summary>
        /// other
        /// </summary>
        Other,
    }
} 


