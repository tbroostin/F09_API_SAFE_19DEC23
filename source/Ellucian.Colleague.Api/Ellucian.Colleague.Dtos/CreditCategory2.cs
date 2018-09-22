// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A category of academic credit.
    /// </summary>
    [DataContract]
    public class CreditCategory2 : CodeItem2
    {
        /// <summary>
        /// The higher-level category of academic credits
        /// </summary>
        [DataMember(Name = "creditType")]
        public CreditCategoryType2? CreditType { get; set; }
    }
}
