// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AcademicHistory
    {
        private List<AcademicTerm> _AcademicTerms = new List<AcademicTerm>();
        public List<AcademicTerm> AcademicTerms { get { return _AcademicTerms; } }

        private List<AcademicCredit> _NonTermAcademicCredits = new List<AcademicCredit>();
        public List<AcademicCredit> NonTermAcademicCredits { get { return _NonTermAcademicCredits; } }

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

        public AcademicHistory(IEnumerable<AcademicCredit> credits, GradeRestriction studentGradeRestriction, string firstTermEnrolled)
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
            // Removed all filtering here because filtering is already done on all the
            // credits coming into this method.
            // SRM - 04/15/2014  ESS Project

            if (credits != null && credits.Count() > 0)
            {
                //call this method to update credits with Replaced and replacement status.
                UpdateCreditsReplaceStatus(credits);
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

                    IEnumerable<AcademicCredit> creds = null;
                    if (string.IsNullOrEmpty(term))
                    {
                        // No need to calculate GPA 
                        creds = credits.Where(c => c.TermCode == null || c.TermCode == "");
                        foreach (var cred in creds)
                        {
                            if (studentGradeRestriction.IsRestricted)
                            {
                                // If the student is restricted from seeing their grades. Don't pass any of this back with the academic history.
                                cred.VerifiedGrade = null;
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
                                transferGpaCredits = transferGpaCredits + cred.GpaCredit ?? 0m;
                                transferGpaGradePoints = transferGpaGradePoints + cred.GradePoints;
                                transferCredits = transferCredits + cred.CompletedCredit ?? 0m;
                            }
                        }
                        // Added Totals for GPA and total completed Credits SRM 11/12/2013
                        totalGpaCredits = totalGpaCredits + creds.Sum(ac => ac.GpaCredit ?? 0m);
                        totalGpaGradePoints = totalGpaGradePoints + creds.Sum(agp => agp.GradePoints);
                        // Changed to completed credits SRM 04/15/2014
                        totalCreditsCompleted = totalCreditsCompleted + creds.Sum(tc => tc.CompletedCredit ?? 0m);
                        // Update totals for Transfer Credits and GPA
                        totalTransferGpaCredits = totalTransferGpaCredits + transferGpaCredits;
                        totalTransferGpaGradePoints = totalTransferGpaGradePoints + transferGpaGradePoints;
                        totalTransferCreditsCompleted = totalTransferCreditsCompleted + transferCredits;
                    }
                    else
                    {
                        creds = credits.Where(c => c.TermCode != null && c.TermCode == term);
                        List<AcademicCredit> credList = new List<AcademicCredit>();
                        foreach (var cred in creds)
                        {
                            if (studentGradeRestriction.IsRestricted)
                            {
                                // If the student is restricted from seeing their grades. Don't pass any of this back with the academic history.
                                // By setting the GpaCredit to zero the GPA will also end up as 0.
                                cred.VerifiedGrade = null;
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
                                transferGpaCredits = transferGpaCredits + cred.GpaCredit ?? 0m;
                                transferGpaGradePoints = transferGpaGradePoints + cred.GradePoints;
                                transferCredits = transferCredits + cred.CompletedCredit ?? 0m;
                            }
                        }
                        termGpaCredits = creds.Sum(ac => ac.GpaCredit ?? 0m);
                        termGpaGradePoints = creds.Sum(agp => agp.GradePoints);
                        // Changed to use CompletedCredit SRM 04/15/2014
                        termCredits = creds.Sum(tc => tc.CompletedCredit ?? 0m);
                        termCeus = creds.Sum(c => c.ContinuingEducationUnits);
                        AcademicTerm tgpa = new AcademicTerm() { GradePointAverage = (termGpaCredits == 0 ? 0 : (termGpaGradePoints / termGpaCredits)), TermId = term, Credits = termCredits, ContinuingEducationUnits = termCeus, AcademicCredits = credList };
                        if (termGpaCredits == 0)
                            tgpa.GradePointAverage = null;
                        _AcademicTerms.Add(tgpa);

                        totalGpaCredits = totalGpaCredits + termGpaCredits;
                        totalGpaGradePoints = totalGpaGradePoints + termGpaGradePoints;
                        totalCreditsCompleted = totalCreditsCompleted + termCredits;
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

        /// <summary>
        /// This method update credit replaced and replacement status.
        /// If a course cannot be retaken for credits but same course is taken multiple times then only one credit will be applied and marked as if its possible replacement
        /// and others credits will be marked as replace in progress.
        /// There is nothing to do for the credits that are already marked as "replaced" by Collegue on STRP screen. In this case if all the credits are completed then only one credit
        /// will be marked as "Replacement" and rest of them will remain "Replaced" .
        /// There are scenarios where credits are repeated and completed but are not marked as Replaced by Colleague like in AVG grade policy, in that case repeats is not considered.
        /// There could be scenarios where few credits are marked as Replaced and few are graded and completed but not marked as replaced like when a course was graded F and Repeat value or grade value on GRDC for F grade was empty.

        /// </summary>
        /// <param name="allCredits"></param>
        private void UpdateCreditsReplaceStatus(IEnumerable<AcademicCredit> allCredits)
        {
            if (allCredits == null)
                return;
            //filter out dropped, deleted, withdrawn, cancelled credits
            IEnumerable<AcademicCredit> filteredCredits = allCredits.Where(c => c!=null && c.Status != CreditStatus.Dropped && c.Status != CreditStatus.Deleted && c.Status != CreditStatus.Withdrawn && c.Status != CreditStatus.Cancelled);
            if (filteredCredits == null || !filteredCredits.Any())
            {
                return;
            }
            //group all the credits on course key.
            //only those credits are needed to be added to group when course does not allow retakes for credits and credits have list of other repeated credit ids.
            ILookup<string, AcademicCredit> groupedCredits = filteredCredits.Where(c => c.RepeatAcademicCreditIds != null && c.RepeatAcademicCreditIds.Any() && c.ReplacedStatus == ReplacedStatus.NotReplaced && c.ReplacementStatus == ReplacementStatus.NotReplacement && c.CanBeReplaced).Where(cr => cr.Course != null && !cr.Course.AllowToCountCourseRetakeCredits).ToLookup(c => c.Course.Id, c => c);
            if (groupedCredits != null && groupedCredits.Any())
            {
                //process each grouped course to mark credits replace/replacement status
                foreach (var creditsLst in groupedCredits)
                {
                    //add equated course credits to above list
                    //find all the credits that are equated to current course
                    var equatedCredits = filteredCredits.Where(c =>c.Course!=null && c.Course.EquatedCourseIds!=null && c.Course.EquatedCourseIds.Contains(creditsLst.Key));
                    var creditsToProcess = groupedCredits[creditsLst.Key].ToList();
                    if (equatedCredits != null && equatedCredits.Any())
                    {
                        creditsToProcess.AddRange(equatedCredits.Where(e => e != null && e.ReplacedStatus != ReplacedStatus.Replaced));
                    }
                   var orderedCreditsToProcess=creditsToProcess.OrderBy(c => c.StartDate).ToList();

                    if (orderedCreditsToProcess != null && orderedCreditsToProcess.Any())
                    {
                        var currentCredit = orderedCreditsToProcess[0];

                        //if all the credits to process are completed credits then mark all as replacement. 
                        int completedCredits = orderedCreditsToProcess.Where(o => o.IsCompletedCredit).Count();
                        if (completedCredits > 0 && completedCredits == orderedCreditsToProcess.Count())
                        {
                            //find if any other repeated credits are replaced and completed
                            List<string> otherRepeatedCredits = currentCredit.RepeatAcademicCreditIds.Where(c => c != currentCredit.Id).ToList();
                            if (otherRepeatedCredits.Any())
                            {
                                int countOfCreditsReplaced = allCredits.Where(c => otherRepeatedCredits.Contains(c.Id) && c.ReplacedStatus == ReplacedStatus.Replaced && c.IsCompletedCredit).Count();
                                if (countOfCreditsReplaced > 0)
                                {
                                    //mark all the completed credits as Replacement
                                    orderedCreditsToProcess.ForEach(o => o.ReplacementStatus = ReplacementStatus.Replacement);
                                }
                            }
                        }
                        else if (orderedCreditsToProcess.Count > 1)
                        {
                            var nextCredit = orderedCreditsToProcess[1];
                          updateReplaceStatus(currentCredit, nextCredit, orderedCreditsToProcess);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// This is recursive method that marks the status of credits on basis of  replace and replacement.
        /// Comparison happens between current credit and next credit in sequence
        /// </summary>
        /// <param name="currentCredit"></param>
        /// <param name="nextCredit"></param>
        /// <param name="creditsToProcess"></param>
        private void updateReplaceStatus(AcademicCredit currentCredit, AcademicCredit nextCredit, List<AcademicCredit> creditsToProcess)
        {
            AcademicCredit creditCompared=null;
            AcademicCredit creditToCompareWith=null;
            if (nextCredit == null || currentCredit == null || creditsToProcess==null)
            {
                return;
            }
            if (!nextCredit.IsCompletedCredit)
            {

                currentCredit.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                currentCredit.ReplacementStatus = ReplacementStatus.NotReplacement;
                nextCredit.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                nextCredit.ReplacedStatus = ReplacedStatus.NotReplaced;
                creditCompared = nextCredit;

            }
            // If one was complete and the other was in progress, then the in-progress course is the possible replacement.
            else if (nextCredit.IsCompletedCredit)
            {
                currentCredit.ReplacedStatus = ReplacedStatus.NotReplaced;
                currentCredit.ReplacementStatus = ReplacementStatus.PossibleReplacement;
                nextCredit.ReplacedStatus = ReplacedStatus.ReplaceInProgress;
                nextCredit.ReplacementStatus = ReplacementStatus.NotReplacement;
                creditCompared = currentCredit;

            }
            try
            {
                //find the next credit in list
                int index = creditsToProcess.FindLastIndex(a => a.Equals(nextCredit))+1;
                creditToCompareWith = creditsToProcess.ElementAt(index);
            }
            catch(ArgumentOutOfRangeException )
            {
                creditToCompareWith = null;
            }
            catch(Exception )
            {
                creditToCompareWith = null;
                throw;
            }
            updateReplaceStatus(creditCompared, creditToCompareWith, creditsToProcess);
        }

       
    }
}
