// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// AcademicPeriod DTO property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CreditCategoryDtoProperty : BaseCodeTitleDetailDtoProperty
    {
        /// <summary>
        /// The higher-level category of academic credits
        /// </summary>
        [JsonProperty("creditType")]
        public CreditCategoryType2? CreditType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public CreditCategoryDtoProperty() : base() { }
    }
}

