/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    public class PayStatementReportEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.PayStatementReport, Dtos.HumanResources.PayStatementReport>
    {
        public PayStatementReportEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.HourlySalaryIndicator, Dtos.HumanResources.HourlySalaryIndicator>();
            AddMappingDependency<Domain.HumanResources.Entities.PayStatementDeductionType, Dtos.HumanResources.PayStatementDeductionType>();
            AddMappingDependency<Domain.Base.Entities.BankAccountType, Dtos.Base.BankAccountType>();
            AddMappingDependency<Domain.HumanResources.Entities.PayStatementEarnings, Dtos.HumanResources.PayStatementEarnings>();            
            AddMappingDependency<Domain.HumanResources.Entities.PayStatementDeduction, Dtos.HumanResources.PayStatementDeduction>();
            AddMappingDependency<Domain.HumanResources.Entities.PayStatementBankDeposit, Dtos.HumanResources.PayStatementBankDeposit>();
            AddMappingDependency<Domain.HumanResources.Entities.PayStatementLeave, Dtos.HumanResources.PayStatementLeave>();
            AddMappingDependency<Domain.HumanResources.Entities.PayStatementAddress, Dtos.HumanResources.PayStatementAddress>();
            AddMappingDependency<Domain.HumanResources.Entities.PayStatementTaxableBenefit, Dtos.HumanResources.PayStatementTaxableBenefit>();
        }
    }
}
