// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Interfaces;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The override rate for the meal plan assignment.
    /// </summary>
    [DataContract]
    public class StudentMealPlansConsumption
    {

        /// <summary>
        /// The meal plan units used
        /// </summary>
        [DataMember(Name = "units", EmitDefaultValue = false)]
        public int? Units { get; set; }

        /// <summary>
        /// The meal plan percentage used
        /// </summary>
        [DataMember(Name = "percent", EmitDefaultValue = false)]
        public int? Percent { get; set; }

    }
}