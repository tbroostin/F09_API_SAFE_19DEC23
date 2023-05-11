/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom AwardYearCreditsEntityToDtoAdapter extends AutoMapperAdapter and provides an override to map embedded AwardPeriodCredits DTO lists
    /// </summary>
    public class AwardYearCreditsEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AwardYearCredits, Dtos.FinancialAid.AwardYearCredits>
    {
        /// <summary>
        /// Constructor for AwardYearCreditsEntityToDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public AwardYearCreditsEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.AwardPeriodCredits, Dtos.FinancialAid.AwardPeriodCredits>();
            AddMappingDependency<Domain.FinancialAid.Entities.CourseCreditAssociation, Dtos.FinancialAid.CourseCreditAssociation>();
        }

        public override Dtos.FinancialAid.AwardYearCredits MapToType(Domain.FinancialAid.Entities.AwardYearCredits Source)
        {
            var awardYearCreditsDto = new Dtos.FinancialAid.AwardYearCredits();

            awardYearCreditsDto.StudentId = Source.StudentId;
            awardYearCreditsDto.AwardYear = Source.AwardYear;
            awardYearCreditsDto.ContainsCourseCredits = Source.ContainsCourseCredits;
            awardYearCreditsDto.AwardPeriodCoursework = new List<Dtos.FinancialAid.AwardPeriodCredits>();

            //If the source AwardYearCredits entity has any AwardPeriodCoursework, use the custom AwardPeriodCredits DTO mapping
            if (Source.AwardPeriodCoursework.Count > 0)
            {
                AwardPeriodCreditsEntityToDtoAdapter awardPeriodCreditsDtoAdapter = new AwardPeriodCreditsEntityToDtoAdapter(adapterRegistry, logger);
                awardYearCreditsDto.AwardPeriodCoursework.AddRange(Source.AwardPeriodCoursework.Select(apc => awardPeriodCreditsDtoAdapter.MapToType(apc)));
            }
            return awardYearCreditsDto;
        }
    }
}
