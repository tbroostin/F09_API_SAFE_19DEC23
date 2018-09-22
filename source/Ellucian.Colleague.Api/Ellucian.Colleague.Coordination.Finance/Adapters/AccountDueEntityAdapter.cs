// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;


namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class AccountDueEntityAdapter : BaseAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue>
    {
        public AccountDueEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        // Since AutoMapper cannot be used here, inherit from BaseAdapter and override the MapToType method for custom mapping
        public override Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue MapToType(Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue Source)
        {
            // You could add validation code here before passing the properties to the Degree Plan entity constructor

            var accountDueDto = new Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue();
            accountDueDto.EndDate = Source.EndDate;
            accountDueDto.StartDate = Source.StartDate;
            accountDueDto.PersonName = Source.PersonName;

            var accountsReceivableItemAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountsReceivableDueItem>();
            var invoiceItemAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem, Ellucian.Colleague.Dtos.Finance.AccountDue.InvoiceDueItem>();
            var paymentPlanAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem, Ellucian.Colleague.Dtos.Finance.AccountDue.PaymentPlanDueItem>();

            foreach (var term in Source.AccountTerms)
            {
                var accountTermDto = new Ellucian.Colleague.Dtos.Finance.AccountDue.AccountTerm();
                accountTermDto.Amount = term.Amount;
                accountTermDto.TermId = term.TermId;
                accountTermDto.Description = term.Description;

                var accountsReceivableItems = term.AccountDetails.Where(detail => detail is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem && !(detail is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem) && !(detail is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem));
                foreach (var entity in accountsReceivableItems)
                {
                    var newDto = accountsReceivableItemAdapter.MapToType(entity);
                    accountTermDto.GeneralItems.Add(newDto);
                }

                var invoiceItems = term.AccountDetails.Where(detail => detail is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem).Cast<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem>();
                foreach (var entity in invoiceItems)
                {
                    var newDto = invoiceItemAdapter.MapToType(entity);
                    accountTermDto.InvoiceItems.Add(newDto);
                }

                var paymentPlanItems = term.AccountDetails.Where(detail => detail is Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem).Cast<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem>();
                foreach (var entity in paymentPlanItems)
                {
                    var newDto = paymentPlanAdapter.MapToType(entity);
                    accountTermDto.PaymentPlanItems.Add(newDto);
                }

                accountDueDto.AccountTerms.Add(accountTermDto);
            }

            return accountDueDto;
        }
    }
}