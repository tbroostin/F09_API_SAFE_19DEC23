// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Result of a person biographic/demographic matching inquiry for Instant Enrollment
    /// </summary>
    [Serializable]
    public class InstantEnrollmentPersonMatchResult
    {
        /// <summary>
        /// ID of an existing person who matches the submitted biographic and demographic information
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Flag indicating whether or not the submitted biographic and demographic information potentially matches one or more existing persons
        /// </summary>
        public bool HasPotentialMatches { get; set; }

        /// <summary>
        /// Flag indicating that the government ID provided in the person biographic/demographic matching inquiry for Instant Enrollment belongs to an existing user who was not identified as a potential or definite match
        /// </summary>
        public bool DuplicateGovernmentIdFound { get; set; }

        /// <summary>
        /// Creates a new <see cref="InstantEnrollmentPersonMatchResult"/>
        /// </summary>
        /// <param name="personMatchResults">Collection of <see cref="PersonMatchResult"/></param>
        /// <param name="duplicateGovernmentIdFound">Flag indicating that the government ID provided in the person biographic/demographic matching inquiry for Instant Enrollment belongs to an existing user who was not identified as a potential or definite match</param>
        public InstantEnrollmentPersonMatchResult(IEnumerable<PersonMatchResult> personMatchResults, bool duplicateGovernmentIdFound)
        {
            if (personMatchResults != null)
            {
                // Remove any nulls
                personMatchResults = personMatchResults.Where(pmr => pmr != null).ToList();
                if (personMatchResults.Any())
                {
                    // If there is 1 definite match, use the Person ID from that match
                    if (personMatchResults.Count() == 1 && personMatchResults.ElementAt(0).MatchCategory == PersonMatchCategoryType.Definite)
                    {
                        PersonId = personMatchResults.ElementAt(0).PersonId;
                    }
                    // If there is more than 1 match, either possible or definite, or at least 1 non-definite match, then there are potential matches
                    else
                    {
                        HasPotentialMatches = true;
                    }
                }
            }
            DuplicateGovernmentIdFound = duplicateGovernmentIdFound;
        }
    }
}

