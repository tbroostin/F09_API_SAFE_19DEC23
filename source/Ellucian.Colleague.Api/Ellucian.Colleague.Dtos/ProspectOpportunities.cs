//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The opportunities for a prospective student at an institution. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ProspectOpportunities : BaseModel2
    {
        /// <summary>
        /// The person who is interested in attending the institution.
        /// </summary>

        [JsonProperty("prospect", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 Prospect { get; set; }

        /// <summary>
        /// The person who is interested in attending the institution and the academic program they are considering at the institution.
        /// </summary>

        [JsonProperty("recruitAcademicPrograms", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<GuidObject2> RecruitAcademicPrograms { get; set; }

        /// <summary>
        /// The academic period the prospect is interested in beginning attendance at the institution.
        /// </summary>

        [JsonProperty("entryAcademicPeriod", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public GuidObject2 EntryAcademicPeriod { get; set; }

        /// <summary>
        /// The admission population the prospect would be included within at the institution if they attend the institution.
        /// </summary>

        [JsonProperty("admissionPopulation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 AdmissionPopulation { get; set; }

        /// <summary>
        /// The institution site (campus) the prospect is considering.
        /// </summary>

        [JsonProperty("site", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Site { get; set; }

        /// <summary>
        /// The education goal associated with the prospect opportunity
        /// </summary>
        [JsonProperty("educationalGoal", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 EducationalGoal { get; set; }

        /// <summary>
        /// The career goals associated with the prospect opportunity
        /// </summary>
        [JsonProperty("careerGoals", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<GuidObject2> CareerGoals { get; set; }
    }
}