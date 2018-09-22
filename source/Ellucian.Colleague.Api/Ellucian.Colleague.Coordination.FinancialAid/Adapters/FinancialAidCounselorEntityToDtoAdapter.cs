//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom adapter maps a Staff Entity to a FinancialAidCounselor Dto
    /// </summary>
    public class FinancialAidCounselorEntityToDtoAdapter : BaseAdapter<Domain.Base.Entities.Staff, Dtos.FinancialAid.FinancialAidCounselor>
    {

        /// <summary>
        /// Instantiate a FinancialAidCounselorDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="logger">Logger</param>
        public FinancialAidCounselorEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Map a Staff Entity to a FinancialAidCounselor Dto
        /// </summary>
        /// <param name="source">Staff object to map to the FinancialAidCounselor object</param>
        /// <returns>FinancialAidCounselorDto</returns>
        /// <exception cref="ArgumentNullException">Thrown if the source argument is null</exception>
        public override Dtos.FinancialAid.FinancialAidCounselor MapToType(Domain.Base.Entities.Staff source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var counselor = new Dtos.FinancialAid.FinancialAidCounselor();
            counselor.Id = source.Id;
            counselor.Name = source.PreferredName;
            counselor.EmailAddress = (source.PreferredEmailAddress != null) ? source.PreferredEmailAddress.Value : string.Empty;

            return counselor;
        }

    }
}
