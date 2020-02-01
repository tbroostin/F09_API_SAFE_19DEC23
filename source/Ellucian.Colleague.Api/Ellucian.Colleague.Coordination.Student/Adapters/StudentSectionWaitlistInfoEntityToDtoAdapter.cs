// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an StudentSectionWaitlistInfo entity to an StudentSectionWaitlistInfo DTO
    /// </summary>
    public class StudentSectionWaitlistInfoEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentSectionWaitlistInfo, Ellucian.Colleague.Dtos.Student.StudentSectionWaitlistInfo>
    {
        /// <summary>
        /// StudentSectionWaitlistInfo entity adapter (current Entity to StudentSectionWaitlistInfo DTO) constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public StudentSectionWaitlistInfoEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        { }
        
        /// <summary>
        /// StudentSectionWaitlistInfo entity adapter (current Entity to StudentSectionWaitlistInfo DTO) constructor
        /// </summary>
        /// <param name="Source"><see cref="Ellucian.Colleague.Domain.Student.Entities.StudentSectionWaitlistInfo">StudentSectionWaitlistInfo</see> entity object</cref></param>
        /// <returns></returns>
        public override Dtos.Student.StudentSectionWaitlistInfo MapToType(Domain.Student.Entities.StudentSectionWaitlistInfo Source)
        {
            Dtos.Student.SectionWaitlistConfig sectionWaitlistConfig = new Dtos.Student.SectionWaitlistConfig()
            {
                ShowRank = Source.SectionWaitlistConfig.ShowRank,
                ShowRating = Source.SectionWaitlistConfig.ShowRating,
                NoOfDaysToEnroll = Source.SectionWaitlistConfig.NoOfDaysToEnroll
            };
            Dtos.Student.SectionWaitlistStudent sectionWaitlistStudent = new Dtos.Student.SectionWaitlistStudent()
            {
                Rank = Source.SectionWaitlistStudent.Rank,
                Rating = Source.SectionWaitlistStudent.Rating,
                SectionId = Source.SectionWaitlistStudent.SectionId,
                StudentId = Source.SectionWaitlistStudent.StudentId,
                StatusDate = Source.SectionWaitlistStudent.StatusDate
            };
            Dtos.Student.StudentSectionWaitlistInfo dto = new Dtos.Student.StudentSectionWaitlistInfo()
            {
                SectionWaitlistConfig = sectionWaitlistConfig,
                SectionWaitlistStudent = sectionWaitlistStudent              
            };

            return dto;
        }
    }
}
