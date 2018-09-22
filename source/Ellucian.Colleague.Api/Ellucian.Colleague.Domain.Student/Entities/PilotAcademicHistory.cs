// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class PilotAcademicHistory
    {
        private List<AcademicTerm> _AcademicTerms = new List<AcademicTerm>();
        public List<AcademicTerm> AcademicTerms { get { return _AcademicTerms; } }

        private List<PilotAcademicCredit> _NonTermAcademicCredits = new List<PilotAcademicCredit>();
        public List<PilotAcademicCredit> NonTermAcademicCredits { get { return _NonTermAcademicCredits; } }

        // Public property so that it can be reset in the case of an advisor accessing a student's history.
        public GradeRestriction GradeRestriction { get; set; }
        public decimal TotalCreditsCompleted { get; set; }
        public decimal? OverallGradePointAverage { get; set; }
        // Add Transfer GPA and credits
        public decimal TotalTransferCreditsCompleted { get; set; }
        public decimal? OverallTransferGradePointAverage { get; set; }

        public string StudentId { get; set; }

        /// <summary>
        /// First Term Enrolled at this level with "active" status where active means:
        /// - "1" or "2" special processing in student acad cred statuses
        /// - before most recent census date from section, term location, or term
        /// </summary>
        public string FirstTermEnrolled { get; set; }

        public PilotAcademicHistory(IEnumerable<PilotAcademicCredit> credits, GradeRestriction studentGradeRestriction, string firstTermEnrolled)
        {
            FirstTermEnrolled = firstTermEnrolled;
            GradeRestriction = studentGradeRestriction;
            if (credits == null)
            {
                throw new ArgumentNullException("credits");
            }
            if (studentGradeRestriction == null)
            {
                throw new ArgumentNullException("studentGradeRestriction");
            }

            // Flag replacements. Each replaced course has a list of courses involved in its replacement. Any item in the list that
            // is not "replaced" is a "replacement".
            foreach (var credit in credits.Where(c => c.ReplacedStatus == ReplacedStatus.Replaced))
            {
                foreach (var repeatCreditId in credit.RepeatAcademicCreditIds)
                {
                    var repeatCredit = credits.Where(c => c.Id == repeatCreditId).FirstOrDefault();
                    if (repeatCredit != null)
                    {
                        // If item has not already been marked as replaced, then it's the replacement.
                        if (repeatCredit.ReplacedStatus != ReplacedStatus.Replaced)
                            repeatCredit.ReplacementStatus = ReplacementStatus.Replacement;
                    }
                }
            }

            // Flag possible future replaced/replacements. Start with all course-based credits that are not marked replaced and can be replaced and are completed
            foreach (var credit in credits.Where(c => !(c.ReplacedStatus == ReplacedStatus.Replaced) && c.CanBeReplaced && c.IsCompletedCredit && c.Course != null))
            {
                // Find all course-based credits that are not completed for that same course or an equated course, and pick the one with the earliest start date
                var replacementCredit = credits.Where(c => c.Id != credit.Id && !c.IsCompletedCredit && c.Course != null && (c.Course.Id == credit.Course.Id || credit.Course.EquatedCourseIds.Contains(c.Course.Id))).OrderBy(c => c.StartDate).FirstOrDefault();
                if (replacementCredit != null)
                {
                    replacementCredit.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                    credit.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                }
            }
            // Removed all filtering here because filtering is already done on all the
            // credits coming into this method.
            // SRM - 04/15/2014  ESS Project

            if (credits != null && credits.Count() > 0)
            {
                // Added fields to track totals SRM 11/12/2013
                decimal totalGpaGradePoints = 0;
                decimal totalGpaCredits = 0;
                decimal totalCreditsCompleted = 0;
                decimal overallGpa = 0;

                decimal totalTransferGpaGradePoints = 0;
                decimal totalTransferGpaCredits = 0;
                decimal totalTransferCreditsCompleted = 0;
                decimal overallTransferGpa = 0;

                var terms = (from credit in credits select credit.TermCode).Distinct();
                foreach (var term in terms)
                {
                    decimal termGpaGradePoints = 0;
                    decimal termGpaCredits = 0;
                    decimal termCredits = 0;
                    decimal termCeus = 0;

                    decimal transferGpaGradePoints = 0;
                    decimal transferGpaCredits = 0;
                    decimal transferCredits = 0;

                    IEnumerable<PilotAcademicCredit> creds = null;
                    if (string.IsNullOrEmpty(term))
                    {
                        // No need to calculate GPA 
                        creds = credits.Where(c => c.TermCode == null || c.TermCode == "");
                        foreach (var cred in creds)
                        {
                            if (studentGradeRestriction.IsRestricted)
                            {
                                // If the student is restricted from seeing their grades. Don't pass any of this back with the academic history.
                                cred.VerifiedGradeId = null;
                                cred.GradeSchemeCode = null;
                                cred.GradePoints = 0;
                                cred.GpaCredit = 0;
                            }

                            _NonTermAcademicCredits.Add(cred);
                            if (string.IsNullOrEmpty(StudentId))
                            {
                                StudentId = cred.StudentId;
                            }
                            // Calcluate Transfer Credits
                            if (cred.IsTransfer == true)
                            {
                                transferGpaCredits = transferGpaCredits + cred.CumulativeGpaCredit;
                                transferGpaGradePoints = transferGpaGradePoints + cred.CumulativeGradePoints;
                                transferCredits = transferCredits + cred.CumulativeCompletedCredit;
                            }
                        }
                        // Added Totals for GPA and total completed Credits SRM 11/12/2013
                        totalGpaCredits = totalGpaCredits + creds.Sum(ac => ac.CumulativeGpaCredit);
                        totalGpaGradePoints = totalGpaGradePoints + creds.Sum(agp => agp.CumulativeGradePoints);
                        // Changed to completed credits SRM 04/15/2014
                        totalCreditsCompleted = totalCreditsCompleted + creds.Sum(tc => tc.CumulativeCompletedCredit);
                        // Update totals for Transfer Credits and GPA
                        totalTransferGpaCredits = totalTransferGpaCredits + transferGpaCredits;
                        totalTransferGpaGradePoints = totalTransferGpaGradePoints + transferGpaGradePoints;
                        totalTransferCreditsCompleted = totalTransferCreditsCompleted + transferCredits;
                    }
                    else
                    {
                        creds = credits.Where(c => c.TermCode != null && c.TermCode == term);
                        List<PilotAcademicCredit> credList = new List<PilotAcademicCredit>();
                        foreach (var cred in creds)
                        {
                            if (studentGradeRestriction.IsRestricted)
                            {
                                // If the student is restricted from seeing their grades. Don't pass any of this back with the academic history.
                                // By setting the GpaCredit to zero the GPA will also end up as 0.
                                cred.VerifiedGradeId = null;
                                cred.GradeSchemeCode = null;
                                cred.GradePoints = 0;
                                cred.GpaCredit = 0;
                            }
                            credList.Add(cred);
                            if (string.IsNullOrEmpty(StudentId))
                            {
                                StudentId = cred.StudentId;
                            }
                            // Calcluate Transfer Credits
                            if (cred.IsTransfer == true)
                            {
                                transferGpaCredits = transferGpaCredits + cred.CumulativeGpaCredit;
                                transferGpaGradePoints = transferGpaGradePoints + cred.CumulativeGradePoints;
                                transferCredits = transferCredits + cred.CumulativeCompletedCredit;
                            }
                        }
                        // For term GPA we want the sum of all credits values to get the gpa. The credits are already filtered by term by this point.
                        termGpaCredits = creds.Sum(ac => ac.GpaCredit);
                        termGpaGradePoints = creds.Sum(agp => agp.GradePoints);
                        termCredits = creds.Sum(tc => tc.CompletedCredit);
                        termCeus = creds.Sum(c => c.ContinuingEducationUnits);
                        AcademicTerm tgpa = new AcademicTerm() { GradePointAverage = (termGpaCredits == 0 ? 0 : (termGpaGradePoints / termGpaCredits)), TermId = term, Credits = termCredits, ContinuingEducationUnits = termCeus };
                        if (termGpaCredits == 0)
                            tgpa.GradePointAverage = null;
                        _AcademicTerms.Add(tgpa);

                        // For overall GPA we don't want to include replaced credits.
                        var overallGpaCredits = creds.Sum(ac => ac.CumulativeGpaCredit);
                        var overallGpaGradePoints = creds.Sum(agp => agp.CumulativeGradePoints);
                        var overallTermCredits = creds.Sum(tc => tc.CumulativeCompletedCredit);

                        totalGpaCredits = totalGpaCredits + overallGpaCredits;
                        totalGpaGradePoints = totalGpaGradePoints + overallGpaGradePoints;
                        totalCreditsCompleted = totalCreditsCompleted + overallTermCredits;

                        // Update totals for Transfer Credits and GPA
                        totalTransferGpaCredits = totalTransferGpaCredits + transferGpaCredits;
                        totalTransferGpaGradePoints = totalTransferGpaGradePoints + transferGpaGradePoints;
                        totalTransferCreditsCompleted = totalTransferCreditsCompleted + transferCredits;
                    }
                }
                overallGpa = (totalGpaCredits == 0 ? 0 : (totalGpaGradePoints / totalGpaCredits));
                TotalCreditsCompleted = totalCreditsCompleted;
                OverallGradePointAverage = overallGpa;
                if (totalGpaCredits == 0 && overallGpa == 0)
                    OverallGradePointAverage = null;
                // Calculate transfer Credit Totals
                overallTransferGpa = (totalTransferGpaCredits == 0 ? 0 : (totalTransferGpaGradePoints / totalTransferGpaCredits));
                TotalTransferCreditsCompleted = totalTransferCreditsCompleted;
                OverallTransferGradePointAverage = overallTransferGpa;
                if (totalTransferGpaCredits == 0 && OverallTransferGradePointAverage == 0)
                    OverallTransferGradePointAverage = null;
            }
        }

        public static PilotAcademicHistory FromAcademicHistory(AcademicHistory academicHistory)
        {
            List<PilotAcademicCredit> credits = new List<PilotAcademicCredit>();
            foreach (var term in academicHistory.AcademicTerms)
            {
                foreach (var cred in term.AcademicCredits)
                {
                    credits.Add(new PilotAcademicCredit(cred));
                }
            }
            foreach (var nonTermCred in academicHistory.NonTermAcademicCredits)
            {
                credits.Add(new PilotAcademicCredit(nonTermCred));
            }
            return new PilotAcademicHistory(credits, academicHistory.GradeRestriction, academicHistory.FirstTermEnrolled);
        }

        /// <summary>
        /// Method to filter out all but the single term passed into this method
        /// </summary>
        /// <param name="academicHistory">Academic History Entity Object before filter</param>
        /// <param name="term">Term to filter all term data to only this term</param>
        /// <returns>Academic History Entity Object containing only the requested term</returns>
        public void FilterTerm(string term)
        {
            if (!string.IsNullOrEmpty(term))
            {
                List<AcademicTerm> newAcademicTerms = new List<AcademicTerm>();
                newAcademicTerms = _AcademicTerms.Where(a => a.TermId != null && a.TermId == term).ToList();
                _AcademicTerms = newAcademicTerms;
            }
        }
    }
}
