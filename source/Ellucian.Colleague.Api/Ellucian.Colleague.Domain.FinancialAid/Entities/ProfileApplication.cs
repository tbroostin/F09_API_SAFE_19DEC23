/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// The ProfileApplication is a type of FinancialAidApplication. Similar to the FAFSA, it is an alternative FA data assessment tool, 
    /// used primarily at private colleges. It produces an assessment similar to what is found within the ISIR.
    /// </summary>
    [Serializable]
    public class ProfileApplication : FinancialAidApplication2
    {
        /// <summary>
        /// Create a ProfileApplication object
        /// </summary>
        /// <param name="id">The database record id of the profile application</param>
        /// <param name="awardYear">The awardYear this application applies to</param>
        /// <param name="studentId">The Colleague PERSON id of the student to whom this application belongs</param>
        public ProfileApplication(string id, string awardYear, string studentId)
            : base(id, awardYear, studentId)
        {

        }
    }
}
