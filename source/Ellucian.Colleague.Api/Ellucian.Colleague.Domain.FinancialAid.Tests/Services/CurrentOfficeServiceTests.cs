//Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Services
{
    [TestClass]
    public class CurrentOfficeServiceTests
    {
        [TestClass]
        public class CurrentOfficeConstructorTests
        {
            private IEnumerable<FinancialAidOffice> inputOffices;
            private TestFinancialAidOfficeRepository officeRepository;

            [TestInitialize]
            public void Initialize()
            {
                officeRepository = new TestFinancialAidOfficeRepository();
                inputOffices = officeRepository.GetFinancialAidOffices();
            }

            [TestMethod]
            public void OfficeInputListIsValidTest()
            {
                new CurrentOfficeService(inputOffices);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullOfficesThrowsExceptionTest()
            {
                new CurrentOfficeService(null);
            }

            [TestMethod]
            public void EmptyOfficeInputListIsValidTest()
            {
                new CurrentOfficeService(new List<FinancialAidOffice>());
            }
        }

        [TestClass]
        public class CurrentOfficeByLocationTests
        {
            private IEnumerable<FinancialAidOffice> inputOffices;
            private TestFinancialAidOfficeRepository officeRepository;

            private CurrentOfficeService currentOfficeService;

            [TestInitialize]
            public void Initialize()
            {
                officeRepository = new TestFinancialAidOfficeRepository();
                inputOffices = officeRepository.GetFinancialAidOffices();

                currentOfficeService = new CurrentOfficeService(inputOffices);
            }

            [TestMethod]
            public void NullInputLocationIdReturnsDefaultOfficeTest()
            {
                var expectedDefaultOffice = inputOffices.First(o => o.IsDefault);
                var actualDefaultOffice = currentOfficeService.GetCurrentOfficeByLocationId(null);

                Assert.AreEqual(expectedDefaultOffice, actualDefaultOffice);
            }

            [TestMethod]
            public void ValidLocationIdReturnsOfficeByLocationTest()
            {
                var expectedOffice = inputOffices.First(o => o.LocationIds != null && o.LocationIds.Count() > 0);
                var actualOffice = currentOfficeService.GetCurrentOfficeByLocationId(expectedOffice.LocationIds.First());

                Assert.AreEqual(expectedOffice, actualOffice);
                CollectionAssert.AreEqual(expectedOffice.LocationIds, actualOffice.LocationIds);
            }

            [TestMethod]
            public void InvalidLocationIdReturnsDefaultOfficeTest()
            {
                var expectedOffice = inputOffices.First(o => o.IsDefault);
                var actualOffice = currentOfficeService.GetCurrentOfficeByLocationId("foobar");

                Assert.AreEqual(expectedOffice, actualOffice);
            }

            [TestMethod]
            public void InvalidLocationIdAndNoDefaultOfficeReturnsNullTest()
            {
                inputOffices.ToList().ForEach(o => o.IsDefault = false);
                FinancialAidOffice expectedOffice = inputOffices.FirstOrDefault(o => o.IsDefault);
                currentOfficeService = new CurrentOfficeService(inputOffices);
                var actualOffice = currentOfficeService.GetCurrentOfficeByLocationId("foobar");

                Assert.IsNull(expectedOffice);
                Assert.IsNull(actualOffice);
            }
        }

        [TestClass]
        public class GetDefaultOfficeTests
        {
            private IEnumerable<FinancialAidOffice> inputOffices;
            private TestFinancialAidOfficeRepository officeRepository;

            private CurrentOfficeService currentOfficeService;

            [TestInitialize]
            public void Initialize()
            {
                officeRepository = new TestFinancialAidOfficeRepository();
                inputOffices = officeRepository.GetFinancialAidOffices();

                currentOfficeService = new CurrentOfficeService(inputOffices);
            }

            [TestMethod]
            public void ReturnDefaultOfficeTest()
            {
                var expectedOffice = inputOffices.First(o => o.IsDefault);
                var actualOffice = currentOfficeService.GetDefaultOffice();

                Assert.AreEqual(expectedOffice, actualOffice);
            }

            [TestMethod]
            public void NoDefaultOfficeReturnsNullTest()
            {
                inputOffices.ToList().ForEach(o => o.IsDefault = false);
                currentOfficeService = new CurrentOfficeService(inputOffices);

                var expectedOffice = inputOffices.FirstOrDefault(o => o.IsDefault);
                var actualOffice = currentOfficeService.GetDefaultOffice();

                Assert.IsNull(expectedOffice);
                Assert.IsNull(actualOffice);
            }
        }

        [TestClass]
        public class GetCurrentOfficeByIdTests
        {
            private IEnumerable<FinancialAidOffice> inputOffices;
            private TestFinancialAidOfficeRepository officeRepository;

            private CurrentOfficeService currentOfficeService;

            [TestInitialize]
            public async void Initialize()
            {
                officeRepository = new TestFinancialAidOfficeRepository();
                inputOffices = await officeRepository.GetFinancialAidOfficesAsync();

                currentOfficeService = new CurrentOfficeService(inputOffices);
            }

            [TestMethod]
            public void GetCurrentOfficeById_ReturnsExpectedOfficeTest()
            {
                var expectedOffice = inputOffices.First();
                var actualOffice = currentOfficeService.GetCurrentOfficeByOfficeId(expectedOffice.Id);
                Assert.AreEqual(expectedOffice.Id, actualOffice.Id);
                Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                Assert.AreEqual(expectedOffice.OpeId, actualOffice.OpeId);
                Assert.AreEqual(expectedOffice.TitleIVCode, actualOffice.TitleIVCode);
            }

            [TestMethod]
            public void GetCurrentOfficeById_ReturnsDefaultIfIdNullTest()
            {
                var expectedOffice = inputOffices.FirstOrDefault(o => o.IsDefault);
                var actualOffice = currentOfficeService.GetCurrentOfficeByOfficeId(null);
                Assert.IsTrue(actualOffice.IsDefault);
                Assert.AreEqual(expectedOffice.Id, actualOffice.Id);
                Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                Assert.AreEqual(expectedOffice.OpeId, actualOffice.OpeId);
                Assert.AreEqual(expectedOffice.TitleIVCode, actualOffice.TitleIVCode);
            }

            [TestMethod]
            public void GetCurrentOfficeById_ReturnsDefaultIfNoMatchingOfficeTest()
            {
                var expectedOffice = inputOffices.FirstOrDefault(o => o.IsDefault);
                var actualOffice = currentOfficeService.GetCurrentOfficeByOfficeId("foo");
                Assert.IsTrue(actualOffice.IsDefault);
                Assert.AreEqual(expectedOffice.Id, actualOffice.Id);
                Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                Assert.AreEqual(expectedOffice.OpeId, actualOffice.OpeId);
                Assert.AreEqual(expectedOffice.TitleIVCode, actualOffice.TitleIVCode);
            }
        }

        [TestClass]
        public class GetActiveOfficeConfigurationsTests
        {
            private TestFinancialAidOfficeRepository officeRepository;

            private CurrentOfficeService currentOfficeService;

            [TestInitialize]
            public void Initialize()
            {
                officeRepository = new TestFinancialAidOfficeRepository();
            }

            [TestMethod]
            public void NullActiveConfigurations_EmptyListReturnedTest()
            {
                officeRepository.officeParameterRecordData.ForEach(p => p.IsSelfServiceActive = "");
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                Assert.IsFalse(currentOfficeService.GetActiveOfficeConfigurations().Any());
            }

            [TestMethod]
            public void AllActiveConfigurations_ExpectedNumberConfigurationsReturnedTest()
            {
                officeRepository.officeParameterRecordData.ForEach(p => p.IsSelfServiceActive = "Y");
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                var expected = officeRepository.officeParameterRecordData.Count;
                Assert.AreEqual(expected, currentOfficeService.GetActiveOfficeConfigurations().Count());
            }

            [TestMethod]
            public void OneActiveConfiguration_OneConfigurationReturnedTest()
            {
                officeRepository.officeParameterRecordData.ForEach(p => p.IsSelfServiceActive = "");
                officeRepository.officeParameterRecordData.First().IsSelfServiceActive = "Y";
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                var expected = 1;
                Assert.AreEqual(expected, currentOfficeService.GetActiveOfficeConfigurations().Count());
            }
        }

        [TestClass]
        public class GetCurrentOfficeWithActiveConfigurationTests
        {
            private TestFinancialAidOfficeRepository officeRepository;

            private CurrentOfficeService currentOfficeService;
            private string locationId, awardYear;

            [TestInitialize]
            public void Initialize()
            {
                officeRepository = new TestFinancialAidOfficeRepository();
                locationId = officeRepository.officeParameterRecordData.First().OfficeCode;
                awardYear = officeRepository.officeParameterRecordData.First().AwardYear;
            }

            [TestMethod]
            public void NoActiveConfigurations_GetCurrentOfficeWithActiveConfiguration_ReturnsNullTest()
            {
                officeRepository.officeParameterRecordData.ForEach(p => p.IsSelfServiceActive = "");
                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                Assert.IsNull(currentOfficeService.GetCurrentOfficeWithActiveConfiguration(locationId, awardYear));
            }

            [TestMethod]
            public void DefaultConfigurationNotActive_GetCurrentOfficeWithActiveConfiguration_ReturnsNullTest()
            {
                var defaultOfficeCode = officeRepository.defaultSystemParametersRecordData.MainOfficeId;
                var defaultOffice = officeRepository.officeParameterRecordData.First(p => p.OfficeCode == defaultOfficeCode);
                defaultOffice.IsSelfServiceActive = "";

                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                Assert.IsNull(currentOfficeService.GetCurrentOfficeWithActiveConfiguration("foo", defaultOffice.AwardYear));
            }

            [TestMethod]
            public void ActiveConfigurationExists_GetCurrentOfficeWithActiveConfiguration_ReturnsOfficeTest()
            {
                officeRepository.officeParameterRecordData.First().IsSelfServiceActive = "Y";

                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                Assert.IsNotNull(currentOfficeService.GetCurrentOfficeWithActiveConfiguration(locationId, awardYear));
            }

            [TestMethod]
            public void ActiveConfigurationExists_GetCurrentOfficeWithActiveConfiguration_ReturnsExpectedOfficeTest()
            {
                officeRepository.officeParameterRecordData.First().IsSelfServiceActive = "Y";
                var expectedOffice = officeRepository.officeParameterRecordData.First();

                currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());
                var actualOffice = currentOfficeService.GetCurrentOfficeWithActiveConfiguration(locationId, awardYear);
                Assert.AreEqual(expectedOffice.OfficeCode, actualOffice.Id);                
            }
        }
    }
}
