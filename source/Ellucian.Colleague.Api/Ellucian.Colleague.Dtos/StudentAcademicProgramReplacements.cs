﻿//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The academic programs in the home institution in which a student has beed enrolled. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentAcademicProgramReplacements : BaseModel2
    {
        /// <summary>
        /// The student who is enrolled in an academic program.
        /// </summary>
        [JsonProperty("student", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Student { get; set; }

        /// <summary>
        /// The academic program the student is replacing.
        /// </summary>
        [JsonProperty("programToReplace", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 ProgramToReplace { get; set; }

        /// <summary>
        /// The student's new academic program.
        /// </summary>
        [JsonProperty("newProgram", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public StudentAcademicProgramsReplacementsNewProgram NewProgram { get; set; }
    }
}