using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementReportDataContextTests
    {
        public DateTime periodEndDate;
        public PayStatementSourceData payStatementSource
        {
            get
            {
                return new PayStatementSourceData("12345",
                    "0003914",
                    "Matt DeDiana",
                    "555-55-5555",
                    new PayStatementAddress[2] { new PayStatementAddress("5 5th Ave"), new PayStatementAddress("Essex Junction, VT 05402") },
                    "1111",
                    "2222",
                    periodEndDate.AddDays(1),
                    periodEndDate,
                    555,
                    500,
                    5555,
                    5000,
                    "stuff and things");
            }
        }
        public PayrollRegisterEntry payrollRegisterEntry
        {
            get
            {
                return new PayrollRegisterEntry("54321", "0003914", periodEndDate.AddMonths(-1).AddDays(1), periodEndDate, "BW", 1, "1111", "2222", true);
            }
        }

        public List<PersonBenefitDeduction> personBenefitDeductions;
        public List<PersonEmploymentStatus> personEmploymentStatuses;

        public PayStatementReportDataContext context
        {
            get
            {
                return new PayStatementReportDataContext(payStatementSource, payrollRegisterEntry, personBenefitDeductions, personEmploymentStatuses);
            }
        }

        [TestInitialize]
        public void Initialize()
        {

            periodEndDate = new DateTime(2017, 7, 31);
            personBenefitDeductions = new List<PersonBenefitDeduction>();
            personEmploymentStatuses = new List<PersonEmploymentStatus>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayStatementSourceRequiredTest()
        {
            new PayStatementReportDataContext(null, payrollRegisterEntry, personBenefitDeductions, personEmploymentStatuses);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayrollRegisterRequiredTest()
        {
            new PayStatementReportDataContext(payStatementSource, null, personBenefitDeductions, personEmploymentStatuses);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonBenefitDeductionsRequiredTest()
        {
            new PayStatementReportDataContext(payStatementSource, payrollRegisterEntry, null, personEmploymentStatuses);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PayStatementAndPayrollRegisterMustShareReferenceKeyTest()
        {
            var invalidRegister = new PayrollRegisterEntry("54321", "0003914", periodEndDate, periodEndDate, "BW", 1, "foo", "bar", false);
            new PayStatementReportDataContext(payStatementSource, invalidRegister, personBenefitDeductions, personEmploymentStatuses);
        }

        [TestMethod]
        public void SourceDataTest()
        {
            new PayStatementReportDataContext(payStatementSource, payrollRegisterEntry, personBenefitDeductions, personEmploymentStatuses);
            Assert.AreEqual(payStatementSource, context.sourceData);
        }

        [TestMethod]
        public void RegisterEntryTest()
        {
            new PayStatementReportDataContext(payStatementSource, payrollRegisterEntry, personBenefitDeductions, personEmploymentStatuses);
            Assert.AreEqual(payrollRegisterEntry, context.payrollRegisterEntry);
        }

        [TestMethod]
        public void EmptyPersonBenefitDeductionsTest()
        {
            personBenefitDeductions = new List<PersonBenefitDeduction>();

            Assert.IsFalse(context.personBenefitDeductions.Any());
        }

        [TestMethod]
        public void BenefitStartsBeforePeriod_NoEndTest()
        {
            personBenefitDeductions.Add(new PersonBenefitDeduction("003914", "foo", periodEndDate.AddDays(-1)));

            Assert.IsTrue(context.personBenefitDeductions.Any());
            Assert.AreEqual(personBenefitDeductions[0].BenefitDeductionId, context.personBenefitDeductions.First().BenefitDeductionId);
        }

        [TestMethod]
        public void BenefitStartsOnPeriodEnd_NoEndTest()
        {
            personBenefitDeductions.Add(new PersonBenefitDeduction("003914", "foo", periodEndDate));

            Assert.IsTrue(context.personBenefitDeductions.Any());
            Assert.AreEqual(personBenefitDeductions[0].BenefitDeductionId, context.personBenefitDeductions.First().BenefitDeductionId);
        }

        [TestMethod]
        public void BenefitStartsAfterPeriodEnd_NoEndTest()
        {
            personBenefitDeductions.Add(new PersonBenefitDeduction("003914", "foo", periodEndDate.AddDays(1)));

            Assert.IsFalse(context.personBenefitDeductions.Any());
        }

        [TestMethod]
        public void BenefitStartsBeforePeriod_LastPayDateAfterPeriodTest()
        {
            personBenefitDeductions.Add(new PersonBenefitDeduction("003914", "foo", periodEndDate.AddDays(-1), lastPayDate: periodEndDate.AddDays(1)));

            Assert.IsTrue(context.personBenefitDeductions.Any());
            Assert.AreEqual(personBenefitDeductions[0].BenefitDeductionId, context.personBenefitDeductions.First().BenefitDeductionId);
        }

        [TestMethod]
        public void BenefitStartsBeforePeriod_LastPayDateOnPeriodTest()
        {
            personBenefitDeductions.Add(new PersonBenefitDeduction("003914", "foo", periodEndDate.AddDays(-1), lastPayDate: periodEndDate));

            Assert.IsTrue(context.personBenefitDeductions.Any());
            Assert.AreEqual(personBenefitDeductions[0].BenefitDeductionId, context.personBenefitDeductions.First().BenefitDeductionId);
        }

        [TestMethod]
        public void BenefitStartsBeforePeriod_LastPayDateBeforePeriodTest()
        {
            personBenefitDeductions.Add(new PersonBenefitDeduction("003914", "foo", periodEndDate.AddDays(-1), lastPayDate: periodEndDate.AddDays(-1)));

            Assert.IsFalse(context.personBenefitDeductions.Any());
        }

        //
        [TestMethod]
        public void EmptyPersonEmploymentStatusTest()
        {
            Assert.IsNull(context.personEmploymentStatus);
        }

        [TestMethod]
        public void EmploymentStatusStartsBeforePeriod_NoEndTest()
        {
            personEmploymentStatuses.Add(new PersonEmploymentStatus("foo", "003914", "PRIPOS", "2222", periodEndDate.AddDays(-1), null));
            Assert.AreEqual(personEmploymentStatuses[0].Id, context.personEmploymentStatus.Id);
        }

        [TestMethod]
        public void EmploymentStatusStartsOnPeriodEnd_NoEndTest()
        {
            personEmploymentStatuses.Add(new PersonEmploymentStatus("foo", "003914", "PRIPOS", "2222", periodEndDate, null));
            Assert.AreEqual(personEmploymentStatuses[0].Id, context.personEmploymentStatus.Id);
        }

        [TestMethod]
        public void EmploymentStatusStartsAfterPeriodEnd_NoEndTest()
        {
            personEmploymentStatuses.Add(new PersonEmploymentStatus("foo", "003914", "PRIPOS", "2222", periodEndDate.AddDays(1), null));
            Assert.IsNull(context.personEmploymentStatus);
        }

        [TestMethod]
        public void EmploymentStatusStartsBeforePeriod_EndDateAfterPeriodTest()
        {
            personEmploymentStatuses.Add(new PersonEmploymentStatus("foo", "003914", "PRIPOS", "2222", periodEndDate.AddDays(-1), periodEndDate.AddDays(1)));
            Assert.AreEqual(personEmploymentStatuses[0].Id, context.personEmploymentStatus.Id);
        }

        [TestMethod]
        public void EmploymentStatusStartsBeforePeriod_EndDateOnPeriodTest()
        {
            personEmploymentStatuses.Add(new PersonEmploymentStatus("foo", "003914", "PRIPOS", "2222", periodEndDate.AddDays(-1), periodEndDate));
            Assert.AreEqual(personEmploymentStatuses[0].Id, context.personEmploymentStatus.Id);
        }

        [TestMethod]
        public void EmploymentStatusStartsBeforePeriod_EndPayDateBeforePeriodTest()
        {
            personEmploymentStatuses.Add(new PersonEmploymentStatus("foo", "003914", "PRIPOS", "2222", periodEndDate.AddMonths(-3), periodEndDate.AddMonths(-2)));
            Assert.IsNull(context.personEmploymentStatus);
        }

    }
}
