// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class AcademicCreditEntityToApplicantAcademicCreditDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.ApplicantAcademicCredit>
    {
        public AcademicCreditEntityToApplicantAcademicCreditDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Custom mapping of AcademicCredit entity to AcademicCredit2 DTO.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns><see cref="ApplicantAcademicCredit">ApplicantAcademicCredit</see> data transfer object</returns>
        public override Dtos.Student.ApplicantAcademicCredit MapToType(Domain.Student.Entities.AcademicCredit Source)
        {
            var academicCreditDto = base.MapToType(Source);

            // Custom: Need only the ID of the course
            if (Source.Course != null)
            {
                academicCreditDto.CourseId = Source.Course.Id;
            }
            // Custom: Return only the grade ID
            if (Source.VerifiedGrade != null)
            {
                academicCreditDto.VerifiedGradeId = Source.VerifiedGrade.Id;
            }

            return academicCreditDto;
        }
    }
}
