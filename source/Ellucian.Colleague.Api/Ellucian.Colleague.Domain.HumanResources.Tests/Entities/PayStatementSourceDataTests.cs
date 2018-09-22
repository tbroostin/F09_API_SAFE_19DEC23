/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
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
    public class PayStatementSourceDataTests
    {
        string id;
        string employeeId;
        string employeeName;
        string employeeSsn;
        List<PayStatementAddress> employeeMailingLabel;        
        string checkReferenceId;
        string statementReferenceId;
        DateTime payDate;
        DateTime periodEndDate;
        decimal periodGrossPay;
        decimal periodNetPay;
        decimal ytdGrossPay;
        decimal ytdNetPay;
        string comments;

        PayStatementSourceData sourceData
        {
            get
            {
                return new PayStatementSourceData(id, employeeId, employeeName, employeeSsn, employeeMailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            id = "14300";
            employeeId = "24601";
            employeeName = "Matt Dediana";
            employeeSsn = "999-99-9999";
            employeeMailingLabel = new List<PayStatementAddress>() { new PayStatementAddress("1234 5th Ave."), new PayStatementAddress("Technology, ZZ 78901") };
            checkReferenceId = "ref";
            statementReferenceId = "ref-008";
            payDate = new DateTime(2017, 8, 1);
            periodEndDate = new DateTime(2017, 7, 31);
            periodGrossPay = 1114.14M;
            periodNetPay = 1023.11M;
            ytdGrossPay = 12202.43M;
            ytdNetPay = 10992.22M;
            comments = "comments here";

        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            //sourceData = new PayStatementSourceData(id, employeeId, employeeName, employeeSsn, employeeMailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            Assert.AreEqual(id, sourceData.Id);
            Assert.AreEqual(employeeId, sourceData.EmployeeId);
            Assert.AreEqual(employeeName, sourceData.EmployeeName);
            Assert.AreEqual(employeeSsn, sourceData.EmployeeSSN);
            Assert.AreEqual(employeeMailingLabel, sourceData.EmployeeMailingLabel);
            Assert.AreEqual(statementReferenceId, sourceData.StatementReferenceId);
            Assert.AreEqual(payDate, sourceData.PayDate);
            Assert.AreEqual(periodEndDate, sourceData.PeriodEndDate);
            Assert.AreEqual(periodGrossPay, sourceData.PeriodGrossPay);
            Assert.AreEqual(periodNetPay, sourceData.PeriodNetPay);
            Assert.AreEqual(ytdGrossPay, sourceData.YearToDateGrossPay);
            Assert.AreEqual(ytdNetPay, sourceData.YearToDateNetPay);
            Assert.AreEqual(comments, sourceData.Comments);
            Assert.IsFalse(sourceData.SourceBankDeposits.Any());

        }

        [TestMethod]
        public void StringIdIsConvertedToInteger()
        {
            id = "14397";
            //sourceData = new PayStatementSourceData(id, employeeId, employeeName, employeeSsn, employeeMailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            Assert.AreEqual(14397, sourceData.IdNumber);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullIdError()
        {
            //sourceData = new PayStatementSourceData(null, employeeId, employeeName, employeeSsn, employeeMailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            id = null;
            var error = sourceData;
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullEmployeeIdError()
        {
            //sourceData = new PayStatementSourceData(id, null, employeeName, employeeSsn, employeeMailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            employeeId = null;
            var error = sourceData;
        }

        [TestMethod]
        public void NullEmployeeNameTest()
        {
            //sourceData = new PayStatementSourceData(id, employeeId, null, employeeSsn, employeeMailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            employeeName = null;
            Assert.IsNull(sourceData.EmployeeName);
        }

        [TestMethod]
        public void NullEmployeeSsnTest()
        {
            //sourceData = new PayStatementSourceData(id, employeeId, employeeName, null, employeeMailingLabel, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            employeeSsn = null;
            Assert.IsNull(sourceData.EmployeeSSN);
        }

        [TestMethod]
        public void NullMailingLabelTest()
        {
            //sourceData = new PayStatementSourceData(id, employeeId, employeeName, employeeSsn, null, checkReferenceId, statementReferenceId, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            employeeMailingLabel = null;
            Assert.IsNotNull(sourceData.EmployeeMailingLabel);
            Assert.IsFalse(sourceData.EmployeeMailingLabel.Any());
        }

        [TestMethod]
        public void ReferenceKeyTest()
        {
            Assert.AreEqual((statementReferenceId + "*" + checkReferenceId), sourceData.ReferenceKey);
        }

        [TestMethod]
        public void NoStatementReferenceIdReferenceKeyTest()
        {
            //sourceData = new PayStatementSourceData(id, employeeId, employeeName, employeeSsn, employeeMailingLabel, null, null, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            statementReferenceId = null;
            Assert.AreEqual(("*" + checkReferenceId), sourceData.ReferenceKey);
        }

        [TestMethod]
        public void NoPaycheckReferenceIdReferenceKeyTest()
        {
            //sourceData = new PayStatementSourceData(id, employeeId, employeeName, employeeSsn, employeeMailingLabel, null, null, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            checkReferenceId = null;
            Assert.AreEqual((statementReferenceId + "*"), sourceData.ReferenceKey);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void NullStatementReferenceIdError()
        {
            //sourceData = new PayStatementSourceData(id, employeeId, employeeName, employeeSsn, employeeMailingLabel, null, null, payDate, periodEndDate, periodGrossPay, periodNetPay, ytdGrossPay, ytdNetPay, comments);
            checkReferenceId = null;
            statementReferenceId = null;
            var error = sourceData;
        }

        [TestMethod]
        public void Equals()
        {
            var sd1 = sourceData;
            var sd2 = sourceData;
            Assert.IsTrue(sd1.Equals(sd2));
        }

        [TestMethod]
        public void NotEquals()
        {
            var sd1 = sourceData;
            id = "foobar";
            var sd2 = sourceData;
            Assert.IsFalse(sd1.Equals(sd2));
        }

        [TestMethod]
        public void NullNotEqualsTest()
        {
            Assert.IsFalse(sourceData.Equals(null));
        }

        [TestMethod]
        public void DifferentTypesNotEqualsTest()
        {
            var sd2 = new List<PayStatementSourceData>();
            Assert.IsFalse(sourceData.Equals(sd2));
        }

        [TestMethod]
        public void HashCode()
        {
            var sd1 = sourceData;
            var sd2 = sourceData;
            Assert.AreEqual(sd1.GetHashCode(), sd2.GetHashCode());
        }

        [TestMethod]
        public void NotHashCode()
        {
            var sd1 = sourceData;
            id = "foobar";
            var sd2 = sourceData;
            Assert.AreNotEqual(sd1.GetHashCode(), sd2.GetHashCode());
        }

    }
}
