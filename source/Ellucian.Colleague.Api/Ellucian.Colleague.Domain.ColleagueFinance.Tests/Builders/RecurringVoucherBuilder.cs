using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class RecurringVoucherBuilder
    {
        #region Private properties
        private string recurringVoucherId;
        private DateTime date;
        private RecurringVoucherStatus status;
        private DateTime statusDate;
        private string vendorName;
        private string invoiceNumber;
        private DateTime invoiceDate;
        #endregion

        public RecurringVoucher RecurringVoucher;

        public RecurringVoucherBuilder()
        {
            this.recurringVoucherId = "RV000001";
            this.date = new DateTime(2015, 01, 01);
            this.status = RecurringVoucherStatus.Outstanding;
            this.statusDate = new DateTime(2015, 01, 02);
            this.vendorName = "Susty Corporation";
            this.invoiceNumber = "IN12345";
            this.invoiceDate = new DateTime(2015, 01, 03);

            BuildRecurringVoucher();
        }

        public RecurringVoucher Build()
        {
            return this.RecurringVoucher;
        }

        public RecurringVoucherBuilder WithInvoiceNumber(string invoiceNumberParam)
        {
            this.invoiceNumber = invoiceNumberParam;
            BuildRecurringVoucher();
            return this;
        }

        private void BuildRecurringVoucher()
        {
            this.RecurringVoucher = new RecurringVoucher(this.recurringVoucherId, this.date, this.status, this.statusDate,
                this.vendorName, this.invoiceNumber, this.invoiceDate);
        }
    }
}
