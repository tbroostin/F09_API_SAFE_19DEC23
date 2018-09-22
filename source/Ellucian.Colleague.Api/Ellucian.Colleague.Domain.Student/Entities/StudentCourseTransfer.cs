using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentCourseTransfer
    {
        public string Guid { get; set; }
        public string Id { get; set; }
        public string Student { get; set; }
        public string Course { get; set; }
        public string TransferredFromInstitution { get; set; }
        public string AcademicLevel { get; set; }
        public List<string> AcademicPrograms { get; set; }
        public string AcademicPeriod { get; set; }
        public string GradeScheme { get; set; }
        public string Grade { get; set; }
        public decimal? QualityPoints { get; set; }
        public string Status { get; set; }

        //studentcoursetrsnfercreditdtoproperty

        public string CreditType { get; set; }

        // StudentCourseTransferCreditCategoryDtoProperty
        //public CreditType StudentCourseTransferCreditType { get; set; }  //this is an enum in the dto
        //public string DetailGuid { get; set; }
        // end StudentCourseTransferCreditCategoryDtoProperty

        public decimal? AwardedCredit { get; set; }
        //end studentcoursetrsnfercreditdtoproperty

        public DateTime? EquivalencyAppliedOn { get; set; }


    }
}
