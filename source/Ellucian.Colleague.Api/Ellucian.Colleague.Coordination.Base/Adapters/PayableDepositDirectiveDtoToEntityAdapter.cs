/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Base;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter to convert a PayableDepositDirective DTO to a PayableDepositDirective Entity
    /// </summary>
    public class PayableDepositDirectiveDtoToEntityAdapter 
        : AutoMapperAdapter<Dtos.Base.PayableDepositDirective, Domain.Base.Entities.PayableDepositDirective>
    {
        /// <summary>
        /// Instantiate a new PayableDepositDirectiveDtoToEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public PayableDepositDirectiveDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Base.Timestamp, Domain.Base.Entities.Timestamp>();
            AddMappingDependency<Dtos.Base.BankAccountType, Domain.Base.Entities.BankAccountType>();
        }

        /// <summary>
        /// Map a PayableDepositDirective DTO to a PayableDepositDirective Entity
        /// </summary>
        /// <param name="Source">PayableDepositDirective DTO to convert to a Domain Entity</param>
        /// <returns>PayableDepositDirective Domain Entity converted from the input PayableDepositDirective DTO</returns>
        public override Domain.Base.Entities.PayableDepositDirective MapToType(Dtos.Base.PayableDepositDirective Source)
        {
            if (Source == null)
            {
                throw new ArgumentNullException("Source");
            }

            var bankAccountTypeDtoToEntityAdapter = adapterRegistry.GetAdapter<Dtos.Base.BankAccountType, Domain.Base.Entities.BankAccountType>();
            var timestampDtoToEntityAdapter = adapterRegistry.GetAdapter<Dtos.Base.Timestamp, Domain.Base.Entities.Timestamp>();

            Domain.Base.Entities.PayableDepositDirective payableDepositDirectiveEntity = null;
            if (!string.IsNullOrEmpty(Source.RoutingId))
            {
                payableDepositDirectiveEntity =
                    new Domain.Base.Entities.PayableDepositDirective(
                        Source.Id,
                        Source.PayeeId,
                        Source.RoutingId,
                        Source.BankName,
                        bankAccountTypeDtoToEntityAdapter.MapToType(Source.BankAccountType),
                        Source.AccountIdLastFour,
                        Source.Nickname,
                        Source.IsVerified,
                        Source.AddressId,
                        Source.StartDate,
                        Source.EndDate,
                        Source.IsElectronicPaymentRequested,
                        timestampDtoToEntityAdapter.MapToType(Source.Timestamp));
            }
            else
            {
                payableDepositDirectiveEntity =
                    new Domain.Base.Entities.PayableDepositDirective(
                        Source.Id,
                        Source.PayeeId,
                        Source.InstitutionId,
                        Source.BranchNumber,
                        Source.BankName,
                        bankAccountTypeDtoToEntityAdapter.MapToType(Source.BankAccountType),
                        Source.AccountIdLastFour,
                        Source.Nickname,
                        Source.IsVerified,
                        Source.AddressId,
                        Source.StartDate,
                        Source.EndDate,
                        Source.IsElectronicPaymentRequested,
                        timestampDtoToEntityAdapter.MapToType(Source.Timestamp));
            }

            if (!string.IsNullOrEmpty(Source.NewAccountId))
            {
                payableDepositDirectiveEntity.SetNewAccountId(Source.NewAccountId);
            }

            return payableDepositDirectiveEntity;
        }
    }
}
