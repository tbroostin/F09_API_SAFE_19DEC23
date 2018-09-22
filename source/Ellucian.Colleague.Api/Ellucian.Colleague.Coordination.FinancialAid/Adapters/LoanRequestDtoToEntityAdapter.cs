/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom Adapter to convert a LoanRequest DTO to a LoanRequest Domain Entity
    /// </summary>
    public class LoanRequestDtoToEntityAdapter : BaseAdapter<Dtos.FinancialAid.LoanRequest, Domain.FinancialAid.Entities.LoanRequest>
    {
        /// <summary>
        /// Instantiate a new LoanRequestDtoToEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="logger">Logger</param>
        public LoanRequestDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Map a LoanRequest DTO to a LoanRequest Domain Entity
        /// </summary>
        /// <param name="Source">LoanRequest DTO to convert to a Domain Entity</param>
        /// <returns>LoanRequest Domain Entity converted from the input LoanRequest DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown if the Source argument is null</exception>
        public override Domain.FinancialAid.Entities.LoanRequest MapToType(Dtos.FinancialAid.LoanRequest Source)
        {
            if (Source == null)
            {
                throw new ArgumentNullException("Source");
            }

            Domain.FinancialAid.Entities.LoanRequestStatus domainLoanRequestStatus;
            switch (Source.Status)
            {
                case Dtos.FinancialAid.LoanRequestStatus.Accepted:
                    domainLoanRequestStatus = Domain.FinancialAid.Entities.LoanRequestStatus.Accepted;
                    break;
                case Dtos.FinancialAid.LoanRequestStatus.Pending:
                    domainLoanRequestStatus = Domain.FinancialAid.Entities.LoanRequestStatus.Pending;
                    break;
                case Dtos.FinancialAid.LoanRequestStatus.Rejected:
                    domainLoanRequestStatus = Domain.FinancialAid.Entities.LoanRequestStatus.Rejected;
                    break;
                default:
                    var message = string.Format("Unable to convert Dto LoanRequestStatus {0} to Domain LoanRequestStatus", Source.Status);
                    logger.Error(message);
                    throw new ApplicationException(message);
            }

            var loanRequestEntity = new Domain.FinancialAid.Entities.LoanRequest(Source.Id, Source.StudentId, Source.AwardYear, Source.RequestDate,
                Source.TotalRequestAmount, Source.AssignedToId, domainLoanRequestStatus, Source.StatusDate, string.Empty);

            if(Source.LoanRequestPeriods == null || Source.LoanRequestPeriods.Count == 0)
            {
                throw new ArgumentException("LoanRequestPeriods list cannot be empty", "Source");
            }

            foreach(var loanPeriod in Source.LoanRequestPeriods)
            {
                if (!loanRequestEntity.AddLoanPeriod(loanPeriod.Code, loanPeriod.LoanAmount))
                {
                    throw new InvalidOperationException("Error mapping DTO to entity: could not add one of the loan periods");
                }
            }

            loanRequestEntity.StudentComments = Source.StudentComments;

            return loanRequestEntity;

        }
    }
}
