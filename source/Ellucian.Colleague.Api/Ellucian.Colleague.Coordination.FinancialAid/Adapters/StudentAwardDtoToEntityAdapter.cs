/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// This adapter converts a StudentAward Dto to a StudentAward Entity. This class extends BaseAdapter and overrides
    /// the MapToType method. The StudentAward and StudentAwardPeriod Entities do not have default constructors, so they
    /// must be build manually by the MapToType method
    /// </summary>
    public class StudentAwardDtoToEntityAdapter
    {

        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        /// <summary>
        /// Constructor for the StudentAwardDtoToEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public StudentAwardDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            Logger = logger;
        }

        /// <summary>
        /// The MapToType method converts a StudentAward Dto to a StudentAward Entity. The StudentAwardPeriods attribute
        /// of the StudentAward objects are also converted from Dto to Entity.
        /// </summary>
        /// <param name="source">The StudentAward Dto to convert</param>
        /// <returns>A StudentAward Entity object</returns>
        public Domain.FinancialAid.Entities.StudentAward MapToType(Dtos.FinancialAid.StudentAward source,
            Domain.FinancialAid.Entities.StudentAwardYear studentAwardYearEntity,
            Domain.FinancialAid.Entities.Award awardEntity,
            IEnumerable<Domain.FinancialAid.Entities.AwardStatus> allAwardStatusEntities)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (studentAwardYearEntity == null)
            {
                throw new ArgumentNullException("studentAwardYearEntity");
            }
            if (studentAwardYearEntity.Code != source.AwardYearId)
            {
                throw new InvalidOperationException("StudentAwardYearEntity must apply to the awardYear of the source: " + source.AwardYearId);
            }
            if (studentAwardYearEntity.StudentId != source.StudentId)
            {
                throw new InvalidOperationException("StudentAwardYearEntity must apply to the studentId of the source: " + source.StudentId);
            }
            if (awardEntity == null)
            {
                throw new ArgumentNullException("awardEntity");
            }
            if (awardEntity.Code != source.AwardId)
            {
                throw new InvalidOperationException("AwardEntity must apply to the awardId of the source: " + source.AwardId);
            }
            if (allAwardStatusEntities == null)
            {
                throw new ArgumentNullException("allAwardStatusEntities");
            }

            //Create the StudentAward Entity
            var studentAwardEntity = new Domain.FinancialAid.Entities.StudentAward(studentAwardYearEntity, source.StudentId, awardEntity, source.IsEligible)
                {
                    PendingChangeRequestId = source.PendingChangeRequestId
                };

            //Create a StudentAwardPeriod entity for each of the StudentAwardPeriod Dtos.
            foreach (var studentAwardPeriod in source.StudentAwardPeriods)
            {

                var awardStatusEntity = allAwardStatusEntities.FirstOrDefault(s => s.Code == studentAwardPeriod.AwardStatusId);

                var studentAwardPeriodEntity = new Domain.FinancialAid.Entities.StudentAwardPeriod(studentAwardEntity, studentAwardPeriod.AwardPeriodId, awardStatusEntity, studentAwardPeriod.IsFrozen, studentAwardPeriod.IsTransmitted);
                studentAwardPeriodEntity.AwardAmount = studentAwardPeriod.AwardAmount;
            }

            return studentAwardEntity;
        }

    }
}
