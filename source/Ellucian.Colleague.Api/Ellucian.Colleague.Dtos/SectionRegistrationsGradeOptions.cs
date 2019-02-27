//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The grading options for the section registration. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SectionRegistrationsGradeOptions : BaseModel2
    {    
        /// <summary>
       /// An instance of a course for which a person has registered.
       /// </summary>
         
       [JsonProperty("section", DefaultValueHandling = DefaultValueHandling.Ignore)]
       [FilterProperty("criteria")]
        public GuidObject2 Section { get; set; }
     
        /// <summary>
       /// An indicator whether the section is gradable.
       /// </summary>
         
       [JsonProperty("sectionGradability", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public SectionRegistrationsGradeOptionsSectionGradability SectionGradability { get; set; }
     
        /// <summary>
       /// The grading scheme used to award the student their grade for the section.
       /// </summary>
         
       [JsonProperty("studentGradeScheme", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public StudentGradeSchemeDtoProperty StudentGradeScheme { get; set; }
     
        /// <summary>
       /// An indicator whether the student has a verified grade or the section registration.
       /// </summary>
         
       [JsonProperty("gradeStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public SectionRegistrationsGradeOptionsGradeStatus? GradeStatus { get; set; }
     
        /// <summary>
       /// The grade options associated with the section registration.
       /// </summary>
         
       [JsonProperty("grades", DefaultValueHandling = DefaultValueHandling.Ignore)]
       public List<SectionRegistrationsGradeOptionsGrades> Grades { get; set; }         
     }
}