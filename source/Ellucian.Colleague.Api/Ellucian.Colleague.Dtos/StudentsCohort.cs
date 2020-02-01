// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The type of students cohort.
    /// </summary>
    [DataContract]
    public class StudentCohort : CodeItem2
    {
        /// <summary>
        /// Student Cohort Type.
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public CohortType StudentCohortType { get; set; }
    }
}