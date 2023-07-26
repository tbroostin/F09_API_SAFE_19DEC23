using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Parent details like LastName, FirstName initial, BirthDate, SsnOrTin, Educational level
    /// </summary>
    public class ParentDetails
    {
        /// <summary>
        /// <see cref="AidApplicationsParentEdLevel"/> Parent's highest grade level completed
        /// </summary>
        [JsonProperty("educationalLevel", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "Parents' highest grade level completed.", DataMaxLength = 25)]
        public AidApplicationsParentEdLevel? EducationalLevel { get; set; }

        /// <summary>
        /// Parent's social security number or ITIN
        /// </summary>
        [JsonProperty("ssnOrItin", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "Parents' Social Security Number or ITIN.", DataMaxLength = 9)]
        public int? SsnOrItin { get; set; }

        /// <summary>
        /// Last name of parent
        /// </summary>
        [JsonProperty("lastName", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "Last name of Parent.", DataMaxLength = 16)]
        public string LastName { get; set; }

        /// <summary>
        /// The initial letter of the first name of parent
        /// </summary>
        [JsonProperty("firstInitial", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "The initial letter of the first name of Parent.", DataMaxLength = 1)]
        public string FirstInitial { get; set; }

        /// <summary>
        /// Parent date of birth
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("birthDate", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata(DataDescription = "Parents' date of birth.", DataMaxLength = 10)]
        public DateTime? BirthDate { get; set; }

    }
}
