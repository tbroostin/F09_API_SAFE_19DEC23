// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RegistrationEligibility
    {
        /// <summary>
        /// List of <see cref="RegistrationMessage">RegistrationMessages</see> as a result of checking student eligibility rules.
        /// </summary>
        private List<RegistrationMessage> _messages;
        public List<RegistrationMessage> Messages { get { return _messages; } }

        /// <summary>
        /// Boolean indicates if the student is eligible for registration
        /// </summary>
        private bool _isEligible;
        public bool IsEligible { get { return _isEligible; } }

        /// <summary>
        /// Boolean indicates if the user has permissions to override and perform registration actions for the student, even if not eligible.
        /// </summary>
        private bool _hasOverride;
        public bool HasOverride { get { return _hasOverride; } }

        // Note: Registration Eligibility Terms do not exist outside of an overall registration eligibility item
        private readonly List<RegistrationEligibilityTerm> _terms = new List<RegistrationEligibilityTerm>();
        public List<RegistrationEligibilityTerm> Terms { get { return _terms; } }

        /// <summary>
        /// Constructs an object indicating if student has registration ineligibility and if the user has the ability to override
        /// </summary>
        /// <param name="messages">List of <see cref="RegistrationMessages">Registration Messages</see>. The presence of any
        /// <param name="isEligible">boolean indicating whether the student is eligible for registration.</param>
        /// <param name="hasOverride">boolean indicating whether the user has the ability to override and perform registration actions, if omitted defaults to false.</param>
        public RegistrationEligibility(IEnumerable<RegistrationMessage> messages, bool isEligible, bool hasOverride = false)
        {
            if (messages == null)
            {
                throw new ArgumentNullException("messages", "Registration messages cannot be null.");
            }

            _messages = messages.ToList();
            _isEligible = isEligible;
            _hasOverride = hasOverride;

        }


        /// <summary>
        /// Method to add a meeting time to a section
        /// </summary>
        /// <param name="meetingTime">A meeting Time object</param>
        public void AddRegistrationEligibilityTerm(RegistrationEligibilityTerm term)
        {
            if (term == null)
            {
                throw new ArgumentNullException("term", "term cannot be null");
            }
            // Only want a term to be in the list once.
            if (Terms.Where(m => m.Equals(term)).Count() > 0)
            {
                throw new ArgumentException("Registration Eligibility already exists in this list", "term");
            }
            _terms.Add(term);
        }

        public void UpdateRegistrationPriorities(IEnumerable<RegistrationPriority> registrationPriorities, IEnumerable<Term> allTerms)
        {
            if (registrationPriorities == null)
            {
                registrationPriorities = new List<RegistrationPriority>();
            }
            // sort priorities ascending by start, end, id
            var sortedPriorities = registrationPriorities.OrderBy(a => (a.Start.HasValue ? a.Start.Value : DateTime.MinValue)).ThenBy(a => (a.End.HasValue ? a.End.Value : DateTime.MinValue)).ThenBy(a => a.Id.PadLeft(10, '0')).ToList();
            foreach (var registrationEligibilityTerm in Terms)
            {
                // Only check the priorities if the registration eligibility term has check priority set to true.
                // If the student is not currently eligibile to register because they have failed term registration rules then do not check the registration priorities
                //   because failing term rules trumps the registration priorities.
                if (registrationEligibilityTerm.CheckPriority && !registrationEligibilityTerm.FailedRegistrationTermRules)
                {
                    var termsToMatch = new List<string>();
                    termsToMatch.Add(registrationEligibilityTerm.TermCode);
                    var term = allTerms.Where(t => t.Code == registrationEligibilityTerm.TermCode).FirstOrDefault();
                    // Note: if the term's code != the terms's reporting term, add the reporting term into the terms to match with
                    if (term != null && !(string.IsNullOrEmpty(term.Code)) && !(string.IsNullOrEmpty(term.ReportingTerm)) && term.Code != term.ReportingTerm)
                    {
                        termsToMatch.Add(term.ReportingTerm);
                    }
                    DateTimeOffset? start = null;
                    DateTimeOffset? end = null;
                    DateTimeOffset? checkAgainOn = null;
                    DateTimeOffset currentTime = DateTimeOffset.Now;
                    bool validStudent = false;
                    bool eligibilityError = false;
                    string eligibilityMsg = null;
                    bool failedPriorityTest = false;
                    RegistrationEligibilityTermStatus status = registrationEligibilityTerm.Status;

                    foreach (var priority in sortedPriorities)
                    {
                        if (priority.Start.HasValue)
                        {
                            if (string.IsNullOrEmpty(priority.TermCode) ||
                                (!string.IsNullOrEmpty(priority.TermCode) && termsToMatch.Contains(priority.TermCode)))
                            {

                                if (start == null ||
                                    (start != null && priority.Start < start))
                                {
                                    start = priority.Start;
                                }

                                if (end != null ||
                                    (end != null && priority.End > end))
                                {
                                    end = priority.End;
                                }
                                else
                                {
                                    end = priority.End;
                                }
                                // check the priority
                                if (priority.Start > currentTime)
                                {
                                    eligibilityError = true;
                                    // RG134
                                    eligibilityMsg = "Student Registration has not opened.";
                                    checkAgainOn = start;
                                    status = RegistrationEligibilityTermStatus.Future;
                                    failedPriorityTest = true;
                                }
                                else
                                {
                                    if (priority.End.HasValue && priority.End < currentTime)
                                    {
                                        eligibilityError = true;
                                        // RG135
                                        eligibilityMsg = "Student Registration Priority has closed.";
                                        start = null;
                                        end = null;
                                        status = RegistrationEligibilityTermStatus.Past;
                                        failedPriorityTest = true;
                                    }
                                    else
                                    {
                                        eligibilityError = false;
                                        validStudent = true;
                                    }
                                }
                            }
                        }
                        if (validStudent)
                        {
                            break;
                        }
                    }

                    // If the student is not valid but there is an eligibility error it means they are either before or after their allowed time.
                    if (!validStudent && eligibilityError && !registrationEligibilityTerm.PriorityOverridable)
                    {
                        // Only update the status if it is not currently overridden. But do update the messages and dates.
                        if (registrationEligibilityTerm.Status != RegistrationEligibilityTermStatus.HasOverride)
                        {
                            registrationEligibilityTerm.Status = status;
                            if (failedPriorityTest)
                            {
                                registrationEligibilityTerm.FailedRegistrationPriorities = true;
                            }
                        }
                        registrationEligibilityTerm.Message = eligibilityMsg;
                        registrationEligibilityTerm.AnticipatedTimeForAdds = checkAgainOn;
                    }

                    // If the student is not valid but there is no error and the term isn't priority overridable for this student
                    // then the status for this student is not eligible.
                    if (!validStudent && !eligibilityError && !registrationEligibilityTerm.PriorityOverridable)
                    {
                        foreach (var termCode in termsToMatch)
                        {
                            var checkTerm = allTerms.Where(t => t.Code == termCode).FirstOrDefault();
                            if (checkTerm != null && checkTerm.RegistrationPriorityRequired)
                            {
                                // If the stauts is not overridden we still need to update the messages and date but do not change the status.
                                if (registrationEligibilityTerm.Status != RegistrationEligibilityTermStatus.HasOverride)
                                {
                                    registrationEligibilityTerm.Status = RegistrationEligibilityTermStatus.NotEligible;
                                }
                                //// RG136
                                registrationEligibilityTerm.Message = "Student has no Registration Priority. Term " + termCode + " requires one.";
                                registrationEligibilityTerm.AnticipatedTimeForAdds = checkAgainOn;
                                registrationEligibilityTerm.FailedRegistrationPriorities = true;
                                //break;
                            }
                        }
                    }
                }
            }

        }
    }
}
