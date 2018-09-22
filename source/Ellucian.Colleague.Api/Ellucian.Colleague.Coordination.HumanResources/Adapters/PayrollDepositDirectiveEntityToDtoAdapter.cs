/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    public class PayrollDepositDirectiveEntityToDtoAdapter : BaseAdapter<Dtos.Base.PayrollDepositDirective, Domain.HumanResources.Entities.PayrollDepositDirective>   
    {
        public PayrollDepositDirectiveEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Map a PayrollDepositDirective Data Transfer Object to a PayrollDepositDirective Entity
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Domain.HumanResources.Entities.PayrollDepositDirective MapToType(Dtos.Base.PayrollDepositDirective source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            // use automapper to map BankAccountType
            var bankAccountTypeDtoToEntityAdapter = adapterRegistry.GetAdapter<Dtos.Base.BankAccountType, Domain.Base.Entities.BankAccountType>();
            var timestampDtoToEntityAdapter = adapterRegistry.GetAdapter<Dtos.Base.Timestamp, Domain.Base.Entities.Timestamp>();
            // create a US or CA directive
            Domain.HumanResources.Entities.PayrollDepositDirective payrollDepositDirective = null;
            if (!string.IsNullOrEmpty(source.RoutingId))
            {
                payrollDepositDirective = new Domain.HumanResources.Entities.PayrollDepositDirective(
                    source.Id,
                    source.PersonId,
                    source.RoutingId,
                    source.BankName,
                    bankAccountTypeDtoToEntityAdapter.MapToType(source.BankAccountType),
                    source.AccountIdLastFour,
                    source.Nickname,
                    source.IsVerified,
                    source.Priority,
                    source.DepositAmount,
                    source.StartDate,
                    source.EndDate,
                    timestampDtoToEntityAdapter.MapToType(source.Timestamp)
                );
            }
            else
            {
                payrollDepositDirective = new Domain.HumanResources.Entities.PayrollDepositDirective(
                    source.Id,
                    source.PersonId,
                    source.InstitutionId,
                    source.BranchNumber,
                    source.BankName,
                    bankAccountTypeDtoToEntityAdapter.MapToType(source.BankAccountType),
                    source.AccountIdLastFour,
                    source.Nickname,
                    source.IsVerified,
                    source.Priority,
                    source.DepositAmount,
                    source.StartDate,
                    source.EndDate,
                    timestampDtoToEntityAdapter.MapToType(source.Timestamp)
                );
            }

            if(!string.IsNullOrWhiteSpace(source.NewAccountId))
            {
                payrollDepositDirective.SetNewAccountId(source.NewAccountId);
            }

            return payrollDepositDirective;
        }
    }
}
