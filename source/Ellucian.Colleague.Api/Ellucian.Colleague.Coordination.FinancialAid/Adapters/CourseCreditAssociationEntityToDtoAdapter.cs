/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom CourseCreditAssociationEntityToDtoAdapter extends AutoMapperAdapter
    /// </summary>
    public class CourseCreditAssociationEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.CourseCreditAssociation, Dtos.FinancialAid.CourseCreditAssociation>
    {
        /// <summary>
        /// Constructor for CourseCreditAssociationEntityToDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public CourseCreditAssociationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        //A redundant override since the Entity->DTO properties match 1:1 for CourseCreditAssociation
        public override Dtos.FinancialAid.CourseCreditAssociation MapToType(Domain.FinancialAid.Entities.CourseCreditAssociation Source)
        {
            var courseCreditAssociationDto = new Dtos.FinancialAid.CourseCreditAssociation();

            courseCreditAssociationDto.StudentId   = Source.StudentId;
            courseCreditAssociationDto.AwardYear   = Source.AwardYear;
            courseCreditAssociationDto.AwardPeriod = Source.AwardPeriod;
            courseCreditAssociationDto.CourseName = Source.CourseName;
            courseCreditAssociationDto.CourseTitle = Source.CourseTitle;
            courseCreditAssociationDto.CourseSection = Source.CourseSection;
            courseCreditAssociationDto.CourseTerm = Source.CourseTerm;
            courseCreditAssociationDto.CourseCred = Source.CourseCred;
            courseCreditAssociationDto.CourseInstCred = Source.CourseInstCred;
            courseCreditAssociationDto.CourseTivCred = Source.CourseTivCred;
            courseCreditAssociationDto.CoursePellCred = Source.CoursePellCred;
            courseCreditAssociationDto.CourseDlCred = Source.CourseDlCred;
            courseCreditAssociationDto.CourseProgFlag = Source.CourseProgFlag;
            courseCreditAssociationDto.DegreeAuditActiveFlag = Source.DegreeAuditActiveFlag;

            return courseCreditAssociationDto;
        }
    }
}
