using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Infrastructure.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;

namespace Ellucian.Colleague.Api.Client.Tests
{
    [TestClass]
    public class ColleagueApiClientColleagueFinanceTests
    {
        #region Initalize and Cleanup
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;

        private const string _serviceUrl = "http://service.url";
        private const string _contentType = "application/json";
        private const string _studentId = "123456";
        private const string _studentId2 = "678";
        private const string _token = "1234567890";
        private const string _courseId = "MATH-100";
        private const string _courseId2 = "ENGL-101";

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = MockLogger.Instance;
            _logger = _loggerMock.Object;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _logger = null;
        }
        #endregion
    }
}
