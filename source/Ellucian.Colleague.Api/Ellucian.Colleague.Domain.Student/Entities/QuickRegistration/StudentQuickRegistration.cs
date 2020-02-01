// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.QuickRegistration
{
    /// <summary>
    /// A student's previously identified academic terms and associated course sections that may be used in the Colleague Self-Service Quick Registration workflow
    /// </summary>
    [Serializable]
    public class StudentQuickRegistration
    {
        /// <summary>
        /// Unique identifier for a student
        /// </summary>
        public string StudentId { get; private set; }

        /// <summary>
        /// List of academic terms for which the student could potentially register via the Colleague Self-Service Quick Registration workflow
        /// </summary>
        public ReadOnlyCollection<QuickRegistrationTerm> Terms { get; private set; }
        private readonly List<QuickRegistrationTerm> _terms = new List<QuickRegistrationTerm>();

        /// <summary>
        /// Creates a new <see cref="StudentQuickRegistration"/> object
        /// </summary>
        /// <param name="studentId">Unique identifier for a student</param>
        public StudentQuickRegistration(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "A student ID is required when building quick registration data.");
            }
            StudentId = studentId;
            Terms = _terms.AsReadOnly();
        }

        /// <summary>
        /// Add a quick registration term to the student's list of quick registration terms
        /// </summary>
        /// <param name="term">Quick registration term</param>
        public void AddTerm(QuickRegistrationTerm term)
        {
            if (term == null)
            {
                throw new ArgumentNullException("term", "Quick registration terms cannot be null.");
            }
            if (_terms.Any(t => t.TermCode == term.TermCode))
            {
                throw new ApplicationException(string.Format("Term {0} cannot be added as a quick registration term for student {1} more than once.", term.TermCode, this.StudentId));
            }
            _terms.Add(term);
        }
    }
}
