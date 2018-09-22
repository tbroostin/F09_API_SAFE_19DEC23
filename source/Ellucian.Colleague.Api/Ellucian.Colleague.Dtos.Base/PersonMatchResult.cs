// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A result of the person matching algorithm
    /// </summary>
    public class PersonMatchResult
    {
        /// <summary>
        /// The identifier of the person who might be a match
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// The score of the person based on the search criteria
        /// </summary>
        public int MatchScore { get; set; }
        /// <summary>
        /// A <cref>PersonMatchCategoryType</cref> value indicating how well the person matches the criteria
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PersonMatchCategoryType MatchCategory { get; set; }
    }
}
