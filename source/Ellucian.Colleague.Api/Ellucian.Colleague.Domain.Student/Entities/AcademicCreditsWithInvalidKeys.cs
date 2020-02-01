// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// AcademicCreditsWithInvalidKeys carries two properties-
    /// 1. AcademicCredits
    /// 2. InvalidAcademicCreditIds
    /// Academic Credit represents some credit that a student has earned.  Typically, academic credit
    /// is a course that was taken, but can also be transfer work or non-course work (life experience, portfolio, etc)
    /// InvalidAcademicCreditIds are the list of academic credits Ids that are not found in STUDENT.ACAD.CRED file but are referenced in STUDENT.COURSE.SEC file.
    /// </summary>
    [Serializable]
    public class AcademicCreditsWithInvalidKeys
    {
        /// <summary>
        /// List of Academic Credits
        /// </summary>
        private IEnumerable<AcademicCredit> _academicCredits;
        /// <summary>
        /// Ids of academic credits that were not found in a file.
        /// </summary>
        private IEnumerable<string> _invalidAcademicCreditIds;
        /// <summary>
        /// List of Academic Credits
        /// </summary>
        public IEnumerable<AcademicCredit> AcademicCredits { get { return _academicCredits; } }
        /// <summary>
        /// Ids of academic credits that were not found in a file.
        /// </summary>
        public IEnumerable<string> InvalidAcademicCreditIds { get { return _invalidAcademicCreditIds; } }
       
        /// <summary>
        /// parameterized constructor
        /// </summary>
        /// <param name="academicCredits"></param>
        /// <param name="invalidAcademicCreditIds"></param>
        public AcademicCreditsWithInvalidKeys(IEnumerable<AcademicCredit> academicCredits, IEnumerable<string> invalidAcademicCreditIds)
        {
            if(academicCredits == null)
            {
                throw new ArgumentNullException("academicCredits", "Academic Credits cannot be null");
            }

            this._academicCredits = academicCredits;
            this._invalidAcademicCreditIds = invalidAcademicCreditIds;
        }
    }
}
