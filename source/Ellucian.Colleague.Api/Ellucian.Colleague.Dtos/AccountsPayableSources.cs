// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;


namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The payment sources associated with this vendor. 
    /// </summary>
    [DataContract]
    public class AccountsPayableSources : CodeItem2
    {
        /// <summary>
        /// The direct deposit status for the account payable source.
        /// </summary>
        [DataMember(Name = "directDeposit", EmitDefaultValue = false)]
        public DirectDeposit DirectDeposit { get; set; }


    }
}
