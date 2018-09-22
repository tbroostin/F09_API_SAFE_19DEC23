//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
   
    /// <summary>
    /// Adapter that converts AwardLetter DTO to AwardLetter Entity.
    /// </summary>
    public class AwardLetter2DtoToEntityAdapter
    {
        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        /// <summary>
        /// Constructor for the AwardLetterDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public AwardLetter2DtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
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
        public Domain.FinancialAid.Entities.AwardLetter2 MapToType(Dtos.FinancialAid.AwardLetter2 awardLetterDto,
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
            if (awardLetterDto.AwardLetterYear != studentAwardYearEntity.Code)
            {
                throw new ArgumentException("studentAwardYearEntity must apply to the awardLetterYear of the awardLetterDto: " + awardLetterDto.AwardLetterYear, "studentAwardYearEntity");
            }

            //Create award letter entity
            var awardLetterEntity = new Domain.FinancialAid.Entities.AwardLetter2(awardLetterDto.StudentId, studentAwardYearEntity);

            //Accepted date is important, make sure it is assigned
            awardLetterEntity.AcceptedDate = awardLetterDto.AcceptedDate;
            //awardLetterEntity.AwardLetterYear = awardLetterDto.AwardLetterYear;
            awardLetterEntity.Id = awardLetterDto.Id;

            return awardLetterEntity;
        }
    }
}
