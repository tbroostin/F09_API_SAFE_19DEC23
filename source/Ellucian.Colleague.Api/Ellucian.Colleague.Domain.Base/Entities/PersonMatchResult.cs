// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A single result from attempting to match a person based on criteria
    /// </summary>
    [Serializable]
    public class PersonMatchResult
    {
        /// <summary>
        /// The identifier of the person who might be a match
        /// </summary>
        public string PersonId { get; private set; }
        /// <summary>
        /// The score of the person based on the search criteria
        /// </summary>
        public int MatchScore { get; private set; }
        /// <summary>
        /// A <cref>PersonMatchCategoryType</cref> value indicating how well the person matches the criteria
        /// </summary>
        public PersonMatchCategoryType MatchCategory { get; private set;}

        /// <summary>
        /// Constructor for a <cref>PersonMatchResult</cref>
        /// </summary>
        /// <param name="personId">the id of the person who might be a match</param>
        /// <param name="score">the score of the person based on the search criteria</param>
        /// <param name="category">A code representing the match category</param>
        public PersonMatchResult(string personId, int? score, string category)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentNullException("category");
            }
            if (!"pPdD".Contains(category))
            {
                throw new ArgumentOutOfRangeException("category");
            }

            PersonId = personId;
            MatchScore = score ?? default(int);
            MatchCategory = "pP".Contains(category)?PersonMatchCategoryType.Potential:PersonMatchCategoryType.Definite;
        }
    }
}
