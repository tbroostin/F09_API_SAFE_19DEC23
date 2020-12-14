// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The literal value or numeric range of the grade.
    /// </summary>
    [DataContract]
    public class GradeItem
    {
        /// <summary>
        /// The type of the grade.
        /// There is only one value in GradeItemType and it's "literal" and all
        /// grades have this default.  It is the default value therefore,
        /// never insert "EmitDefaultValue = false" into the DataMember.
        /// </summary>
        [DataMember(Name = "type")]
        public GradeItemType GradeItemType { get; set; }
        
        /// <summary>
        /// The literal value of the grade.
        /// </summary>
        [DataMember(Name = "value", EmitDefaultValue = false)]
        public string GradeValue { get; set; }
    }
}
