// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// This is DTO for student aptitude assessments v11
    /// </summary>
    [DataContract]
    public class StudentAptitudeAssessmentsFilter : BaseModel2
    {
        //to support old filters
        /// <summary>
        ///title
        /// </summary>
        [DataMember(Name = "student")]
        [FilterProperty("criteria")]
        public string Student { get; set; }
    }
}