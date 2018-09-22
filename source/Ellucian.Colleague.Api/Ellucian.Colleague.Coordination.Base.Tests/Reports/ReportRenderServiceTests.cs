using Ellucian.Colleague.Coordination.Base.Reports;
using Microsoft.Reporting.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Reports
{
    [TestClass]
    public class ReportRenderServiceTests
    {

        public Mock<ILogger> loggerMock;

        public LocalReportService serviceUnderTest;

        public void ReportRenderServiceTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();

            serviceUnderTest = new LocalReportService(loggerMock.Object);
        }

        [TestClass]
        public class RenderReportTests : ReportRenderServiceTests
        {
            public LocalReport inputReport;

            [TestInitialize]
            public void Initialize()
            {
                ReportRenderServiceTestsInitialize();
                inputReport = new LocalReport();

                inputReport.ReportPath = "../../../Ellucian.Colleague.Coordination.HumanResources.Tests/Reports/TestReport.rdlc";
            }

            //[TestMethod]
            //public void RenderTest()
            //{
            //    var renderedBytes = serviceUnderTest.RenderReport(inputReport);
            //    Assert.IsInstanceOfType(renderedBytes, typeof(byte[]));
            //}
        }
    }
}
