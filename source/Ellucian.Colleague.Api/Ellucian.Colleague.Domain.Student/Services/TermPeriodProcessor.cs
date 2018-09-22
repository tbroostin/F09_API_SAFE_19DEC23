// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Services
{
    /// <summary>
    /// Class to perform term-related business logic
    /// </summary>
    public static class TermPeriodProcessor
    {
        /// <summary>
        /// Gets the sort order for a term by its ID
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="terms">Collection of terms</param>
        /// <returns>Sort order for the term</returns>
        public static string GetTermSortOrder(string termId, IEnumerable<Term> terms)
        {
            string order = String.Empty;
            var term = terms.Where(t => t.Code == termId).FirstOrDefault();
            if (term == null)
            {
                // Sort non-term entries to the end
                order += DateTime.MaxValue.ToString("s");           // Max date value
                order += "999";                                     // Max sequence number
                order += DateTime.MaxValue.ToString("s");           // Max date value
                order += "zzzzzzz";                                 // Max code value
            }
            else
            {
                order += term.StartDate.ToString("s");              // First is start date
                order += term.Sequence.ToString().PadLeft(3, '0');  // Next is sequence number
                order += term.EndDate.ToString("s");                // End date
                order += term.Code;                                 // Term code
            }

            return order;
        }

        /// <summary>
        /// Compare two term IDs.
        /// </summary>
        /// <param name="source">Source term ID</param>
        /// <param name="compare">Comparison term ID</param>
        /// <returns>Boolean comparison value</returns>
        public static bool AreTermIdsEqual(string source, string compare)
        {
            return (source ?? String.Empty) == (compare ?? String.Empty);
        }

        /// <summary>
        /// Compare a term ID and date against those of a Past/Current/Future period
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="sourceDate">Transaction date</param>
        /// <param name="termIds">List of term IDs for a period</param>
        /// <param name="startDate">Period start date</param>
        /// <param name="endDate">Period end date</param>
        /// <returns>Boolean comparison value</returns>
        public static bool IsInPeriod(string termId, DateTime sourceDate, IEnumerable<string> termIds, DateTime? startDate, DateTime? endDate)
        {
            bool include = false;
            if (String.IsNullOrEmpty(termId))
            {
                // Source item is non-term - see if the source date is in the specified date range
                include = IsDateInRange(sourceDate, startDate, endDate);
            }
            else
            {
                // Source item is term-based - look for a matching term
                if (termIds != null && termIds.Count() > 0)
                {
                    include = termIds.Contains(termId);
                }
            }
            return include;
        }

        /// <summary>
        /// Determine whether the specified term is a reporting term
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="terms">Collection of terms</param>
        /// <returns>Boolean value</returns>
        public static bool IsReportingTerm(string termId, IEnumerable<Term> terms)
        {
            if (String.IsNullOrEmpty(termId) || terms == null || terms.Count() == 0)
            {
                return false;
            }
            // A term is a reporting term if the reporting term and the term code are the same
            return CompareTerms(termId, GetReportingTerm(termId, terms));
        }

        /// <summary>
        /// Is the specified term in the specified reporting term
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="reportingTerm">Reporting term ID</param>
        /// <param name="terms">Collection of terms</param>
        /// <returns>Boolean value</returns>
        public static bool IsInReportingTerm(string termId, string reportingTerm, IEnumerable<Term> terms)
        {
            if (string.IsNullOrEmpty(termId) || string.IsNullOrEmpty(reportingTerm) || terms == null || terms.Count() == 0)
            {
                return false;
            }
            return CompareTerms(GetReportingTerm(termId, terms), reportingTerm);
        }

        /// <summary>
        /// Compare two term codes with a null term equal to a blank term.
        /// </summary>
        /// <param name="sourceTerm">Source term code</param>
        /// <param name="compareTerm">Comparison term code</param>
        /// <returns>Boolean comparison value</returns>
        public static bool CompareTerms(string sourceTerm, string compareTerm)
        {
            return (sourceTerm ?? String.Empty) == (compareTerm ?? String.Empty);
        }

        /// <summary>
        /// Get the reporting term for a term
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="terms">Collection of terms</param>
        /// <returns>Reporting term ID</returns>
        public static string GetReportingTerm(string termId, IEnumerable<Term> terms)
        {
            var term = terms.Where(t => t.Code == termId).FirstOrDefault();
            return (term == null) ? string.Empty : term.ReportingTerm;
        }

        /// <summary>
        /// Get the IDs of terms with the specified reporting term
        /// </summary>
        /// <param name="reportingTermId">Reporting term ID</param>
        /// <param name="terms">Collection of terms</param>
        /// <returns>Collection of term IDs</returns>
        public static IEnumerable<string> GetTermIdsForReportingTerm(string reportingTermId, IEnumerable<Term> terms)
        {
            var termsInReportingTerm = new List<string>();
            if (!string.IsNullOrEmpty(reportingTermId) && terms != null)
            {
                termsInReportingTerm.AddRange(terms.Where(t => t.ReportingTerm == reportingTermId).Select(t => t.Code));
            }
            return termsInReportingTerm;
        }

        /// <summary>
        /// Get the Financial period in which a term falls.
        /// </summary>
        /// <param name="termId">Term code</param>
        /// <param name="terms">Collection of terms</param>
        /// <returns>Financial period</returns>
        public static PeriodType? GetTermPeriod(string termId, IEnumerable<Term> terms)
        {
            PeriodType? period = null;
            var term = terms.Where(t => t.Code == termId).FirstOrDefault();
            if (term != null)
            {
                period = term.FinancialPeriod;
            }

            return period;
        }

        /// <summary>
        /// Return default values for a date range and put them in order
        /// </summary>
        /// <param name="rangeStart">Start of date range</param>
        /// <param name="rangeEnd">End of date range</param>
        public static void DateRangeDefaults(ref DateTime? rangeStart, ref DateTime? rangeEnd)
        {
            // Assign default values for null range dates
            if (rangeStart == null)
            {
                rangeStart = DateTime.MinValue;
            }
            if (rangeEnd == null)
            {
                rangeEnd = DateTime.MaxValue;
            }
            // Put range dates in correct order
            if (rangeStart > rangeEnd)
            {
                var temp = rangeEnd;
                rangeEnd = rangeStart;
                rangeStart = temp;
            }
        }

        /// <summary>
        /// Evaluate whether two date ranges overlap
        /// </summary>
        /// <param name="range1Start">Range 1 start date</param>
        /// <param name="range1End">Range 1 end date</param>
        /// <param name="range2Start">Range 2 start date</param>
        /// <param name="range2End">Range 2 end date</param>
        /// <returns>Boolean comparison value</returns>
        public static bool IsRangeOverlap(DateTime? range1Start, DateTime? range1End, DateTime? range2Start, DateTime? range2End)
        {
            DateRangeDefaults(ref range1Start, ref range1End);
            DateRangeDefaults(ref range2Start, ref range2End);
            return IsDateInRange(range1Start.Value, range2Start, range2End)
                || IsDateInRange(range1End.Value, range2Start, range2End)
                || IsDateInRange(range2Start.Value, range1Start, range1End)
                || IsDateInRange(range2End.Value, range1Start, range1End);
        }

        /// <summary>
        /// Evaluates whether a date is in a specified date range
        /// </summary>
        /// <param name="date">Date to evaluate</param>
        /// <param name="rangeStart">Range start date</param>
        /// <param name="rangeEnd">Range end date</param>
        /// <returns>Boolean comparison value</returns>
        public static bool IsDateInRange(DateTime date, DateTime? rangeStart, DateTime? rangeEnd)
        {
            // Assign default values for null range dates
            DateRangeDefaults(ref rangeStart, ref rangeEnd);
            return (date.Date >= rangeStart.Value.Date) && (date.Date <= rangeEnd.Value.Date);
        }

        /// <summary>
        /// Get the term ID associated with a term description
        /// </summary>
        /// <param name="termDescription">Description of a term</param>
        /// <param name="terms">Collection of terms</param>
        /// <returns>ID for the term</returns>
        public static string GetTermIdForTermDescription(string termDescription, IEnumerable<Term> terms)
        {
            if (terms == null || terms.Count() == 0)
            {
                return termDescription;
            }
            var term = terms.Where(t => t.Description == termDescription).FirstOrDefault();
            // If no matching term based on description, check to see if supplied description is an ID
            if (term == null)
            {
                term = terms.Where(t => t.Code == termDescription).FirstOrDefault();
            }
            return term != null ? term.Code : null;
        }
    }
}
