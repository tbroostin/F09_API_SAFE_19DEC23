// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The quantity and interval
    /// </summary>
    [DataContract]
    public class QuantityIntervalDtoProperty
    {
        /// <summary>
        /// The quantity.
        /// </summary>
        [DataMember(Name = "quantity", EmitDefaultValue = false)]
        public decimal? Quantity { get; set; }
        
        /// <summary>
        /// The time interval 
        /// </summary>
        [DataMember(Name = "interval", EmitDefaultValue = false)]
        public IntervalTypes? Interval { get; set; }
    }
}