// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    ///"A designation of the level of instruction of a course or a program of study.
    /// </summary>
    [DataContract]
    public class AcademicLevelProperty
    {
        /// <summary>
        /// The code that identifies the academic level (e.g. 'UG')
        /// </summary>
        [DataMember(Name = "code", EmitDefaultValue = false)]
        public string Code { get; set; }

        /// <summary>
        /// The full name of the academic level (e.g. 'Undergraduate')
        /// </summary>
        [DataMember(Name = "title", EmitDefaultValue = false)]
        public string Title { get; set; }

        /// <summary>
        /// A designation of the level of instruction of a course or a program of study.
        /// The <see cref="GuidObject2">guid</see> for the academic level
        /// </summary>
        [DataMember(Name = "detail", EmitDefaultValue = false)]
        public GuidObject2 Detail { get; set; }

    }
}