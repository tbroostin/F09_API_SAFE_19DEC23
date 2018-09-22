// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// AcademicPeriod DTO property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CreditCategory2DtoProperty : BaseCodeTitleDetailDtoProperty
    {
        /// <summary>
        /// The higher-level category of academic credits
        /// </summary>
        [JsonProperty("creditType")]
        public CreditCategoryType3? CreditType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public CreditCategory2DtoProperty() : base() { }
    }
}

