// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// AcademicCreditsWithInvalidKeys DTO carries valid AcademicCredits and Invalid Academic Credits Ids.
    /// </summary>
    public class AcademicCreditsWithInvalidKeys
    {
        /// <summary>
        /// Academic Credits
        /// </summary>
        public IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicCredit3> AcademicCredits { get;  set; }
        /// <summary>
        /// Ids of academic credits that were not found in a file.
        /// </summary>
        public IEnumerable<string> InvalidAcademicCreditIds { get;  set; }
        /// <summary>
        /// Default constructor
        /// </summary>
        public  AcademicCreditsWithInvalidKeys()
        {

        }
        /// <summary>
        /// parameterized constructor
        /// </summary>
        /// <param name="academicCredits"></param>
        /// <param name="invalidAcademicCreditIds"></param>
        public AcademicCreditsWithInvalidKeys(IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicCredit3> academicCredits, IEnumerable<string> invalidAcademicCreditIds)
        {
            this.AcademicCredits = academicCredits;
            this.InvalidAcademicCreditIds = invalidAcademicCreditIds;
        }
    }
}
