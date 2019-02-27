// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class GraduationApplicationProgramEligibility
    {
        /// <summary>
        /// Id of student for which eligibility applies
        /// </summary>
        private string _StudentId;
        public string StudentId { get { return _StudentId; } }

        /// <summary>
        /// Program for which the graduation application eligibilty applies
        /// </summary>
        private string _ProgramCode;
        public string ProgramCode { get { return _ProgramCode; } }

        /// <summary>
        /// If student is not eligible, this is a list of reasons explaining why they are not eligible.
        /// </summary>
        private readonly List<string> _IneligibleMessages = new List<string>();
        public ReadOnlyCollection<string> IneligibleMessages { get; private set; }

        /// <summary>
        /// Boolean indicates if the student is eligible to apply for graduation in this student program
        /// </summary>
        private bool _IsEligible;
        public bool IsEligible { get { return _IsEligible; } }

        /// <summary>
        /// Constructs an object indicating if student has registration ineligibility and if the user has the ability to override
        /// </summary>
        /// <param name="messages">List of reasons explaining why the student is not eligible to apply for graduation in this program. 
        /// <param name="isEligible">boolean indicating whether the student is eligible for registration.</param>
        public GraduationApplicationProgramEligibility(string studentId, string programCode, bool isEligible)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student Id cannot be null.");
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program Code cannot be null.");
            }
            _StudentId = studentId;
            _ProgramCode = programCode;
            _IsEligible = isEligible;
            IneligibleMessages = _IneligibleMessages.AsReadOnly();
        }

        public void AddIneligibleMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message", "Cannot add a null or empty message to graduation application program eligibility.");
            }
            _IneligibleMessages.Add(message);
        }
        
    }
}
