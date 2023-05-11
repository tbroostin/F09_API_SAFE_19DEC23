/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom AwardPeriodCreditsEntityToDtoAdapter extends AutoMapperAdapter to provide an override and allow for mapping of embedded CourseCreditAssociation DTO lists
    /// </summary>
    public class AwardPeriodCreditsEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AwardPeriodCredits, Dtos.FinancialAid.AwardPeriodCredits>
    {
        /// <summary>
        /// Constructor for AwardPeriodCreditsEntityToDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public AwardPeriodCreditsEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.CourseCreditAssociation, Dtos.FinancialAid.CourseCreditAssociation>();
        }

        public override Dtos.FinancialAid.AwardPeriodCredits MapToType(Domain.FinancialAid.Entities.AwardPeriodCredits Source)
        {
            var awardPeriodCreditsDto = new Dtos.FinancialAid.AwardPeriodCredits();

            awardPeriodCreditsDto.StudentId   = Source.StudentId;
            awardPeriodCreditsDto.AwardYear   = Source.AwardYear;
            awardPeriodCreditsDto.AwardPeriod = Source.AwardPeriod;
            awardPeriodCreditsDto.AwardPeriodDescription = Source.AwardPeriodDescription;
            awardPeriodCreditsDto.ProgramName = Source.ProgramName;
            awardPeriodCreditsDto.DegreeAuditActive = Source.DegreeAuditActive;
            awardPeriodCreditsDto.Coursework = new List<Dtos.FinancialAid.CourseCreditAssociation>();

            //If the source AwardPeriodCredits entity record has CourseCreditAssociations use the custom map provided to map them
            if (Source.Coursework.Count > 0)
            {
                CourseCreditAssociationEntityToDtoAdapter courseCreditAssociationEntityToDtoAdapter = new CourseCreditAssociationEntityToDtoAdapter(adapterRegistry, logger);
                awardPeriodCreditsDto.Coursework.AddRange(Source.Coursework.Select(cw => courseCreditAssociationEntityToDtoAdapter.MapToType(cw)));
            }
            return awardPeriodCreditsDto;
        }
    }
}
