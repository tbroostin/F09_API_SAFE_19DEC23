/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
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
    /// Outside award dto to entity adapter
    /// </summary>
    public class OutsideAwardDtoToEntityAdapter : BaseAdapter<Dtos.FinancialAid.OutsideAward, Domain.FinancialAid.Entities.OutsideAward>
    {
        /// <summary>
        /// Instantiate a new OutsideAwardDtoToEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public OutsideAwardDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) { }

        /// <summary>
        /// Map an OutsideAward DTO to OutsideAward entity
        /// </summary>
        /// <param name="source">OutsideAward DTO to map to OutsideAward entity</param>
        /// <returns>OutsideAward entity</returns>
        public override Domain.FinancialAid.Entities.OutsideAward MapToType(Dtos.FinancialAid.OutsideAward source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Domain.FinancialAid.Entities.OutsideAward outsideAwardEntity;
            try
            {
                if (string.IsNullOrEmpty(source.Id))
                {
                    outsideAwardEntity = new Domain.FinancialAid.Entities.OutsideAward(source.StudentId, source.AwardYearCode, source.AwardName, source.AwardType, source.AwardAmount, source.AwardFundingSource);
                }
                else
                {
                    outsideAwardEntity = new Domain.FinancialAid.Entities.OutsideAward(source.Id,
                    source.StudentId, source.AwardYearCode, source.AwardName, source.AwardType, source.AwardAmount, source.AwardFundingSource);
                }                
            }
            catch (Exception e)
            {
                string message = "Error mapping OutsideAward DTO to entity";
                logger.Error(e, message);
                throw new InvalidOperationException(message, e);
            }

            return outsideAwardEntity;
        }
    }
}
