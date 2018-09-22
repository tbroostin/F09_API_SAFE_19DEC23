// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    ///  A catalog listing of the courses offered by an organization.
    /// </summary>
    [DataContract]
    public class AcademicCatalog2 : CodeItem2
    {   
        /// <summary>
        /// The beginning date for when an academic catalog takes effect
        /// </summary>
        [DataMember(Name = "startOn", EmitDefaultValue = false)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The ending date for when an academic catalog is no longer in effect
        /// </summary>
        [DataMember(Name = "endOn", EmitDefaultValue = false)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The educational institution that owns the academic catalog
        /// </summary>
        [DataMember(Name = "institution", EmitDefaultValue = false)]
        public GuidObject2 Institution { get; set; }

        /// <summary>
        /// Academic programs that make up the catalog.
        /// </summary>
        [DataMember(Name = "academicPrograms")]
        public List<GuidObject2> AcademicPrograms { get; set; }

        /// <summary>
        /// A status of an academic catalog
        /// </summary>
        [DataMember(Name = "status")]
        public LifeCycleStatus status { get; set;  }
    }
}