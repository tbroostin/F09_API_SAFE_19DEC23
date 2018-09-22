//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{

    /// <summary>
    /// This class maps a StudentAward Entity to an outbound StudentAward DTO
    /// </summary>
    public class StudentAwardEntityToDtoAdapter : BaseAdapter<Domain.FinancialAid.Entities.StudentAward, Dtos.FinancialAid.StudentAward>
    {
        /// <summary>
        /// Constructor for the custom StudentAwardEntityToDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public StudentAwardEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }


        /// <summary>
        /// This Override method maps a StudentAward entity object to a StudentAward DTO. 
        /// A custom mapping is needed specifically to map the Award and AwardStatus objects to the
        /// id strings in the Dto.
        /// </summary>
        /// <param name="Source">The StudentAward entity object to map</param>
        /// <returns>A StudentAward DTO mapped from the given entity object.</returns>
        public override Dtos.FinancialAid.StudentAward MapToType(Domain.FinancialAid.Entities.StudentAward Source)
        {
            var studentAwardDto = new Dtos.FinancialAid.StudentAward();
            studentAwardDto.AwardYearId = Source.StudentAwardYear.Code;
            studentAwardDto.StudentId = Source.StudentId;
            studentAwardDto.AwardId = Source.Award.Code;
            studentAwardDto.StudentAwardPeriods = new List<Dtos.FinancialAid.StudentAwardPeriod>();
            studentAwardDto.IsEligible = Source.IsEligible;
            studentAwardDto.IsAmountModifiable = Source.IsAmountModifiable;
            studentAwardDto.PendingChangeRequestId = Source.PendingChangeRequestId;


            foreach (var studentAwardPeriodEntity in Source.StudentAwardPeriods)
            {
                var studentAwardPeriodDto = new Dtos.FinancialAid.StudentAwardPeriod()
                {
                    AwardYearId = studentAwardPeriodEntity.StudentAwardYear.Code,
                    StudentId = studentAwardPeriodEntity.StudentId,
                    AwardId = studentAwardPeriodEntity.Award.Code,
                    AwardPeriodId = studentAwardPeriodEntity.AwardPeriodId,
                    AwardAmount = studentAwardPeriodEntity.AwardAmount,
                    AwardStatusId = studentAwardPeriodEntity.AwardStatus.Code,
                    IsFrozen = studentAwardPeriodEntity.IsFrozen,
                    IsTransmitted = studentAwardPeriodEntity.IsTransmitted,
                    IsAmountModifiable = studentAwardPeriodEntity.IsAmountModifiable,
                    IsStatusModifiable = studentAwardPeriodEntity.IsStatusModifiable,
                    IsIgnoredOnAwardLetter = studentAwardPeriodEntity.IsIgnoredOnAwardLetter,
                    IsViewableOnAwardLetterAndShoppingSheet = studentAwardPeriodEntity.IsViewableOnAwardLetterAndShoppingSheet,
                    IsIgnoredOnChecklist = studentAwardPeriodEntity.IsIgnoredOnChecklist
                };
                studentAwardDto.StudentAwardPeriods.Add(studentAwardPeriodDto);
            }

            return studentAwardDto;
        }
    }
}
