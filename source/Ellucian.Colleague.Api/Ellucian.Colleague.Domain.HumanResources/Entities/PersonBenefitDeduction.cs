using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Contains general information for a Benefit/Deduction held by a HR person.
    /// </summary>
    [Serializable]
    public class PersonBenefitDeduction
    {
        public string PersonId { get; private set; }

        public string BenefitDeductionId { get; private set; }

        public DateTime EnrollmentDate { get; private set; }

        public DateTime? CancelDate { get; set; }

        public DateTime? LastPayDate { get; set; }

        public PersonBenefitDeduction(string personId, string benefitDeductionId, DateTime enrollmentDate, DateTime? cancelDate = null, DateTime? lastPayDate = null)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(benefitDeductionId))
            {
                throw new ArgumentNullException("benefitDeductionId");
            }

            PersonId = personId;
            BenefitDeductionId = benefitDeductionId;
            EnrollmentDate = enrollmentDate;
            CancelDate = cancelDate;
            LastPayDate = lastPayDate;
        }
    }
}
