//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Adapter that converts AwardLetter DTO to AwardLetter Entity.
    /// </summary>
    public class AwardLetterDtoToEntityAdapter
    {
        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        /// <summary>
        /// Constructor for the AwardLetterDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public AwardLetterDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            Logger = logger;
        }

        /// <summary>
        /// Map an AwardLetter DTO to an AwardLetter entity
        /// </summary>
        /// <param name="awardLetterDto">award letter dto</param>
        /// <param name="studentAwardYearEntity">student award year entity</param>
        /// <returns>award letter entity</returns>
        public Domain.FinancialAid.Entities.AwardLetter MapToType(Dtos.FinancialAid.AwardLetter awardLetterDto, 
            Domain.FinancialAid.Entities.StudentAwardYear studentAwardYearEntity)
        {
            if (awardLetterDto == null)
            {
                throw new ArgumentNullException("awardLetterDto");
            }
            if (studentAwardYearEntity == null)
            {
                throw new ArgumentNullException("studentAwardYearEntity");
            }
            if (awardLetterDto.AwardYearCode != studentAwardYearEntity.Code)
            {
                throw new ArgumentException("studentAwardYearEntity must apply to the awardYear of the awardLetterDto: " + awardLetterDto.AwardYearCode, "studentAwardYearEntity");
            }
           
            //Create award letter entity
            var awardLetterEntity = new Domain.FinancialAid.Entities.AwardLetter(awardLetterDto.StudentId, studentAwardYearEntity);

            //Accepted date is important, make sure it is assigned
            awardLetterEntity.AcceptedDate = awardLetterDto.AcceptedDate;
           
            return awardLetterEntity;
        }
    }
}
