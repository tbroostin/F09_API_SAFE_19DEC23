using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Contains the basic information of student's parents
    /// </summary>
    public class ParentsInfo
    {
        /// <summary>
        /// <see cref="ParentDetails"/> Details of first parent
        /// </summary>  
        [JsonProperty("firstParent", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ParentDetails FirstParent { get; set; }

        /// <summary>
        /// <see cref="ParentDetails"/> Details of second parent
        /// </summary>  
        [JsonProperty("secondParent", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ParentDetails SecondParent { get; set; }

        /// <summary>
        /// <see cref="ParentMaritalInfo"> Parent's marital status and marital status date </see> object
        /// </summary>
        [JsonProperty("parentMarital", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ParentMaritalInfo ParentMarital { get; set; }

        /// <summary>
        /// Parent email address
        /// </summary>
        [JsonProperty("emailAddress", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.PARENT.EMAIL", false, DataDescription = "Parents' email address.")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// <see cref="LegalResidence"/> Parent's legal residence
        /// </summary>
        [JsonProperty("legalResidence", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public LegalResidence ParentLegalResidence { get; set; }

        /// <summary>
        /// Parent's number of family members
        /// </summary>
        [JsonProperty("numberInFamily", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.P.NBR.FAMILY", false, DataDescription = "Parents' number of family members.")]
        public int? NumberInFamily { get; set; }

        /// <summary>
        /// Parent's number in college
        /// </summary>
        [JsonProperty("numberInCollege", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.P.NBR.COLLEGE", false, DataDescription = "Parents' number in college.")]
        public int? NumberInCollege { get; set; }

        /// <summary>
        /// <see cref="ParentsIncome"/> Parent's income details
        /// </summary>
        [JsonProperty("income", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ParentsIncome Income { get; set; }

    }
}