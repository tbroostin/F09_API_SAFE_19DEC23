using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class RecurringVoucherScheduleTests
    {
        #region Initialize and Cleanup
        private RecurringVoucherSchedule schedule;
        private RecurringVoucherScheduleBuilder scheduleBuilder = new RecurringVoucherScheduleBuilder();

        [TestInitialize]
        public void Initialize()
        {
            schedule = scheduleBuilder.Build();
        }

        [TestCleanup]
        public void Cleanup()
        {
            schedule = null;
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void RecurringVoucherSchedule()
        {
            Assert.AreEqual(scheduleBuilder.Schedule.Date, schedule.Date);
            Assert.AreEqual(scheduleBuilder.Schedule.Amount, schedule.Amount);
        }

        [TestMethod]
        public void VoucherId()
        {
            string voucherId = "V0000001";
            schedule.VoucherId = voucherId;
            Assert.AreEqual(voucherId, schedule.VoucherId, "Voucher ID should be properly initialized.");
            Assert.AreEqual(null, schedule.PurgedVoucherId, "PurgedVoucherId should be null.");
        }

        [TestMethod]
        public void PurgedVoucherId()
        {
            string purgedVoucherId = "V0000002";
            schedule.PurgedVoucherId = purgedVoucherId;
            Assert.AreEqual(purgedVoucherId, schedule.PurgedVoucherId, "PurgedVoucherId should be properly initialized.");
            Assert.AreEqual(null, schedule.VoucherId, "VoucherId should be null.");
        }
        #endregion
    }
}
