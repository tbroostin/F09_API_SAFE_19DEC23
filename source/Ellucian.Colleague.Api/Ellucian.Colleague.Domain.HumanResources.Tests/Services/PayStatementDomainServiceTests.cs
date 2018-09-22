/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Services
{
    [TestClass]
    public class PayStatementDomainServiceTests
    {
        IPayStatementDomainService service;
        TestPayStatementRepository testData;
        IEnumerable<PayStatementSourceData> sourceData;
        IEnumerable<BenefitDeductionType> bendedTypes;
        

        public async Task Initialize()
        {
            testData = new TestPayStatementRepository();
            sourceData = await testData.GetPayStatementSourceDataByPersonIdAsync(testData.payStatementRecords.First().employeeId, null,null);
        }

        [TestClass]
        public class BuildPayStatementReportTests : PayStatementDomainServiceTests
        {
            //public BuildPayStatementReportTests() { }            

            //[TestInitialize]
            //public void _Initialize()
            //{
            //    Initialize();
            //    service = null;
            //    service = new PayStatementDomainService();
            //    //service.SetContext(sourceData);
            //    //service.SetBenefitDeductionContext(bendedTypes);
            //}

            //[TestMethod]
            //public void ReportIsBuilt()
            //{
            //    var report = service.BuildPayStatementReport(sourceData.First());
            //    Assert.IsNotNull(report);
            //    Assert.AreEqual(report.Id,sourceData.First().Id);
            //}

            //[TestMethod, ExpectedException(typeof(InvalidOperationException))]
            //public void CannotBuildReportWithoutSettingContext()
            //{
            //    service = new PayStatementDomainService();
            //    service.BuildPayStatementReport(sourceData.First());
            //}

            //[TestMethod, ExpectedException(typeof(ArgumentNullException))]
            //public void CannotSetNullContext()
            //{
            //    //service.SetContext(null);
            //}

            //[TestMethod, ExpectedException(typeof(ArgumentNullException))]
            //public void CannotGetReportWithoutSourceData()
            //{                
            //    service.BuildPayStatementReport(null);
            //}

            //[TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            //public void CannotGetReportWithInvalidEmployee()
            //{
            //    var data = new PayStatementSourceData(
            //        sourceData.First().Id,
            //        "probably an invalid employeeedid",
            //        sourceData.First().PaycheckReferenceId,
            //        sourceData.First().StatementReferenceId, 
            //        sourceData.First().PayDate,
            //        sourceData.First().PeriodGrossPay,
            //        sourceData.First().PeriodNetPay
            //    );
            //    service.BuildPayStatementReport(data);
            //}

            //[TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            //public void CannotGetReportWithInvalidYear()
            //{
            //    var data = new PayStatementSourceData(
            //        sourceData.First().Id,
            //        sourceData.First().EmployeeId,
            //        sourceData.First().PaycheckReferenceId,
            //        sourceData.First().StatementReferenceId,
            //        new DateTime(3099,12,12),
            //        sourceData.First().PeriodGrossPay,
            //        sourceData.First().PeriodNetPay
            //    );
            //    service.BuildPayStatementReport(data);
            //}
        }
    }
}
