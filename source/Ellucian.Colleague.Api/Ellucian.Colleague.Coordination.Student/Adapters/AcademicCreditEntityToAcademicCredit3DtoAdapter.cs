// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class AcademicCreditEntityToAcademicCredit3DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit3>
    {
        public AcademicCreditEntityToAcademicCredit3DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>();
        }

        /// <summary>
        /// Custom mapping of AcademicCredit entity to AcademicCredit2 DTO.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns><see cref="AcademicCredit2">AcademicCredit2</see> data transfer object</returns>
        public override Dtos.Student.AcademicCredit3 MapToType(Domain.Student.Entities.AcademicCredit Source)
        {
            var academicCredit2Dto = base.MapToType(Source);

            // Custom: Need only the ID of the course
            if (Source.Course != null)
            {
                academicCredit2Dto.CourseId = Source.Course.Id;
            }
            // Custom: Return only the grade ID
            if (Source.VerifiedGrade != null)
            {
                academicCredit2Dto.VerifiedGradeId = Source.VerifiedGrade.Id;
            }

            return academicCredit2Dto;
        }
    }
}
