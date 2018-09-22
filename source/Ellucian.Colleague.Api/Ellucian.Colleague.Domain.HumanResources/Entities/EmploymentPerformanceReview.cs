//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// EmploymentPerformanceReview
    /// </summary>
    [Serializable]
    public class EmploymentPerformanceReview
    {
        /// <summary>
        /// The global identifier for the employee
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// The PERSON Id
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// The JOB Id
        /// </summary>
        public string PerposId { get; private set; }

		/// <summary>
        /// The date by which the review must be completed.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// The date by which the review was completed.
        /// </summary>
        public DateTime? CompletedDate { get; private set; }

        /// <summary>
        /// The TYPE Id
        /// </summary>
        public string RatingCycleCode { get; private set; }

        /// <summary>
        /// The REVIEWEDBY Id
        /// </summary>
        public string ReviewedById { get; set; }

        /// <summary>
        /// The RATING Id
        /// </summary>
        public string RatingCode { get; private set; }

        /// <summary>
        /// The COMMENT
        /// </summary>
        public string Comment { get; set; }

		/// <summary>
        /// Initializes a new instance of the <see cref="EmploymentPerformanceReview"/> class.
        /// </summary>
        /// <param name="guid">The global identifier for the employee record</param>
        /// <param name="personId">The Colleague PERSON id of the person</param>
        /// <param name="perposId">The Colleague JOB id of the person</param>
        /// <param name="completedDate">The Colleague complete date of the review</param>
        /// <param name="ratingCycleCode">The Colleague TYPE id of the review</param>
        /// <param name="ratingCode">The Colleague RATING id of the review</param>
        public EmploymentPerformanceReview(string guid, string personId, string perposId, DateTime? completedDate, string ratingCycleCode, string ratingCode)
        { 
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(perposId))
            {
                throw new ArgumentNullException("perposId");
            }
            if (completedDate == null)
            {
                throw new ArgumentNullException("completedDate");
            }
            if (string.IsNullOrEmpty(ratingCycleCode))
            {
                throw new ArgumentNullException("ratingCycleCode");
            }
            if (string.IsNullOrEmpty(ratingCode))
            {
                throw new ArgumentNullException("ratingCode");
            }

            Guid = guid;
            PersonId = personId;
            PerposId = perposId;
            CompletedDate = completedDate;
            RatingCycleCode = ratingCycleCode;
            RatingCode = ratingCode;
        }

        /// <summary>
        /// Two reviews are equal when their Ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var employmentPerformanceReview = obj as EmploymentPerformanceReview;

            return employmentPerformanceReview.Guid == Guid;
        }

        /// <summary>
        /// Hashcode representation of object (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        /// <summary>
        /// String representation of object (Id)
        /// </summary>
        /// <returns>Global Identifier</returns>
        public override string ToString()
        {
            return Guid;
        }
    }
}