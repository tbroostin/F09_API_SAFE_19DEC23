// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A category of academic credit.
    /// </summary>
    [DataContract]
    public class CreditCategory3 : CodeItem2
    {
        /// <summary>
        /// The higher-level category of academic credits
        /// </summary>
        [DataMember(Name = "creditType")]
        public CreditCategoryType3? CreditType { get; set; }
    }
}
