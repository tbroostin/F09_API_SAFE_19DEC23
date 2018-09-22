// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The supervisor associated with the job.
    /// </summary>
    [DataContract]
    public class SupervisorsDtoProperty
    {
        /// <summary>
        ///  The supervisor for the job.
        /// </summary>
        [DataMember(Name = "supervisor", EmitDefaultValue = false)]
        public GuidObject2 Supervisor { get; set; }

        /// <summary>
        /// The type of supervisor (e.g. primary, alternate, etc.)
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public PositionReportsToType Type { get; set; }
    }
}