// Copyright 2015 Ellucian Company L.P. and its affiliates.using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Registration Date information by Term.
    /// </summary>
    [Serializable]
    public class TermRegistrationDate : RegistrationDate
    {
        /// <summary>
        /// Term Id for which these dates apply.
        /// </summary>
        public string TermId { get { return _termId; } }
        private string _termId;

        public TermRegistrationDate(string termId, string location, DateTime? registrationStartDate, DateTime? registrationEndDate,
            DateTime? preRegistrationStartDate, DateTime? preRegistrationEndDate,
            DateTime? addStartDate, DateTime? addEndDate,
            DateTime? dropStartDate, DateTime? dropEndDate,
            DateTime? dropGradeRequiredDate, List<DateTime?> censusDates)
            : base(location, registrationStartDate, registrationEndDate, preRegistrationStartDate, preRegistrationEndDate, addStartDate, addEndDate, dropStartDate, dropEndDate, dropGradeRequiredDate, censusDates)
        {
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "Must have a term Id to have a TermRegistrationDate.");
            }
            _termId = termId;
        }
    }
}
