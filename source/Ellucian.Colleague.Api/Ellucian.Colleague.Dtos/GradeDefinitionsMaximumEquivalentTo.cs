// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    ///  A list of mappings to equivalent grades from other grade schemes
    /// </summary>
    [DataContract]
    public class GradeDefinitionsMaximumEquivalentTo 
    {
        /// <summary>
        /// A named grouping of grades that can be assigned to students at a given academic level
        /// </summary>
        [DataMember(Name = "scheme")]
        public GradeSchemeProperty GradeScheme { get; set; }

        /// <summary>
        /// The literal value or numeric range of the grade
        /// </summary>
        [DataMember(Name = "grade")]
        public GradeItem GradeItem { get; set; }

        /// <summary>
        /// What degree of credit this grade qualifies for.
        /// </summary>
        [DataMember(Name = "credit")]
        public GradeCmplCreditType GradeCmplCreditType { get; set; }

        /// <summary>
        /// A grade scheme item in different grade schemes that is the equivalent to this grade scheme item.
        /// </summary>
        [DataMember(Name = "detail")]
        public GuidObject2 Detail { get; set; }
       
    }
}