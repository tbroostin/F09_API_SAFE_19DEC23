// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Web.Http.Tests.Utilities.TestData;
using Ellucian.Web.Http.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Web.Http.Tests.Utilities
{
    [TestClass]
    public class DataPrivacyTests
    {
        private Mock<ILogger> LoggerMock { get; set; }
        private DataPrivacyTestData TestData { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            LoggerMock = new Mock<ILogger>();
            TestData = new DataPrivacyTestData();
        }

        [TestMethod]
        public void TestDataPrivacyWithEmptySettingList()
        {
            var returnedJson = DataPrivacy.ApplyDataPrivacy(TestData.SinglePersonJContainer, TestData.EmptySettingsList, LoggerMock.Object);
            Assert.IsNull(returnedJson);
        }

        [TestMethod]
        public void TestDataPrivacyOnSinglePerson()
        {
            var testCompare = TestData.GetGenderDateOfBirthColleagueCredentialsJContainer();

            var returnedJson = DataPrivacy.ApplyDataPrivacy(TestData.SinglePersonJContainer,
                TestData.GetGenderDateOfBirthColleagueCredentialsSettings(), LoggerMock.Object);
            
            Assert.IsTrue(JToken.DeepEquals(testCompare, returnedJson));
        }

        [TestMethod]
        public void TestDataPrivacyOnSinglePersonWithNonExistentPropertyFilters()
        {
            var returnedJson = DataPrivacy.ApplyDataPrivacy(TestData.SinglePersonJContainer,
                TestData.GetNonExistentProperties(), LoggerMock.Object);

            Assert.IsNull(returnedJson);
        }

        [TestMethod]
        public void TestDataPrivacyOnFivePerson()
        {
            var testCompare = TestData.GetHomeAddressAndLegalNameJContainer();

            var returnedJson = DataPrivacy.ApplyDataPrivacy(TestData.FivePersonJContainer,
                TestData.GetHomeAddressAndLegalNameSettings(), LoggerMock.Object);

            Assert.IsTrue(JToken.DeepEquals(testCompare, returnedJson));
        }

        [TestMethod]
        public void TestDataPrivacyOnFivePersonWithNonExistentPropertyFilters()
        {
            var returnedJson = DataPrivacy.ApplyDataPrivacy(TestData.FivePersonJContainer,
                TestData.GetNonExistentProperties(), LoggerMock.Object);

            Assert.IsNull(returnedJson);
        }
    }
}
