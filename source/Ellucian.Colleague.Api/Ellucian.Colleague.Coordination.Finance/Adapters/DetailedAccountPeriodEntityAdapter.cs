// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class DetailedAccountPeriodEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Ellucian.Colleague.Dtos.Finance.AccountActivity.DetailedAccountPeriod>
    {
        public DetailedAccountPeriodEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            // Activity Items
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDateTermItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDateTermItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityDepositItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityDepositItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidTerm, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidTerm>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityFinancialAidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityFinancialAidItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRemainingAmountItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentMethodItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentMethodItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPaidItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPaidItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanScheduleItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanScheduleItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityPaymentPlanDetailsItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityPaymentPlanDetailsItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRemainingAmountItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRemainingAmountItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityRoomAndBoardItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityRoomAndBoardItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivitySponsorPaymentItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivitySponsorPaymentItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityTuitionItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityTuitionItem>();

            // Charge Types
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FeeType, Ellucian.Colleague.Dtos.Finance.AccountActivity.FeeType>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.OtherType, Ellucian.Colleague.Dtos.Finance.AccountActivity.OtherType>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RoomAndBoardType, Ellucian.Colleague.Dtos.Finance.AccountActivity.RoomAndBoardType>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionBySectionType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionBySectionType>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.TuitionByTotalType, Ellucian.Colleague.Dtos.Finance.AccountActivity.TuitionByTotalType>();

            // Categories
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ChargesCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.ChargesCategory>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DepositCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.DepositCategory>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.FinancialAidCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.FinancialAidCategory>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.PaymentPlanCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.PaymentPlanCategory>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.RefundCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.RefundCategory>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.SponsorshipCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.SponsorshipCategory>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.StudentPaymentCategory, Ellucian.Colleague.Dtos.Finance.AccountActivity.StudentPaymentCategory>();
        }
    }
}
