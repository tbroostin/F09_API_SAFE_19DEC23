// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class VerifiedTermGrade :TermGrade
    {
        /// <summary>
        /// Constructor for verified grade
        /// </summary>
        /// <param name="id"></param>
        /// <param name="submittedOn"></param>
        /// <param name="submittedBy"></param>
        /// <param name="gradeTypeCode"></param>
        public VerifiedTermGrade(string id, DateTimeOffset? submittedOn, string submittedBy, string gradeTypeCode = "") :
            base(id, submittedOn, submittedBy, gradeTypeCode)
        {

        }     
    }
}
