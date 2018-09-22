using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class RecurringVoucherScheduleBuilder
    {
        private DateTime date;
        private decimal amount;
        public RecurringVoucherSchedule Schedule;

        public RecurringVoucherScheduleBuilder()
        {
            this.amount = 150.54m;
            BuildRecurringVoucherSchedule();
        }

        public RecurringVoucherSchedule Build()
        {
            return this.Schedule;
        }

        public RecurringVoucherScheduleBuilder WithDate(DateTime date)
        {
            this.date = date;
            BuildRecurringVoucherSchedule();
            return this;
        }

        public RecurringVoucherScheduleBuilder WithAmount(decimal amount)
        {
            this.amount = amount;
            BuildRecurringVoucherSchedule();
            return this;
        }

        private void BuildRecurringVoucherSchedule()
        {
            this.Schedule = new RecurringVoucherSchedule(this.date, this.amount);
        }
    }
}
