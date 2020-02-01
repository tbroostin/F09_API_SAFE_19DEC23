using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class OfferLetterHousingEnrollmentItem
    {
        /// <summary>
        /// Description associated with a housing assignment
        /// </summary>
        public string AlhHousingDesc { get; set; }

        /// <summary>
        /// Description associated with an enrollment assignment
        /// </summary>
        public string AlhEnrollmentDesc { get; set; }

        public OfferLetterHousingEnrollmentItem(string housingDescription, string enrollmentDescription)
        {
            AlhHousingDesc = housingDescription;
            AlhEnrollmentDesc = enrollmentDescription;
        }
    }
}
