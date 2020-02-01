/*Copyright 2014-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class FinancialAidOfficeRepositoryTests
    {
        [TestClass]
        public class GetFinancialAidOfficeAsyncTests : BaseRepositorySetup
        {

            private TestFinancialAidOfficeRepository expectedRepository;
            private List<FinancialAidOffice> expectedOffices
            {
                get
                {
                    return (expectedRepository.GetFinancialAidOfficesAsync().Result).ToList();
                }
            }

            private FinancialAidOfficeRepository actualRepository;
            private List<FinancialAidOffice> actualOffices;
            

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();

                expectedRepository = new TestFinancialAidOfficeRepository();

                actualRepository = BuildRepositoryAsync();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
            }



            [TestMethod]
            public void NumOfficesAreEqualTest()
            {
                Assert.AreEqual(expectedOffices.Count(), actualOffices.Count());
            }

            [TestMethod]
            public void ObjectsAreEqualTest()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Equals(expectedOffice));
                    Assert.IsNotNull(actualOffice);
                }
            }

            [TestMethod]
            public async Task EmptyOfficeIdInOfficeRecordTest()
            {
                expectedRepository.officeRecordData.ForEach(o => o.Id = null);
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsFalse(actualOffices.Any());
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public async Task EmptyOfficeIdAndOpeIdTest()
            {
                expectedRepository.shoppingSheetParameterData.ForEach(o => o.OpeId = null);
                expectedRepository.officeParameterRecordData.ForEach(o => o.OfficeCode = null);
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsFalse(actualOffices.SelectMany(o => o.Configurations).Any());
            }

            [TestMethod]
            public async Task EmptyIdInAcademicProgressParameterTest()
            {
                expectedRepository.academicProgressParameterData.ForEach(c => c.officeId = null);
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsTrue(actualOffices.Select(o => o.AcademicProgressConfiguration).All(c => !c.IsSatisfactoryAcademicProgressActive));
            }

            [TestMethod]
            public void LocationIdsAreEqualTest()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var locationId = expectedOffice.LocationIds.First();
                    var actualOffice = actualOffices.FirstOrDefault(o => o.LocationIds.Contains(locationId));
                    Assert.IsNotNull(actualOffice);
                }
            }

            //Give an officeRecord an Id that doesn't exist as an OfficeId in the locationsResponseData records.
            [TestMethod]
            public async Task OfficeRecordWithNoLocationTest()
            {
                var noLocationOfficeId = "foobar";
                expectedRepository.officeRecordData.First().Id = noLocationOfficeId;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var noLocationOffice = actualOffices.First(o => o.Id == noLocationOfficeId);

                Assert.AreEqual(0, noLocationOffice.LocationIds.Count());
            }

            [TestMethod]
            public void DefaultOfficeTest()
            {
                var defaultOfficeId = expectedRepository.defaultSystemParametersRecordData.MainOfficeId;
                var defaultOffice = actualOffices.First(o => o.Id == defaultOfficeId);
                Assert.IsTrue(defaultOffice.IsDefault);
            }

            [TestMethod]
            public void AllOtherOfficesNotDefaultTest()
            {
                var defaultOfficeId = expectedRepository.defaultSystemParametersRecordData.MainOfficeId;
                var notDefaultOffices = actualOffices.Where(o => o.Id != defaultOfficeId);

                foreach (var notDefaultOffice in notDefaultOffices)
                {
                    Assert.IsFalse(notDefaultOffice.IsDefault);
                }
            }

            [TestMethod]
            public async Task NoOfficeRecordsReturnsEmptyListLogsMessageTest()
            {
                expectedRepository.officeRecordData = new List<TestFinancialAidOfficeRepository.OfficeRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.AreEqual(0, actualOffices.Count());
                loggerMock.Verify(l => l.Info("Office records not found in database"));
            }

            [TestMethod]
            public async Task EmptyOfficeRecordsReturnsEmptyListLogsMessageTest()
            {
                expectedRepository.officeRecordData = new List<TestFinancialAidOfficeRepository.OfficeRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.AreEqual(0, actualOffices.Count());
                loggerMock.Verify(l => l.Info("Office records not found in database"));
            }

            [TestMethod]
            public async Task NoOfficeParametersRecordsLogsMessageTest()
            {
                expectedRepository.officeParameterRecordData = new List<TestFinancialAidOfficeRepository.OfficeParameterRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration == null);

                Assert.AreEqual(0, configurations.Count());
                loggerMock.Verify(l => l.Info("FA Office parameter records not found in database"));
            }

            [TestMethod]
            public async Task EmptyOfficeParametersRecordsLogsMessageTest()
            {
                expectedRepository.officeParameterRecordData = new List<TestFinancialAidOfficeRepository.OfficeParameterRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration == null);

                Assert.AreEqual(0, configurations.Count());
                loggerMock.Verify(l => l.Info("FA Office parameter records not found in database"));
            }

            [TestMethod]
            public async Task NoShoppingSheetParametersRecordsLogsMessageTest()
            {
                expectedRepository.shoppingSheetParameterData = new List<TestFinancialAidOfficeRepository.ShoppingSheetParameterRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration != null);

                Assert.AreEqual(0, configurations.Count());
                loggerMock.Verify(l => l.Info("Shopping Sheet Parameter records not found in database"));
            }

            [TestMethod]
            public async Task EmptyShoppingSheetParametersRecordsLogsMessageTest()
            {
                expectedRepository.shoppingSheetParameterData = new List<TestFinancialAidOfficeRepository.ShoppingSheetParameterRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration != null);

                Assert.AreEqual(0, configurations.Count());
                loggerMock.Verify(l => l.Info("Shopping Sheet Parameter records not found in database"));
            }

            [TestMethod]
            public async Task NoParametersNoConfigurationsTest()
            {
                expectedRepository.shoppingSheetParameterData = new List<TestFinancialAidOfficeRepository.ShoppingSheetParameterRecord>();
                expectedRepository.officeParameterRecordData = new List<TestFinancialAidOfficeRepository.OfficeParameterRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations);
                Assert.AreEqual(0, configurations.Count());
            }

            [TestMethod]
            public void AddressTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();

                var expectedCsz = string.Format("{0}, {1} {2}", expectedOffice.City, expectedOffice.State, expectedOffice.Zip);

                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(expectedOffice.Address.Count() + 1, actualOffice.AddressLabel.Count());

                for (int i = 0; i < actualOffice.AddressLabel.Count(); i++)
                {
                    if (actualOffice.AddressLabel[i] == actualOffice.AddressLabel.Last())
                    {
                        Assert.AreEqual(expectedCsz, actualOffice.AddressLabel[i]);
                    }
                    else
                    {
                        Assert.AreEqual(expectedOffice.Address[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public async Task NullAddressInOfficeRecord_EmptyAddressInObjectTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.Address = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(0, actualOffice.AddressLabel.Count());
            }

            [TestMethod]
            public async Task EmptyAddressInOfficeRecord_EmptyAddressInObjectTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.Address = new List<string>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(0, actualOffice.AddressLabel.Count());
            }

            [TestMethod]
            public async Task AddressWithEmptyLinesInOfficeRecord_EmptyAddressInObjectTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.Address = new List<string>() { string.Empty, string.Empty };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(0, actualOffice.AddressLabel.Count());
            }

            [TestMethod]
            public async Task EmptyCityInOfficeRecord_EmptyAddressInObjectTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.City = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(0, actualOffice.AddressLabel.Count());
            }

            [TestMethod]
            public async Task EmptyStateInOfficeRecord_EmptyAddressInObjectTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.State = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(0, actualOffice.AddressLabel.Count());
            }

            [TestMethod]
            public async Task EmptyZipInOfficeRecord_EmptyAddressInObjectTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.Zip = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(0, actualOffice.AddressLabel.Count());
            }


            [TestMethod]
            public async Task NullLocationRecordsReturnsOfficesWithNoLocationId()
            {
                expectedRepository.locationRecordData = new List<TestFinancialAidOfficeRepository.LocationRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsTrue(actualOffices.All(o => o.LocationIds.Count() == 0));
            }

            [TestMethod]
            public async Task EmptyLocationRecordsReturnsOfficesWithNoLocationId()
            {
                expectedRepository.locationRecordData = new List<TestFinancialAidOfficeRepository.LocationRecord>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsTrue(actualOffices.All(o => o.LocationIds.Count() == 0));
            }

            [TestMethod]
            public async Task NoFaSysParamsReturnsOfficesWithNoDefault()
            {
                expectedRepository.defaultSystemParametersRecordData = new TestFinancialAidOfficeRepository.DefaultSystemParametersRecord();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsNull(actualOffices.FirstOrDefault(o => o.IsDefault));                
            }

            [TestMethod]
            public async Task EmptyMainOfficeReturnsOfficesWithNoDefault()
            {
                expectedRepository.defaultSystemParametersRecordData.MainOfficeId = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsNull(actualOffices.FirstOrDefault(o => o.IsDefault));
            }

            [TestMethod]
            public async Task OpeIdOnOfficeRecordTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.OpeId = "foobar";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(expectedOffice.OpeId, actualOffice.OpeId);
            }

            [TestMethod]
            public async Task EmptyOpeIdOnOfficeRecordUsesDefaultOpeIdTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.OpeId = string.Empty;
                expectedRepository.defaultSystemParametersRecordData.OpeId = "foobar";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(o => o.Id == expectedOffice.Id);

                Assert.AreEqual(expectedRepository.defaultSystemParametersRecordData.OpeId, actualOffice.OpeId);
            }

            /// <summary>
            /// Tests if the office TitleIVCode is set to the correct value if the
            /// corresponding officeRecord property is not null
            /// </summary>
            [TestMethod]
            public async Task TitleIVCode_EqualsOfficeRecordTitleIVCodeTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.FirstOrDefault(rd => !string.IsNullOrEmpty(rd.TitleIVCode));
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(ao => ao.Id == expectedOffice.Id);
                Assert.AreEqual(expectedOffice.TitleIVCode, actualOffice.TitleIVCode);
            }

            /// <summary>
            /// Tests if the TitleIVCode equals the defaultSysParam TitleIVCode
            /// when the corresponding officeRecord property is null
            /// </summary>
            [TestMethod]
            public async Task TitleIVCode_EqualsDefaultSysParamTitleIVCode()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                expectedOffice.TitleIVCode = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualOffice = actualOffices.First(ao => ao.Id == expectedOffice.Id);

                Assert.AreEqual(expectedRepository.defaultSystemParametersRecordData.TitleIVCode, actualOffice.TitleIVCode);
            }

            [TestMethod]
            public async Task NumConfigurationsTest()
            {
                expectedRepository.officeParameterRecordData.Add(new TestFinancialAidOfficeRepository.OfficeParameterRecord()
                {
                    AwardYear = "1492",
                    OfficeCode = expectedRepository.officeParameterRecordData.First().OfficeCode
                });
                expectedRepository.shoppingSheetParameterData.Add(new TestFinancialAidOfficeRepository.ShoppingSheetParameterRecord()
                {
                    AwardYear = "2376",
                    OpeId = (!string.IsNullOrEmpty(expectedRepository.officeRecordData.First().OpeId)) ? expectedRepository.officeRecordData.First().OpeId : expectedRepository.defaultSystemParametersRecordData.OpeId
                });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var actualOffice in actualOffices)
                {
                    //officeparam award years.concat(shopsheet param award years).distinct.count

                    var numExpectedConfigs = expectedRepository.officeParameterRecordData.Where(op => op.OfficeCode == actualOffice.Id).Select(op => op.AwardYear)
                        .Concat(expectedRepository.shoppingSheetParameterData.Where(p => p.OpeId == actualOffice.OpeId).Select(p => p.AwardYear))
                        .Distinct()
                        .Count();

                    Assert.AreEqual(numExpectedConfigs, actualOffice.Configurations.Count());
                }
            }

            [TestMethod]
            public async Task NoOfficeParameterRecord_ShoppingSheetParameterExists_ConfigurationExistsTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(p => { p.AwardYearDescription = "foo"; p.IsSelfServiceActive = "y"; });

                expectedRepository.officeRecordData.ForEach(o => o.OpeId = "foobar");
                expectedRepository.shoppingSheetParameterData.Add(new TestFinancialAidOfficeRepository.ShoppingSheetParameterRecord()
                {
                    AwardYear = "2376",
                    OpeId = "foobar",
                    OfficeType = "5",
                    GraduationRate = 100
                });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == "foobar").GetConfiguration("2376");
                Assert.IsNotNull(actualConfiguration);
                Assert.IsNotNull(actualConfiguration.ShoppingSheetConfiguration);

                //Assert that a few configuration attributes that come from the officeParameterRecord are not what were set above
                Assert.IsTrue(string.IsNullOrEmpty(actualConfiguration.AwardYearDescription));
                Assert.AreEqual(null, actualConfiguration.UndergraduatePackage);
                Assert.AreEqual(false, actualConfiguration.IsSelfServiceActive);


            }

            [TestMethod]
            public async Task NoShoppingSheetParameterRecord_OfficeParameterExists_ConfigurationExistsTest()
            {
                expectedRepository.shoppingSheetParameterData.ForEach(p => { p.GraduationRate = 100; p.NationalLoanDefaultRate = 50; });

                expectedRepository.officeRecordData.ForEach(o => o.Id = "foobar");
                expectedRepository.officeParameterRecordData.Add(new TestFinancialAidOfficeRepository.OfficeParameterRecord()
                {
                    AwardYear = "1492",
                    OfficeCode = "foobar",
                    AwardYearDescription = "foobarDescription",
                    AverageUndergradGrantAmount = 500,
                    AverageUndergradLoanAmount = 600,
                    AverageGradScholarshipAmount = 700,
                    IsSelfServiceActive = "y"
                });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == "foobar").GetConfiguration("1492");
                Assert.IsNotNull(actualConfiguration);
                Assert.IsNull(actualConfiguration.ShoppingSheetConfiguration);

                //Assert that a the configuration attributes that come from the officeParameterRecord are set
                Assert.AreEqual("foobarDescription", actualConfiguration.AwardYearDescription);
                Assert.IsNotNull(actualConfiguration.UndergraduatePackage);
                Assert.IsTrue(actualConfiguration.IsSelfServiceActive);
            }

            [TestMethod]
            public async Task Configuration_AwardYearDescriptionTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardYearDescription = "foobar";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.AwardYearDescription, actualConfiguration.AwardYearDescription);
            }

            [TestMethod]
            public async Task Configuration_UndergraduatePackageTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AverageUndergradGrantAmount = 101;
                officeParametersRecord.AverageUndergradLoanAmount = 102;
                officeParametersRecord.AverageUndergradScholarshipAmount = 103;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();

                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsNotNull(actualConfiguration.UndergraduatePackage);
                Assert.AreEqual(officeParametersRecord.AverageUndergradGrantAmount, actualConfiguration.UndergraduatePackage.AverageGrantAmount);
                Assert.AreEqual(officeParametersRecord.AverageUndergradLoanAmount, actualConfiguration.UndergraduatePackage.AverageLoanAmount);
                Assert.AreEqual(officeParametersRecord.AverageUndergradScholarshipAmount, actualConfiguration.UndergraduatePackage.AverageScholarshipAmount);
            }

            [TestMethod]
            public async Task Configuration_UndergraduatePackageThrowsExceptionLogsErrorTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AverageUndergradGrantAmount = -1;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsNull(actualConfiguration.UndergraduatePackage);

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), "Error creating undergraduate average award package for {0} award year", officeParametersRecord.AwardYear));
            }

            [TestMethod]
            public async Task Configuration_GraduatePackageTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AverageGradGrantAmount = 789;
                officeParametersRecord.AverageGradLoanAmount = 987;
                officeParametersRecord.AverageGradScholarshipAmount = 567;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();

                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsNotNull(actualConfiguration.GraduatePackage);
                Assert.AreEqual(officeParametersRecord.AverageGradGrantAmount, actualConfiguration.GraduatePackage.AverageGrantAmount);
                Assert.AreEqual(officeParametersRecord.AverageGradLoanAmount, actualConfiguration.GraduatePackage.AverageLoanAmount);
                Assert.AreEqual(officeParametersRecord.AverageGradScholarshipAmount, actualConfiguration.GraduatePackage.AverageScholarshipAmount);
            }

            [TestMethod]
            public async Task Configuration_GraduatePackageThrowsExceptionLogsErrorTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AverageGradGrantAmount = -1;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsNull(actualConfiguration.GraduatePackage);

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), "Error creating graduate average award package for {0} award year", officeParametersRecord.AwardYear));
            }

            [TestMethod]
            public async Task Configuration_IsSelfServiceActiveTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IsSelfServiceActive = "y";

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.IsSelfServiceActive);

                officeParametersRecord.IsSelfServiceActive = "";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.IsSelfServiceActive);
            }

            [TestMethod]
            public async Task Configuration_IsAwardingActiveTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IsAwardingActive = "y";

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.IsAwardingActive);

                officeParametersRecord.IsAwardingActive = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.IsAwardingActive);
            }

            [TestMethod]
            public async Task Configuration_AreAwardChangesAllowedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.CanMakeAwardChanges = "y";

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.AreAwardChangesAllowed);

                officeParametersRecord.CanMakeAwardChanges = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AreAwardChangesAllowed);
            }

            [TestMethod]
            public async Task Configuration_AllowAnnualAwardUpdatesOnlyTest()
            {
                //set to "y"
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AnnualAccRejOnly = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode)
                    .Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.AllowAnnualAwardUpdatesOnly);

                //set to "n"
                officeParametersRecord.AnnualAccRejOnly = "n";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode)
                    .Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AllowAnnualAwardUpdatesOnly);

                //set to null
                officeParametersRecord.AnnualAccRejOnly = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode)
                    .Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AllowAnnualAwardUpdatesOnly);

                //empty string
                officeParametersRecord.AnnualAccRejOnly = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode)
                    .Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AllowAnnualAwardUpdatesOnly);

                //random char
                officeParametersRecord.AnnualAccRejOnly = "c";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode)
                    .Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AllowAnnualAwardUpdatesOnly);
            }

            [TestMethod]
            public async Task Configuration_IsProfileActiveTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IsProfileActive = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.IsProfileActive);

                officeParametersRecord.IsProfileActive = "n";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.IsProfileActive);
            }

            [TestMethod]
            public async Task Configuration_AreLoanRequestsActiveTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AreLoanRequestsAllowed = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == officeParametersRecord.OfficeCode && c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.AreLoanRequestsAllowed);

                officeParametersRecord.AreLoanRequestsAllowed = "N";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == officeParametersRecord.OfficeCode && c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AreLoanRequestsAllowed);
            }

            [TestMethod]
            public async Task Configuration_IsShoppingSheetActiveTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IsShoppingSheetActive = "Y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == officeParametersRecord.OfficeCode && c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.IsShoppingSheetActive);

                officeParametersRecord.IsShoppingSheetActive = "n";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == officeParametersRecord.OfficeCode && c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.IsShoppingSheetActive);
            }

            [TestMethod]
            public async Task Configuration_IsAwardHistoryActiveTest()
            {
                var officeParameterRecord = expectedRepository.officeParameterRecordData.First();
                officeParameterRecord.IsAwardLetterHistoryActive = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == officeParameterRecord.OfficeCode && c.AwardYear == officeParameterRecord.AwardYear);
                Assert.IsTrue(actualConfiguration.IsAwardLetterHistoryActive);

                officeParameterRecord.IsAwardLetterHistoryActive = "n";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == officeParameterRecord.OfficeCode && c.AwardYear == officeParameterRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.IsAwardLetterHistoryActive);
            }

            [TestMethod]
            public async Task Configuration_NullExcludeActionCategoryFromViewListTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardStatusCategoriesToExcludeFromView = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsNotNull(actualConfiguration.ExcludeAwardStatusCategoriesView);
                Assert.AreEqual(0, actualConfiguration.ExcludeAwardStatusCategoriesView.Count());
            }

            [TestMethod]
            public async Task Configuration_AllAwardStatusCategoriesAddedToViewListTest()
            {
                var parameterRecord = expectedRepository.officeParameterRecordData.First();
                parameterRecord.AwardStatusCategoriesToExcludeFromView.AddRange(new string[5] { "A", "E", "P", "R", "D" });

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == parameterRecord.OfficeCode).Configurations.First(c => c.AwardYear == parameterRecord.AwardYear);

                Assert.AreEqual(5, actualConfiguration.ExcludeAwardStatusCategoriesView.Count());
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesView, AwardStatusCategory.Accepted);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesView, AwardStatusCategory.Pending);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesView, AwardStatusCategory.Estimated);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesView, AwardStatusCategory.Rejected);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesView, AwardStatusCategory.Denied);
            }

            [TestMethod]
            public async Task Configuration_BadCodeNotAddedToAwardStatusCategoryViewListTest()
            {
                var parameterRecord = expectedRepository.officeParameterRecordData.First();
                parameterRecord.AwardStatusCategoriesToExcludeFromView.AddRange(new string[2] { "D", "FOOBAR" });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == parameterRecord.OfficeCode).Configurations.First(c => c.AwardYear == parameterRecord.AwardYear);

                Assert.AreEqual(1, actualConfiguration.ExcludeAwardStatusCategoriesView.Count());
            }

            [TestMethod]
            public async Task Configuration_NullExcludeAwardCategoriesFromViewTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardCategoriesToExcludeFromView = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.AreEqual(0, actualConfiguration.ExcludeAwardCategoriesView.Count());
            }

            [TestMethod]
            public async Task Configuration_ExcludeAwardCategoriesFromViewTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardCategoriesToExcludeFromView = new List<string>() { "PELL", "TEACH", "USTF", "FWS" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                CollectionAssert.AreEqual(officeParametersRecord.AwardCategoriesToExcludeFromView, actualConfiguration.ExcludeAwardCategoriesView);
            }

            [TestMethod]
            public async Task Configuration_NullExcludeAwardPeriodsFromViewTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardPeriodsToExcludeFromView = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(0, actualConfiguration.ExcludeAwardPeriods.Count());
            }

            [TestMethod]
            public async Task Configuration_ExcludeAwardPeriodsFromViewTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardPeriodsToExcludeFromView = new List<string>() { "14/FA", "15/WI", "16/SP" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                CollectionAssert.AreEqual(officeParametersRecord.AwardPeriodsToExcludeFromView, actualConfiguration.ExcludeAwardPeriods);
            }

            [TestMethod]
            public async Task Configuration_NullExcludeAwardsFromViewTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardsToExcludeFromView = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(0, actualConfiguration.ExcludeAwardsView.Count());
            }

            [TestMethod]
            public async Task Configuration_ExcludeAwardsFromViewTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardsToExcludeFromView = new List<string>() { "PELL", "UGTCH", "WOOFY" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                CollectionAssert.AreEqual(officeParametersRecord.AwardsToExcludeFromView, actualConfiguration.ExcludeAwardsView);

            }

            [TestMethod]
            public async Task Configuration_NullExcludeActionCategoryFromChangeListTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardStatusCategoriesToPreventChanges = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(0, actualConfiguration.ExcludeAwardStatusCategoriesFromChange.Count());
            }

            [TestMethod]
            public async Task Configuration_AllAwardStatusCategoriesAddedToChangeListTest()
            {
                var parameterRecord = expectedRepository.officeParameterRecordData.First();
                parameterRecord.AwardStatusCategoriesToPreventChanges.AddRange(new string[5] { "A", "E", "P", "R", "D" });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == parameterRecord.OfficeCode).Configurations.First(c => c.AwardYear == parameterRecord.AwardYear);

                Assert.AreEqual(5, actualConfiguration.ExcludeAwardStatusCategoriesFromChange.Count());
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesFromChange, AwardStatusCategory.Accepted);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesFromChange, AwardStatusCategory.Pending);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesFromChange, AwardStatusCategory.Estimated);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesFromChange, AwardStatusCategory.Rejected);
                CollectionAssert.Contains(actualConfiguration.ExcludeAwardStatusCategoriesFromChange, AwardStatusCategory.Denied);

            }

            [TestMethod]
            public async Task Configuration_BadCodeNotAddedToAwardStatusCategoryChangeListTest()
            {
                var parameterRecord = expectedRepository.officeParameterRecordData.First();
                parameterRecord.AwardStatusCategoriesToPreventChanges.AddRange(new string[2] { "D", "FOOBAR" });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == parameterRecord.OfficeCode).Configurations.First(c => c.AwardYear == parameterRecord.AwardYear);

                Assert.AreEqual(1, actualConfiguration.ExcludeAwardStatusCategoriesFromChange.Count());
            }

            [TestMethod]
            public async Task Configuration_NullExcludeAwardCategoriesFromChangeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardCategoriesToPreventChanges = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(0, actualConfiguration.ExcludeAwardCategoriesFromChange.Count());
            }

            [TestMethod]
            public async Task Configuration_ExcludeAwardCategoriesFromChangeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardCategoriesToPreventChanges = new List<string>() { "PELL", "MATT", "CAT" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                CollectionAssert.AreEqual(officeParametersRecord.AwardCategoriesToPreventChanges, actualConfiguration.ExcludeAwardCategoriesFromChange);
            }

            [TestMethod]
            public async Task Configuration_NullExcludeAwardsFromChangeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardsToPreventChanges = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(0, actualConfiguration.ExcludeAwardsFromChange.Count);
            }

            [TestMethod]
            public async Task Configuration_ExcludeAwardsFromChangeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardsToPreventChanges = new List<string>() { "PELL", "UGTCH", "GRTCH", "SUB" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                CollectionAssert.AreEqual(officeParametersRecord.AwardsToPreventChanges, actualConfiguration.ExcludeAwardsFromChange);
            }

            [TestMethod]
            public async Task Configuration_NullExcludeAwardStatusesFromChangeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ActionStatusesToPreventChanges = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(0, actualConfiguration.ExcludeAwardStatusesFromChange.Count());
            }

            [TestMethod]
            public async Task Configuration_ExcludeAwardStatusesFromChangeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ActionStatusesToPreventChanges = new List<string>() { "Z", "Y", "X" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                CollectionAssert.AreEqual(officeParametersRecord.ActionStatusesToPreventChanges, actualConfiguration.ExcludeAwardStatusesFromChange);
            }

            [TestMethod]
            public async Task Configuration_NoChecklistItemCodesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemCodes = new List<string>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemCodes.Any());
            }

            [TestMethod]
            public async Task Configuration_NullChecklistItemCodesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemCodes = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemCodes.Any());
            }

            [TestMethod]
            public async Task Configuration_ChecklistItemsContainExpectedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemCodes = new List<string>() { "Awards", "Letter", "MPN"};
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.ChecklistItemCodes, actualConfiguration.ChecklistItemCodes);
            }

            [TestMethod]
            public async Task Configuration_NoChecklistItemControlStatusesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemControlStatuses = new List<string>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemControlStatuses.Any());
            }

            [TestMethod]
            public async Task Configuration_NullChecklistItemControlStatusesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemControlStatuses = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemControlStatuses.Any());
            }

            [TestMethod]
            public async Task Configuration_ChecklistItemControlStatusesContainExpectedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemControlStatuses = new List<string>() { "A", "L", "M", "*", "9O" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.ChecklistItemControlStatuses, actualConfiguration.ChecklistItemControlStatuses);
            }

            [TestMethod]
            public async Task Configuration_NoIgnoreActionStatusesFromEvalTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IgnoreAwardStatusesFromEval = new List<string>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.IgnoreAwardStatusesFromEval.Any());
            }

            [TestMethod]
            public async Task Configuration_NullIgnoreActionStatusesFromEvalTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IgnoreAwardStatusesFromEval = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.IgnoreAwardStatusesFromEval.Any());
            }

            [TestMethod]
            public void Configuration_IgnoreActionStatusesFromEvalContainExpectedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();               
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.IgnoreAwardStatusesFromEval, actualConfiguration.IgnoreAwardStatusesFromEval);
            }

            [TestMethod]
            public async Task Configuration_AcceptedAwardActionTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AcceptedAwardAction = "F";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.AcceptedAwardAction, actualConfiguration.AcceptedAwardStatusCode);
            }

            [TestMethod]
            public async Task Configuration_AcceptedAwardCommunicationCodeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AcceptedAwardCommunicationCode = "FOOBAR";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.AcceptedAwardCommunicationCode, actualConfiguration.AcceptedAwardCommunicationCode);
            }

            [TestMethod]
            public async Task Configuration_AcceptedAwardCommunicationStatusTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AcceptedAwardCommunicationStatus = "T";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.AcceptedAwardCommunicationStatus, actualConfiguration.AcceptedAwardCommunicationStatus);
            }

            [TestMethod]
            public async Task Configuration_RejectedAwardActionTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.RejectedAwardAction = "$";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.RejectedAwardAction, actualConfiguration.RejectedAwardStatusCode);

            }

            [TestMethod]
            public async Task Configuration_RejectedAwardCommunicationCodeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.RejectedAwardCommunicationCode = "RABOOF";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.RejectedAwardCommunicationCode, actualConfiguration.RejectedAwardCommunicationCode);
            }

            [TestMethod]
            public async Task Configuration_RejectedAwardCommunicationStatusTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.RejectedAwardCommunicationStatus = "V";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.RejectedAwardCommunicationStatus, actualConfiguration.RejectedAwardCommunicationStatus);
            }

            [TestMethod]
            public async Task Configuration_AllowNegativeUnmetNeedBorrowingTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AllowNegativeUnmetNeedBorrowing = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.AllowNegativeUnmetNeedBorrowing);

                officeParametersRecord.AllowNegativeUnmetNeedBorrowing = "n";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AllowNegativeUnmetNeedBorrowing);
            }

            [TestMethod]
            public async Task Configuration_AllowLoanChangesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AllowLoanChanges = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.AllowLoanChanges);

                officeParametersRecord.AllowLoanChanges = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AllowLoanChanges);
            }

            [TestMethod]
            public async Task Configuration_AllowLoanChangeIfAcceptedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AllowLoanChangeIfAccepted = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.AllowLoanChangeIfAccepted);

                officeParametersRecord.AllowLoanChangeIfAccepted = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.AllowLoanChangeIfAccepted);
            }

            /// <summary>
            /// Test if SuppressInstanceData property is set correctly
            /// </summary>
            [TestMethod]
            public async Task Configuration_SuppressInstanceDataSetTrueTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.SuppressInstanceData = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.SuppressInstanceData);

                officeParametersRecord.SuppressInstanceData = "n";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.SuppressInstanceData);

            }

            [TestMethod]
            public async Task Configuration_NewLoanCommunicationCodeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.NewLoanCommunicationCode = "FOOBAR";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.NewLoanCommunicationCode, actualConfiguration.NewLoanCommunicationCode);
            }

            [TestMethod]
            public async Task Configuration_NewLoanCommunicationStatusTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.NewLoanCommunicationStatus = "R";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.NewLoanCommunicationStatus, actualConfiguration.NewLoanCommunicationStatus);
            }

            [TestMethod]
            public async Task Configuration_LoanChangeCommunicationCodeTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.LoanChangeCommunicationCode = "FOOBAR";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.LoanChangeCommunicationCode, actualConfiguration.LoanChangeCommunicationCode);
            }

            [TestMethod]
            public async Task Configuration_LoanChangeCommunicationStatusTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.LoanChangeCommunicationStatus = "H";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.LoanChangeCommunicationStatus, actualConfiguration.LoanChangeCommunicationStatus);
            }

            [TestMethod]
            public async Task Configuration_PaperCopyOptionTextTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.PaperCopyOptionText = "This is the paper copy opTION Text";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.AreEqual(officeParametersRecord.PaperCopyOptionText, actualConfiguration.PaperCopyOptionText);
            }

            [TestMethod]
            public async Task Configuration_ReviewLoanChangesTrueTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.ReviewLoanChanges = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.AreEqual(expectedConfiguration.IsLoanAmountChangeRequestRequired, actualConfiguration.IsLoanAmountChangeRequestRequired);
                    Assert.IsTrue(actualConfiguration.IsLoanAmountChangeRequestRequired);
                }
            }

            [TestMethod]
            public async Task Configuration_ReviewLoanChangesFalseTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.ReviewLoanChanges = "n");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.AreEqual(expectedConfiguration.IsLoanAmountChangeRequestRequired, actualConfiguration.IsLoanAmountChangeRequestRequired);
                    Assert.IsFalse(actualConfiguration.IsLoanAmountChangeRequestRequired);
                }
            }

            [TestMethod]
            public async Task Configuration_ReviewDeclinedAwardsTrueTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.ReviewDeclinedAwards = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.AreEqual(expectedConfiguration.IsDeclinedStatusChangeRequestRequired, actualConfiguration.IsDeclinedStatusChangeRequestRequired);
                    Assert.IsTrue(actualConfiguration.IsDeclinedStatusChangeRequestRequired);
                }
            }

            [TestMethod]
            public async Task Configuration_ReviewDeclinedAwardsFalseTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.ReviewDeclinedAwards = null);
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.AreEqual(expectedConfiguration.IsDeclinedStatusChangeRequestRequired, actualConfiguration.IsDeclinedStatusChangeRequestRequired);
                    Assert.IsFalse(actualConfiguration.IsDeclinedStatusChangeRequestRequired);
                }
            }

            [TestMethod]
            public async Task Configuration_CounselorPhoneTypeTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.CounselorPhoneType = "FOOBAR");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.AreEqual(expectedConfiguration.CounselorPhoneType, actualConfiguration.CounselorPhoneType);
                }
            }

            [TestMethod]
            public async Task Configuration_SuppressMaximumLoanLimitsTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressMaximumLoanLimits = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsTrue(actualConfiguration.SuppressMaximumLoanLimits);
                }
            }

            [TestMethod]
            public async Task Configuration_NotSuppressMaximumLoanLimitsTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressMaximumLoanLimits = "");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressMaximumLoanLimits);
                }
            }

            [TestMethod]
            public async Task Configuration_SuppressAverageAwardPackageDisplayTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAverageAwardPackageDisplay = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsTrue(actualConfiguration.SuppressAverageAwardPackageDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_NotSuppressAverageAwardPackageDisplayTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAverageAwardPackageDisplay = "");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressAverageAwardPackageDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_NotSuppressAverageAwardPackageDisplayBadValueTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAverageAwardPackageDisplay = "k");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressAverageAwardPackageDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_SuppressAccountSummaryDisplayTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAccountSummaryDisplay = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsTrue(actualConfiguration.SuppressAccountSummaryDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_NotSuppressAccountSummaryDisplayTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAccountSummaryDisplay = "");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressAccountSummaryDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_NotSuppressAccountSummaryDisplayBadValueTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAccountSummaryDisplay = "k");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressAccountSummaryDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_SuppressDisbursementInfoDisplayTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressDisbursementInfoDisplay = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsTrue(actualConfiguration.SuppressDisbursementInfoDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_NotSuppressDisbursementInfoDisplayTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressDisbursementInfoDisplay = "");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressDisbursementInfoDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_NotSuppressDisbursementInfoDisplayTestBadValueTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressDisbursementInfoDisplay = "k");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressDisbursementInfoDisplay);
                }
            }

            [TestMethod]
            public async Task Configuration_UseDocumentStatusDescriptionTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.UseDocumentStatusDescription = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsTrue(actualConfiguration.UseDocumentStatusDescription);
                }
            }

            [TestMethod]
            public async Task Configuration_NotUseDocumentStatusDescriptionTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.UseDocumentStatusDescription = "D");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.UseDocumentStatusDescription);
                }
            }

            [TestMethod]
            public async Task Configuration_NullUseDocumentStatusDescriptionTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.UseDocumentStatusDescription = null);
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.UseDocumentStatusDescription);
                }
            }

            [TestMethod]
            public async Task Configuration_DisplayPellLEUTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.DisplayPellLifetimeEarningsUsed = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsTrue(actualConfiguration.DisplayPellLifetimeEarningsUsed);
                }
            }

            [TestMethod]
            public async Task Configuration_NullDisplayPellLEUTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.DisplayPellLifetimeEarningsUsed = null);
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.DisplayPellLifetimeEarningsUsed);
                }
            }

            [TestMethod]
            public async Task Configuration_BadDateDisplayPellLEUTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.DisplayPellLifetimeEarningsUsed = "P");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.DisplayPellLifetimeEarningsUsed);
                }
            }

            [TestMethod]
            public async Task Configuration_SuppressAwardLetterAcceptance_ReturnsFalseTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAwardLetterAcceptance = "");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressAwardLetterAcceptance);
                }
            }

            [TestMethod]
            public async Task Configuration_SuppressAwardLetterAcceptance_ReturnsFalseTest2()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAwardLetterAcceptance = "o");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsFalse(actualConfiguration.SuppressAwardLetterAcceptance);
                }
            }

            [TestMethod]
            public async Task Configuration_SuppressAwardLetterAcceptance_ReturnsTrueTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(op => op.SuppressAwardLetterAcceptance = "y");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach (var expectedConfiguration in expectedOffices.SelectMany(o => o.Configurations))
                {
                    var actualConfiguration = actualOffices.SelectMany(o => o.Configurations).First(c => c.OfficeId == expectedConfiguration.OfficeId && c.AwardYear == expectedConfiguration.AwardYear);
                    Assert.IsTrue(actualConfiguration.SuppressAwardLetterAcceptance);
                }
            }


            [TestMethod]
            public async Task ShoppingSheetConfiguration_CustomMessageRulesTableIdTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.CustomMessageRuleTableId = "FOOBAR";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.CustomMessageRuleTableId, actualConfiguration.ShoppingSheetConfiguration.CustomMessageRuleTableId);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_GraduationRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.GraduationRate = 55.5m;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.GraduationRate, actualConfiguration.ShoppingSheetConfiguration.GraduationRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullGraduationRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.GraduationRate = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.GraduationRate, actualConfiguration.ShoppingSheetConfiguration.GraduationRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_LowToMediumBoundaryTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.LowToMediumBoundary = 12.6m;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.LowToMediumBoundary, actualConfiguration.ShoppingSheetConfiguration.LowToMediumBoundary);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullLowToMediumBoundaryTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.LowToMediumBoundary = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.LowToMediumBoundary, actualConfiguration.ShoppingSheetConfiguration.LowToMediumBoundary);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_MediumToHighBoundaryTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.MediumToHighBoundary = 66.6m;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.MediumToHighBoundary, actualConfiguration.ShoppingSheetConfiguration.MediumToHighBoundary);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullMediumToHighBoundaryTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.MediumToHighBoundary = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.MediumToHighBoundary, actualConfiguration.ShoppingSheetConfiguration.MediumToHighBoundary);
            }


            [TestMethod]
            public async Task ShoppingSheetConfiguration_LoanDefaultRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.LoanDefaultRate = 8.7m;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.LoanDefaultRate, actualConfiguration.ShoppingSheetConfiguration.LoanDefaultRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullLoanDefaultRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.LoanDefaultRate = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.LoanDefaultRate, actualConfiguration.ShoppingSheetConfiguration.LoanDefaultRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NationalLoanDefaultRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.NationalLoanDefaultRate = 12.5m;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.NationalLoanDefaultRate, actualConfiguration.ShoppingSheetConfiguration.NationalLoanDefaultRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullNationalLoanDefaultRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.NationalLoanDefaultRate = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.NationalLoanDefaultRate, actualConfiguration.ShoppingSheetConfiguration.NationalLoanDefaultRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_MedianBorrowingAmountTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.MedianBorrowingAmount = 97847;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.MedianBorrowingAmount, actualConfiguration.ShoppingSheetConfiguration.MedianBorrowingAmount);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullMedianBorrowingAmountTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.MedianBorrowingAmount = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.MedianBorrowingAmount, actualConfiguration.ShoppingSheetConfiguration.MedianBorrowingAmount);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_MedianMonthlyPaymentAmountTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.MedianMonthlyPaymentAmount = 1233;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.MedianMonthlyPaymentAmount, actualConfiguration.ShoppingSheetConfiguration.MedianMonthlyPaymentAmount);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullMedianMonthlyPaymentAmountTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.MedianMonthlyPaymentAmount = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.MedianMonthlyPaymentAmount, actualConfiguration.ShoppingSheetConfiguration.MedianMonthlyPaymentAmount);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_InstitutionRepaymentRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.InstitutionRepaymentRate, actualConfiguration.ShoppingSheetConfiguration.InstitutionRepaymentRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullInstitutionRepaymentRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.InstitutionRepaymentRate = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.InstitutionRepaymentRate, actualConfiguration.ShoppingSheetConfiguration.InstitutionRepaymentRate);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NationalRepaymentRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.NationalRepaymentRateAverage, actualConfiguration.ShoppingSheetConfiguration.NationalRepaymentRateAverage);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NullNationalRepaymentRateTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.NationalRepaymentRateAverage = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(shoppingSheetParametersRecord.NationalRepaymentRateAverage, actualConfiguration.ShoppingSheetConfiguration.NationalRepaymentRateAverage);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_EmptyOfficeTypeIsDefaultInConfigurationTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.OfficeType = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(ShoppingSheetOfficeType.BachelorDegreeGranting, actualConfiguration.ShoppingSheetConfiguration.OfficeType);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_UnknownOfficeTypeIsDefaultInConfigurationTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.OfficeType = "foobar";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(ShoppingSheetOfficeType.BachelorDegreeGranting, actualConfiguration.ShoppingSheetConfiguration.OfficeType);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_BachelorsOfficeTypeTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.OfficeType = "1";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(ShoppingSheetOfficeType.BachelorDegreeGranting, actualConfiguration.ShoppingSheetConfiguration.OfficeType);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_AssociatesOfficeTypeTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.OfficeType = "2";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(ShoppingSheetOfficeType.AssociateDegreeGranting, actualConfiguration.ShoppingSheetConfiguration.OfficeType);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_CertificateOfficeTypeTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.OfficeType = "3";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(ShoppingSheetOfficeType.CertificateGranting, actualConfiguration.ShoppingSheetConfiguration.OfficeType);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_GraduateOfficeTypeTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.OfficeType = "4";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(ShoppingSheetOfficeType.GraduateDegreeGranting, actualConfiguration.ShoppingSheetConfiguration.OfficeType);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_NonDegreeOfficeTypeTest()
            {
                var shoppingSheetParametersRecord = expectedRepository.shoppingSheetParameterData.First();
                shoppingSheetParametersRecord.OfficeType = "5";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.OpeId == shoppingSheetParametersRecord.OpeId).Configurations.First(c => c.AwardYear == shoppingSheetParametersRecord.AwardYear);

                Assert.AreEqual(ShoppingSheetOfficeType.NonDegreeGranting, actualConfiguration.ShoppingSheetConfiguration.OfficeType);
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_ProfileNotActiveDefaultEfcOptionTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(office => office.IsProfileActive = null);
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration != null);

                Assert.IsTrue(configurations.All(c => c.ShoppingSheetConfiguration.EfcOption == ShoppingSheetEfcOption.IsirEfc));
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_IsirEfc_EfcOptionTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(office => office.IsProfileActive = "y");
                expectedRepository.shoppingSheetParameterData.ForEach(param => param.UseProfileImEfc = "n");
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration != null);

                Assert.IsTrue(configurations.All(c => c.ShoppingSheetConfiguration.EfcOption == ShoppingSheetEfcOption.IsirEfc));
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_ProfileEfc_EfcOptionTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(office => office.IsProfileActive = "y");
                expectedRepository.shoppingSheetParameterData.ForEach(param => { param.UseProfileImEfc = "y"; param.UseProfileImUntilIsirIsFederal = ""; });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration != null);

                Assert.IsTrue(configurations.All(c => c.ShoppingSheetConfiguration.EfcOption == ShoppingSheetEfcOption.ProfileEfc));
            }

            [TestMethod]
            public async Task ShoppingSheetConfiguration_ProfileEfcUntilIsirExists_EfcOptionTest()
            {
                expectedRepository.officeParameterRecordData.ForEach(office => office.IsProfileActive = "y");
                expectedRepository.shoppingSheetParameterData.ForEach(param => { param.UseProfileImEfc = "y"; param.UseProfileImUntilIsirIsFederal = "y"; });
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var configurations = actualOffices.SelectMany(o => o.Configurations).Where(c => c.ShoppingSheetConfiguration != null);

                Assert.IsTrue(configurations.All(c => c.ShoppingSheetConfiguration.EfcOption == ShoppingSheetEfcOption.ProfileEfcUntilIsirExists));
            }

            [TestMethod]
            public async Task AcademicProgressConfiguration_InitializedTest()
            {
                expectedRepository.academicProgressParameterData = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsTrue(actualOffices.All(o => o.AcademicProgressConfiguration != null));
                loggerMock.Verify(l => l.Info("FA Office SAP Parameter records not found in database"));

            }

            [TestMethod]
            public async Task AcademicProgressConfiguration_OfficeIdTest()
            {
                expectedRepository.academicProgressParameterData = new List<TestFinancialAidOfficeRepository.AcademicProgressParameters>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsTrue(actualOffices.All(o => o.AcademicProgressConfiguration.OfficeId == o.Id));
                loggerMock.Verify(l => l.Info("FA Office SAP Parameter records not found in database"));
            }


            [TestMethod]
            public async Task AcademicProgressConfiguration_DetailPropertyConfigurationsInitializedTest()
            {
                expectedRepository.academicProgressParameterData = new List<TestFinancialAidOfficeRepository.AcademicProgressParameters>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsTrue(actualOffices.All(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations != null));
            }

            [TestMethod]
            public void NumberSAPHistoryItemsNull_DefaultResturnedTest()
            {
                var sapConfig = expectedRepository.academicProgressParameterData.First(pd => pd.numberOfAcademicProgressHistoryRecordsToDisplay == null);
                var actualConfiguration = actualOffices.First(o => o.Id == sapConfig.officeId);
                Assert.IsTrue(actualConfiguration.AcademicProgressConfiguration.NumberOfAcademicProgressHistoryRecordsToDisplay == 5);
            }

            [TestMethod]
            public async Task AcademicProgressConfiguration_IsAcademicProgressAvailableSetToFalseTest()
            {
                expectedRepository.academicProgressParameterData = new List<TestFinancialAidOfficeRepository.AcademicProgressParameters>();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                Assert.IsTrue(actualOffices.All(o => !o.AcademicProgressConfiguration.IsSatisfactoryAcademicProgressActive));
            }

            [TestMethod]
            public void AcademicProgressConfiguration_IsAcademicProgressAvailableTest()
            {
                foreach (var office in actualOffices)
                {
                    var expectedConfig = expectedOffices.First(o => o.Id == office.Id).AcademicProgressConfiguration;
                    Assert.AreEqual(expectedConfig.IsSatisfactoryAcademicProgressActive, office.AcademicProgressConfiguration.IsSatisfactoryAcademicProgressActive);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_EvaluationPeriodAttemptedCreditsDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.EvaluationPeriodAttemptedCredits);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_EvaluationPeriodCompletedCreditsDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.EvaluationPeriodCompletedCredits);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_EvaluationPeriodCompletedGpaDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.EvaluationPeriodOverallGpa);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_EvaluationPeriodRateOfCompletionDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.EvaluationPeriodRateOfCompletion);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_CumulativeAttemptedCreditsDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.CumulativeAttemptedCredits);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_CumulativeCompletedCreditsDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.CumulativeCompletedCredits);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_CumulativeCompletedGpaDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.CumulativeOverallGpa);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_CumulativeRateOfCompletionDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.CumulativeRateOfCompletion);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_CumulativeAttemptedCreditsExcludingRemedialDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.CumulativeAttemptedCreditsExcludingRemedial);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_CumulativeCompletedCreditsExcludingRemedialDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.CumulativeCompletedCreditsExcludingRemedial);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_CumulativeRateOfCompletionExcludingRemedialDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.CumulativeRateOfCompletionExcludingRemedial);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public void AcademicProgressConfiguration_MaximumProgramCreditsDetailsTest()
            {
                foreach (var office in actualOffices)
                {
                    var propertyConfig = office.AcademicProgressConfiguration.DetailPropertyConfigurations
                        .First(c => c.Type == AcademicProgressPropertyType.MaximumProgramCredits);

                    var expectedPropertyConfig = expectedOffices.Where(o => o.Id == office.Id).SelectMany(o => o.AcademicProgressConfiguration.DetailPropertyConfigurations).First(c => c.Type == propertyConfig.Type);
                    Assert.AreEqual(expectedPropertyConfig.Description, propertyConfig.Description);
                    Assert.AreEqual(expectedPropertyConfig.Label, propertyConfig.Label);
                    Assert.AreEqual(expectedPropertyConfig.IsHidden, propertyConfig.IsHidden);
                }
            }

            [TestMethod]
            public async Task Configuration_CreateChecklistItemsForNewStudentTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.CreateChecklistItemsForNewStudent = "Y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.CreateChecklistItemsForNewStudent);

                officeParametersRecord.CreateChecklistItemsForNewStudent = "";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.CreateChecklistItemsForNewStudent);


            }

            [TestMethod]
            public async Task Configuration_UseDefaultContactTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.UseDefaultContact = "y";
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsTrue(actualConfiguration.UseDefaultContact);

                officeParametersRecord.UseDefaultContact = string.Empty;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);

                Assert.IsFalse(actualConfiguration.UseDefaultContact);
            }

            [TestMethod]
            public async Task NullIncomingChecklistItemCodes_ChecklistItemCodesListEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemCodes = null;

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemCodes.Any());
            }

            [TestMethod]
            public async Task NoIncomingChecklistItemCodes_ChecklistItemCodesListEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemCodes = new List<string>();

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemCodes.Any());
            }

            [TestMethod]
            public async Task ExpectedNumberChecklistItemCodesSetTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemCodes = new List<string>() { "A", "B", "C", "D", "E", "F", "G"};
                int expectedCount = officeParametersRecord.ChecklistItemCodes.Count;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.AreEqual(expectedCount, actualConfiguration.ChecklistItemCodes.Count);
            }

            [TestMethod]
            public async Task NullIncomingChecklistItemControlStatuses_ChecklistItemControlStatusesListEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemControlStatuses = null;

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemControlStatuses.Any());
            }

            [TestMethod]
            public async Task NoIncomingChecklistItemControlStatuses_ChecklistItemControlStatusesListEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemControlStatuses = new List<string>();

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemControlStatuses.Any());
            }

            [TestMethod]
            public async Task ExpectedNumberChecklistItemControlStatusesSetTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemControlStatuses = new List<string>() { "A", "B", "C", "D", "E"};
                int expectedCount = officeParametersRecord.ChecklistItemControlStatuses.Count;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.AreEqual(expectedCount, actualConfiguration.ChecklistItemControlStatuses.Count);
            }

            [TestMethod]
            public async Task NullIncomingChecklistItemDefaultFlags_ChecklistItemDefaultFlagsListEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemDefaultFlags = null;

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemDefaultFlags.Any());
            }

            [TestMethod]
            public async Task NoIncomingChecklistItemDefaultFlags_ChecklistItemDefaultFlagsListEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemDefaultFlags = new List<string>();

                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ChecklistItemDefaultFlags.Any());
            }

            [TestMethod]
            public async Task ExpectedNumberChecklistItemDefaultFlagsSetTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ChecklistItemDefaultFlags = new List<string>() { "A", "B", "C", "D", "E", "K", "(", "&*" };
                int expectedCount = officeParametersRecord.ChecklistItemDefaultFlags.Count;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.AreEqual(expectedCount, actualConfiguration.ChecklistItemDefaultFlags.Count);
            }

            [TestMethod]
            public async Task ExcludeAwardsFromAwardLetterAndShoppingSheet_ContainsExpectedValuesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardsToExcludeFromAwardLetter = new List<string>() { "Woofy", "Goofy" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.AwardsToExcludeFromAwardLetter, actualConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public async Task ExcludeAwardsFromAwardLetterAndShoppingSheet_IsEmptyListTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardsToExcludeFromAwardLetter = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ExcludeAwardsFromAwardLetterAndShoppingSheet.Any());
            }

            [TestMethod]
            public async Task ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet_ContainsExpectedValuesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardPeriodsToExcludeFromAwardLetter = new List<string>() { "15/FA", "16/SP", "16/SU" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.AwardPeriodsToExcludeFromAwardLetter, actualConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public async Task ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet_IsEmptyListTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardPeriodsToExcludeFromAwardLetter = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ExcludeAwardPeriodsFromAwardLetterAndShoppingSheet.Any());
            }

            [TestMethod]
            public async Task ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet_ContainsExpectedValuesTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardCategoriesToExcludeFromAwardLetter = new List<string>() { "15/FA", "16/SP", "16/SU" };
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.AwardCategoriesToExcludeFromAwardLetter, actualConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet);
            }

            [TestMethod]
            public async Task ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet_IsEmptyListTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.AwardCategoriesToExcludeFromAwardLetter = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ExcludeAwardCategoriesFromAwardLetterAndShoppingSheet.Any());
            }

            [TestMethod]
            public void DefaultDisplayYearCode_EqualsOfficeRecordDefaultDisplayYearCodeTest()
            {
                var expectedOffice = expectedRepository.officeRecordData.First();
                var actualOffice = actualOffices.First(ao => ao.Id == expectedOffice.Id);
                Assert.AreEqual(expectedOffice.DefaultDisplayYearCode, actualOffice.DefaultDisplayYearCode);
            }

            [TestMethod]
            public async Task IgnoreAwardsOnChecklist_ListIsEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IgnoreAwardsOnChecklist = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.IgnoreAwardsOnChecklist.Any());
            }

            [TestMethod]
            public async Task IgnoreAwardsOnChecklist_ExpectedValuesReturnedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.IgnoreAwardsOnChecklist, actualConfiguration.IgnoreAwardsOnChecklist);
            }

            [TestMethod]
            public async Task IgnoreAwardStatusesOnChecklist_ListIsEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IgnoreActionStatusesOnChecklist = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.IgnoreAwardStatusesOnChecklist.Any());
            }

            [TestMethod]
            public async Task IgnoreAwardStatusesOnChecklist_ExpectedValuesReturnedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.IgnoreActionStatusesOnChecklist, actualConfiguration.IgnoreAwardStatusesOnChecklist);
            }

            [TestMethod]
            public async Task IgnoreAwardCategoriesOnChecklist_ListIsEmptyTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.IgnoreAwardCategoriesOnChecklist = null;
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.IgnoreAwardCategoriesOnChecklist.Any());
            }

            [TestMethod]
            public async Task IgnoreAwardCategoriesOnChecklist_ExpectedValuesReturnedTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                CollectionAssert.AreEqual(officeParametersRecord.IgnoreAwardCategoriesOnChecklist, actualConfiguration.IgnoreAwardCategoriesOnChecklist);
            }

            [TestMethod]
            public async Task ShowBudgetDetailsOnAwardLetter_ReturnsFalseTest()
            {
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First().Configurations.First();
                Assert.IsFalse(actualConfiguration.ShowBudgetDetailsOnAwardLetter);
            }

            [TestMethod]
            public async Task EmptyContractValue_ShowBudgetDetailsOnAwardLetter_ReturnsFalseTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ShowBudgetDetailsOnAwardLetter = "";
                BuildRepositoryAsync();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsFalse(actualConfiguration.ShowBudgetDetailsOnAwardLetter);
            }

            [TestMethod]
            public async Task ShowBudgetDetailsOnAwardLetter_ReturnsTrueTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.ShowBudgetDetailsOnAwardLetter = "Y";
                BuildRepositoryAsync();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsTrue(actualConfiguration.ShowBudgetDetailsOnAwardLetter);
            }

            [TestMethod]
            public async Task StudentAwardLetterBudgetDetailsDescription_ReturnsExpectedValueTest()
            {
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                foreach(var record in expectedRepository.officeParameterRecordData)
                {
                    var actualConfiguration = actualOffices.First(o => o.Id == record.OfficeCode).Configurations.First(c => c.AwardYear == record.AwardYear);
                    Assert.AreEqual(record.StudentAwardLetterBudgetDetailsDescription, actualConfiguration.StudentAwardLetterBudgetDetailsDescription);
                }
                
            }

            [TestMethod]
            public async Task NullStudentAwardLetterBudgetDetailsDescription_ReturnsExpectedValueTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.StudentAwardLetterBudgetDetailsDescription = null;
                BuildRepositoryAsync();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.IsNull(actualConfiguration.StudentAwardLetterBudgetDetailsDescription);
            }

            [TestMethod]
            public async Task EmptyStudentAwardLetterBudgetDetailsDescription_ReturnsExpectedValueTest()
            {
                var officeParametersRecord = expectedRepository.officeParameterRecordData.First();
                officeParametersRecord.StudentAwardLetterBudgetDetailsDescription = string.Empty;
                BuildRepositoryAsync();
                actualOffices = (await actualRepository.GetFinancialAidOfficesAsync()).ToList();
                var actualConfiguration = actualOffices.First(o => o.Id == officeParametersRecord.OfficeCode).Configurations.First(c => c.AwardYear == officeParametersRecord.AwardYear);
                Assert.AreEqual(string.Empty, actualConfiguration.StudentAwardLetterBudgetDetailsDescription);
            }

            private FinancialAidOfficeRepository BuildRepositoryAsync()
            {
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FaLocations>("", true))
                    .Returns<string, bool>((s, b) =>
                    {
                        if (expectedRepository.locationRecordData == null) return null;
                        return Task.FromResult(new Collection<FaLocations>(expectedRepository.locationRecordData.Select(location =>
                            new FaLocations()
                            {
                                Recordkey = location.Id,
                                FalocFaOffice = location.OfficeId
                            }).ToList()));
                    });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FaOffices>("", true))
                    .Returns<string, bool>((s, b) =>
                    {
                        if (expectedRepository.officeRecordData == null) return null;
                        return Task.FromResult(new Collection<FaOffices>(expectedRepository.officeRecordData.Select(office =>
                            new FaOffices()
                            {
                                Recordkey = office.Id,
                                FaofcAddress = office.Address,
                                FaofcCity = office.City,
                                FaofcName = office.Name,
                                FaofcPellInternetAddress = office.Email,
                                FaofcPellPhoneNumber = office.PhoneNumber,
                                FaofcPellFaDirector = office.DirectorName,
                                FaofcState = office.State,
                                FaofcZip = office.Zip,
                                FaofcOpeId = office.OpeId,
                                FaofcTitleIvCode = office.TitleIVCode,
                                FaofcSsStartDisplayYear = office.DefaultDisplayYearCode
                            }).ToList()));
                    });

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true))
                    .Returns<string, string, bool>((s1, s2, b) =>
                    {
                        if (expectedRepository.defaultSystemParametersRecordData == null) return null;
                        return Task.FromResult(new FaSysParams()
                        {
                            Recordkey = "FA.SYS.PARAMS",
                            FspMainFaOffice = expectedRepository.defaultSystemParametersRecordData.MainOfficeId,
                            FspOpeId = expectedRepository.defaultSystemParametersRecordData.OpeId,
                            FspTitleIvCode = expectedRepository.defaultSystemParametersRecordData.TitleIVCode
                        });
                    });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FaOfficeParameters>("", true))
                    .Returns<string, bool>((s, b) =>
                    {
                        if (expectedRepository.officeParameterRecordData == null) return null;
                        return Task.FromResult(new Collection<FaOfficeParameters>(expectedRepository.officeParameterRecordData.Select(officeParameter =>
                            new FaOfficeParameters()
                            {
                                FopFaOfficeCode = officeParameter.OfficeCode,
                                FopAwardChangesAvail = officeParameter.CanMakeAwardChanges,
                                FopAnnualAccrejOnly = officeParameter.AnnualAccRejOnly,
                                FopAwardingAvail = officeParameter.IsAwardingActive,
                                FopAwardLetterAvail = officeParameter.IsAwardLetterActive,
                                FopLoanRequestsAvail = officeParameter.AreLoanRequestsAllowed,
                                FopShoppingSheetAvail = officeParameter.IsShoppingSheetActive,
                                FopExclActCatFromChg = officeParameter.AwardStatusCategoriesToPreventChanges,
                                FopExclActCatFromView = officeParameter.AwardStatusCategoriesToExcludeFromView,
                                FopExclAwardsFromChg = officeParameter.AwardsToPreventChanges,
                                FopExclAwardsFromView = officeParameter.AwardsToExcludeFromView,
                                FopExclAwdCatFromChg = officeParameter.AwardCategoriesToPreventChanges,
                                FopExclActStFromChg = officeParameter.ActionStatusesToPreventChanges,
                                FopExclAwdCatFromView = officeParameter.AwardCategoriesToExcludeFromView,
                                FopExclAwdPdsFromView = officeParameter.AwardPeriodsToExcludeFromView,
                                FopExclActCatFromAwdltr = officeParameter.AwardStatusCategoriesToExcludeFromAwardLetter,
                                FopExclAwardsFromAwdltr = officeParameter.AwardsToExcludeFromAwardLetter,
                                FopExclAwdCatFromAwdltr = officeParameter.AwardCategoriesToExcludeFromAwardLetter,
                                FopExclAwdPdsFromAwdltr = officeParameter.AwardPeriodsToExcludeFromAwardLetter,
                                FopGrAvgGrantAmt = officeParameter.AverageGradGrantAmount,
                                FopGrAvgLoanAmt = officeParameter.AverageGradLoanAmount,
                                FopGrAvgScholarshipAmt = officeParameter.AverageGradScholarshipAmount,
                                FopProfileAvail = officeParameter.IsProfileActive,
                                FopSelfServiceAvail = officeParameter.IsSelfServiceActive,
                                FopUgAvgGrantAmt = officeParameter.AverageUndergradGrantAmount,
                                FopUgAvgLoanAmt = officeParameter.AverageUndergradLoanAmount,
                                FopUgAvgScholarshipAmt = officeParameter.AverageUndergradScholarshipAmount,
                                FopYear = officeParameter.AwardYear,
                                FopYearDescription = officeParameter.AwardYearDescription,
                                FopAccAwdsActSt = officeParameter.AcceptedAwardAction,
                                FopAccAwdsCcCode = officeParameter.AcceptedAwardCommunicationCode,
                                FopAccAwdsCcSt = officeParameter.AcceptedAwardCommunicationStatus,
                                FopRejAwdsActSt = officeParameter.RejectedAwardAction,
                                FopRejAwdsCcCode = officeParameter.RejectedAwardCommunicationCode,
                                FopRejAwdsCcSt = officeParameter.RejectedAwardCommunicationStatus,
                                FopNegUnmetNeed = officeParameter.AllowNegativeUnmetNeedBorrowing,
                                FopLoanAmtChanges = officeParameter.AllowLoanChanges,
                                FopChangeAcceptedLoans = officeParameter.AllowLoanChangeIfAccepted,
                                FopDeclineZeroAccLoans = officeParameter.AllowDeclineZeroOutIfAccepted,
                                FopNewLoanCcCode = officeParameter.NewLoanCommunicationCode,
                                FopNewLoanCcStatus = officeParameter.NewLoanCommunicationStatus,
                                FopChgLoanCcCode = officeParameter.LoanChangeCommunicationCode,
                                FopChgLoanCcStatus = officeParameter.LoanChangeCommunicationStatus,
                                FopSuppressInstanceData = officeParameter.SuppressInstanceData,
                                FopPaperCopyOptionDesc = (officeParameter.PaperCopyOptionText),
                                FopReviewDeclinedAwards = (officeParameter.ReviewDeclinedAwards),
                                FopReviewLoanChanges = (officeParameter.ReviewLoanChanges),
                                FopCounselorPhoneType = (officeParameter.CounselorPhoneType),
                                FopAwdLtrHistAvail = officeParameter.IsAwardLetterHistoryActive,
                                FopChecklistNoFinAid = officeParameter.CreateChecklistItemsForNewStudent,
                                FopUseDefaultContact = officeParameter.UseDefaultContact,
                                FopSupressLoanLimit = officeParameter.SuppressMaximumLoanLimits,
                                FopUseMailingCodeDesc = officeParameter.UseDocumentStatusDescription,
                                FopChecklistItems = officeParameter.ChecklistItemCodes,
                                FopChecklistDisplayAction = officeParameter.ChecklistItemControlStatuses,
                                FopChecklistAssignByDflt = officeParameter.ChecklistItemDefaultFlags,
                                FopIgnoreActStFromEval = officeParameter.IgnoreAwardStatusesFromEval,
                                FopDisplayPellLeu = officeParameter.DisplayPellLifetimeEarningsUsed,
                                FopSuppressAvgPkgDisplay = officeParameter.SuppressAverageAwardPackageDisplay,
                                FopSuppressActSumDisplay = officeParameter.SuppressAccountSummaryDisplay,
                                FopSuppressAwdLtrAccept = officeParameter.SuppressAwardLetterAcceptance,
                                FopSuppressDisbInfoDispl = officeParameter.SuppressDisbursementInfoDisplay,
                                FopIgnoreActStOnChklst = officeParameter.IgnoreActionStatusesOnChecklist,
                                FopIgnoreAwardsOnChklst = officeParameter.IgnoreAwardsOnChecklist,
                                FopIgnoreAwdCatOnChklst = officeParameter.IgnoreAwardCategoriesOnChecklist,
                                FopShowBudgetDetails = officeParameter.ShowBudgetDetailsOnAwardLetter,
                                FopBudgetDtlDesc = officeParameter.StudentAwardLetterBudgetDetailsDescription
                            }).ToList()));
                    });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FaShopsheetParams>("", true))
                    .Returns<string, bool>((s, b) =>
                    {
                        if (expectedRepository.shoppingSheetParameterData == null) return null;
                        return Task.FromResult(new Collection<FaShopsheetParams>(expectedRepository.shoppingSheetParameterData.Select(shoppingSheetParameter =>
                            new FaShopsheetParams()
                            {
                                FsspFaYear = shoppingSheetParameter.AwardYear,
                                FsspOpeId = shoppingSheetParameter.OpeId,
                                FsspCustomMessageRtId = shoppingSheetParameter.CustomMessageRuleTableId,
                                FsspInstitutionType = shoppingSheetParameter.OfficeType,
                                FsspGraduationRate = shoppingSheetParameter.GraduationRate,
                                FsspInstLoanDefaultRate = shoppingSheetParameter.LoanDefaultRate,
                                FsspUsLoanDefaultRate = shoppingSheetParameter.NationalLoanDefaultRate,
                                FsspMedianBorrowAmount = shoppingSheetParameter.MedianBorrowingAmount,
                                FsspMedianPayment = shoppingSheetParameter.MedianMonthlyPaymentAmount,
                                FsspEfcOption = shoppingSheetParameter.UseProfileImEfc,
                                FsspEfcOptionExt = shoppingSheetParameter.UseProfileImUntilIsirIsFederal,
                                FsspGradRateLowToMed = (shoppingSheetParameter.LowToMediumBoundary),
                                FsspGradRateMedToHigh = (shoppingSheetParameter.MediumToHighBoundary),
                                FsspNatRepaymentRateAvg = shoppingSheetParameter.NationalRepaymentRateAverage,
                                FsspInstRepaymentRate = shoppingSheetParameter.InstitutionRepaymentRate
                            }).ToList()));
                    });

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FaOfficeSapParameters>("", true))
                    .Returns<string, bool>((s, b) =>
                    {
                        return Task.FromResult(expectedRepository.academicProgressParameterData == null ? null :
                            new Collection<FaOfficeSapParameters>(expectedRepository.academicProgressParameterData.Select(record =>
                                new FaOfficeSapParameters()
                                {
                                    FospFaOfficeCode = record.officeId,
                                    FospSapAvail = record.isAcademicProgressActive,
                                    FospNumSapHistToDisp = record.numberOfAcademicProgressHistoryRecordsToDisplay,
                                    FospMaxCredOpt = record.maxCreditOption,
                                    FospMaxCredLabel = record.maxCreditLabel,
                                    FospMaxCredExpl = record.maxCreditDescription,
                                    FospCmplWoRemOpt = record.cumulativeCompletedCreditsExcludingRemedialOption,
                                    FospCmplWoRemLabel = record.cumulativeCompletedCreditsExcludingRemedialLabel,
                                    FospCmplWoRemExpl = record.cumulativeCompletedCreditsExcludingRemedialDescription,
                                    FospAttWoRemOpt = record.cumulativeAttemptedCreditsExcludingRemedialOption,
                                    FospAttWoRemLabel = record.cumulativeAttemptedCreditsExcludingRemedialLabel,
                                    FospAttWoRemExpl = record.cumulativeAttemptedCreditsExcludingRemedialDescription,
                                    FospAttEvalPdExpl = record.evaluationPeriodAttemptedCreditsDescription,
                                    FospAttEvalPdLabel = record.evaluationPeriodAttemptedCreditsLabel,
                                    FospAttEvalPdOpt = record.evaluationPeriodAttemptedCreditsOption,
                                    FospAttWithRemExpl = record.cumulativeAttemptedCreditsDescription,
                                    FospAttWithRemLabel = record.cumulativeAttemptedCreditsLabel,
                                    FospAttWithRemOpt = record.cumulativeAttemptedCreditsOption,
                                    FospCmplEvalPdExpl = record.evaluationPeriodCompletedCreditsDescription,
                                    FospCmplEvalPdLabel = record.evaluationPeriodCompletedCreditsLabel,
                                    FospCmplEvalPdOpt = record.evaluationPeriodCompletedCreditsOption,
                                    FospCmplWithRemExpl = record.cumulativeCompletedCreditsDescription,
                                    FospCmplWithRemLabel = record.cumulativeCompletedCreditsLabel,
                                    FospCmplWithRemOpt = record.cumulativeCompletedCreditsOption,
                                    FospGpaEvalPdExpl = record.evaluationPeriodCompletedGpaDescription,
                                    FospGpaEvalPdLabel = record.evaluationPeriodCompletedGpaLabel,
                                    FospGpaEvalPdOpt = record.evaluationPeriodCompletedGpaOption,
                                    FospGpaWithRemExpl = record.cumulativeCompletedGpaDescription,
                                    FospGpaWithRemLabel = record.cumulativeCompletedGpaLabel,
                                    FospGpaWithRemOpt = record.cumulativeCompletedGpaOption,
                                    FospPaceEvalPdExpl = record.evaluationPeriodPaceDescription,
                                    FospPaceEvalPdLabel = record.evaluationPeriodPaceLabel,
                                    FospPaceEvalPdOpt = record.evaluationPeriodPaceOption,
                                    FospPaceWithRemExpl = record.cumulativePaceDescription,
                                    FospPaceWithRemLabel = record.cumulativePaceLabel,
                                    FospPaceWithRemOpt = record.cumulativePaceOption,
                                    FospPaceWoRemExpl = record.cumulativePaceExcludingRemedialDescription,
                                    FospPaceWoRemLabel = record.cumulativePaceExcludingRemedialLabel,
                                    FospPaceWoRemOpt = record.cumulativePaceExcludingRemedialOption,
                                }).ToList()));
                    });

                loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

                return new FinancialAidOfficeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }
    }
}
