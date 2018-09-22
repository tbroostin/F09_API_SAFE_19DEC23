// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public abstract class BaseResult
    {

        public abstract decimal GetCompletedCredits();
        public abstract decimal GetAppliedCredits();
        public abstract decimal GetPlannedAppliedCredits();
        public abstract bool IsSatisfied();
        public abstract bool IsPlannedSatisfied();
        public CompletionStatus CompletionStatus;
        public PlanningStatus PlanningStatus;
        public abstract IEnumerable<AcadResult> GetCreditsToIncludeInGpa();

        protected BaseResult()
        {
            CompletionStatus = CompletionStatus.NotStarted;
            PlanningStatus = PlanningStatus.NotPlanned;
        }

        /// <summary>
        ///  Calculates GPA (Grade Point Average): Grade points divided by credits
        /// </summary>
        /// <param name="gradePoints">Decimal quality points</param>
        /// <param name="gpaCredits">Decimal gpa credits</param>
        /// <returns>Decimal GPA (Grade Point Average)</returns>
        public static decimal? CalculateGpa(decimal gradePoints, decimal credits)
        {
             if (credits == 0)
                  return null;
             else
                  return gradePoints / credits;
        }

        /// <summary>
        /// Returns the Gpa of the credits applied
        /// </summary>
        public decimal? Gpa
        {
            get
            {
                var acadResults = GetCreditsToIncludeInGpa().Distinct<AcadResult>();
                return CalculateGpa(GetGradePoints(acadResults), GetGpaCredits(acadResults));
            }
        }

        /// <summary>
        /// Returns the Gpa of the institutional credits applied
        /// </summary>
        public decimal? InstGpa
        {
            get
            {
                var acadResults = GetCreditsToIncludeInGpa().Distinct<AcadResult>();
                return CalculateGpa(GetInstGradePoints(acadResults), GetInstGpaCredits(acadResults));
            }
        }

        /// <summary>
        /// Returns the sum of grade points from the given academic credits
        /// </summary>
        public decimal GetGradePoints(IEnumerable<AcadResult> acadResults)
        {
            return acadResults.Sum(cr => cr.GetGradePoints());
        }

        /// <summary>
        /// Returns the sum of Gpa credits from the given academic credits
        /// </summary>
        public decimal GetGpaCredits(IEnumerable<AcadResult> acadResults)
        {
            return acadResults.Sum(cr => cr.GetGpaCredit());
        }

        /// <summary>
        /// Returns the sum of Institutional grade points from the given list of academic credits.
        /// </summary>
        public decimal GetInstGradePoints(IEnumerable<AcadResult> acadResults)
        {
            return acadResults.Where(cr => cr.IsInstitutional()).Sum(cr => cr.GetGradePoints());
        }

        /// <summary>
        /// Returns the sum of Institutional Gpa credits from the list of academic credits
        /// </summary>
        public decimal GetInstGpaCredits(IEnumerable<AcadResult> acadResults)
        {
            return acadResults.Where(cr => cr.IsInstitutional()).Sum(cr => cr.GetGpaCredit());
        }
    }
    /// <summary>
    /// Is this requirement element (Program, Requirement, SubRequirement, or Group) complete?
    /// </summary>
    [Serializable]
    public enum CompletionStatus
    {
        NotStarted, PartiallyCompleted, Completed, Waived
    }
    /// <summary>
    /// Is this requirement element planned out?  
    /// </summary>
    [Serializable]
    public enum PlanningStatus
    {
        NotPlanned, PartiallyPlanned, CompletelyPlanned
    }
}