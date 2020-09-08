//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.  

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class CompoundConfigurationSettingsRepositoryTests
    {
        /// <summary>
        /// Test class for CompoundConfigurationSettings codes
        /// </summary>
        [TestClass]
        public class CompoundConfigurationSettingsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CompoundConfigurationSettings> _compoundConfigurationSettingsCollection;
            IEnumerable<CompoundConfigurationSettingsOptions> _compoundConfigurationSettingsOptionsCollection;
            string codeItemName;

            CompoundConfigurationSettingsRepository compoundConfigurationSettingsRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _compoundConfigurationSettingsCollection = new List<Domain.Base.Entities.CompoundConfigurationSettings>()
                {
                    new Domain.Base.Entities.CompoundConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1",
                    "Payroll Deduction Occurrence Intervals")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "payPeriodOccurrence.interval"
                            }
                        },
                        Properties = new List<Domain.Base.Entities.CompoundConfigurationSettingsProperty>()
                        {
                            new Domain.Base.Entities.CompoundConfigurationSettingsProperty()
                            {
                                PrimaryValue = "10"
                            }
                        },
                        PrimaryLabel = "Payroll Interval",
                        SecondaryLabel = "Period Code"
                    },

                    new Domain.Base.Entities.CompoundConfigurationSettings("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2",
                    "Payroll Deduction Monthly Pay Periods")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "payPeriodOccurrence.monthlyPayPeriods"
                            }
                        },
                        Properties = new List<Domain.Base.Entities.CompoundConfigurationSettingsProperty>()
                        {
                            new Domain.Base.Entities.CompoundConfigurationSettingsProperty()
                            {
                               PrimaryValue = "10"
                            }
                        },
                        PrimaryLabel = "Monthly Interval",
                        SecondaryLabel = "Period Code"
                    },

                    new Domain.Base.Entities.CompoundConfigurationSettings("9a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "3",
                    "Default Student Programs")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "section-registrations"
                            }
                        },
                        Properties = new List<Domain.Base.Entities.CompoundConfigurationSettingsProperty>()
                        {
                            new Domain.Base.Entities.CompoundConfigurationSettingsProperty()
                            {
                                PrimaryTitle = "Undergraduate",
                                PrimaryValue = "UG",
                                SecondaryTitle = "Associate of Arts in Automotice Technology",
                                SecondaryValue = "AUTO.AA",
                                TertiaryTitle = "Graduated",
                                TertiaryValue = "G"
                            }
                        },
                        PrimaryEntity = "ACAD.LEVELS",
                        PrimaryLabel = "Academic Level",
                        SecondaryEntity = "ACAD.PROGRAMS",
                        SecondaryLabel = "Academic Program",
                        TertiaryEntity = "ST.VALCODES",
                        TertiaryLabel = "Status",
                        TertiaryValcode = "STUDENT.PROGRAM.STATUSES"
                    },

                    new Domain.Base.Entities.CompoundConfigurationSettings("1d820443-1466-444f-a2be-51a456201456", "4",
                    "Colleague Subject/Department Mapping")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "courses"
                            }
                        },
                        Properties = new List<Domain.Base.Entities.CompoundConfigurationSettingsProperty>()
                        {
                            new Domain.Base.Entities.CompoundConfigurationSettingsProperty()
                            {
                                PrimaryTitle = "Nursing Subject",
                                PrimaryValue = "NURS",
                                SecondaryTitle = "Nursing Department",
                                SecondaryValue = "NURS"
                            }
                        }
                    }
                 };                    

                _compoundConfigurationSettingsOptionsCollection = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptions>()
                {
                    new Domain.Base.Entities.CompoundConfigurationSettingsOptions("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1",
                    "Payroll Deduction Occurrence Intervals")
                    {
                        EthosResource = "payroll-deduction-arrangements",
                        EthosPropertyName = "payPeriodOccurrence.interval",
                        PrimarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "2"
                           },
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "3"
                           }
                        },
                        SecondarySource = "BD.PERIOD.CODE",
                        SecondarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "P1"
                           },
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "L1"
                           }
                        },
                    },

                    new Domain.Base.Entities.CompoundConfigurationSettingsOptions("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2",
                    "Payroll Deduction Monthly Pay Periods")
                    {
                        EthosResource = "payroll-deduction-arrangements",
                        EthosPropertyName = "payPeriodOccurrence.monthlyPayPeriods",
                        PrimarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "-1"
                           },
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "0"
                           }
                        },
                        SecondarySource = "BD.PERIOD.CODE",
                        SecondarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "P1"
                           },
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "L1"
                           }
                        },
                    },


                    new Domain.Base.Entities.CompoundConfigurationSettingsOptions("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "3",
                    "Default Student Programs")
                    {
                        EthosResource = "section-registrations",
                        PrimarySource = "ACAD.LEVELS",
                        PrimarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "UG",
                               Title = "Undergraduate"
                           },
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "GR",
                               Title = "Graduate"
                           },
                        },
                        TertiarySource = "STUDENT.PROGRAM.STATUSES",
                        TertiarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "A",
                               Title = "Active"
                           },
                           new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                           {
                               Value = "G",
                               Title = "Graduated"
                           },
                        }
                    },

                    new Domain.Base.Entities.CompoundConfigurationSettingsOptions("1d820443-1466-444f-a2be-51a456201456", "4",
                    "Colleague Subject/Department Mapping")
                    {
                        EthosResource = "courses",
                        PrimarySource = "SUBJECTS",
                        PrimarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                            new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                            {
                                Title = "Nursing Subject",
                                Value = "NURS"
                            },
                            new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                            {
                                Title = "Accounting",
                                Value = "ACCT"
                            }
                        },
                        SecondarySource = "DEPTS",
                        SecondarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                        {
                            new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                            {
                                Title = "Nursing Department",
                                Value = "NURS"
                            },
                            new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                            {
                                Title = "Accounting",
                                Value = "ACCT"
                            }
                        }
                    }
                };


                // Build repository
                compoundConfigurationSettingsRepo = BuildValidCompoundConfigurationSettingsRepository();
                codeItemName = compoundConfigurationSettingsRepo.BuildFullCacheKey("AllIntgConfigSettings");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _compoundConfigurationSettingsCollection = null;
                compoundConfigurationSettingsRepo = null;
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetCompoundConfigurationSettingsCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_compoundConfigurationSettingsCollection, new SemaphoreSlim(1, 1)));

                var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsAsync(false);

                for (int i = 0; i < _compoundConfigurationSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).EthosResources.FirstOrDefault(), result.ElementAt(i).EthosResources.FirstOrDefault());
                }
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_CompoundConfigurationSettingsNonCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_compoundConfigurationSettingsCollection, new SemaphoreSlim(1, 1)));

                var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsAsync(true);

                for (int i = 0; i < _compoundConfigurationSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_compoundConfigurationSettingsCollection.ElementAt(i).EthosResources.FirstOrDefault(), result.ElementAt(i).EthosResources.FirstOrDefault());
                }
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetCompoundConfigurationSettingsByGuidCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_compoundConfigurationSettingsCollection, new SemaphoreSlim(1, 1)));

                foreach (var expected in _compoundConfigurationSettingsCollection)
                {
                    var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsByGuidAsync(expected.Guid, false);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Code, result.Code);
                    Assert.AreEqual(expected.Description, result.Description);
                    Assert.AreEqual(expected.EthosResources.FirstOrDefault(), result.EthosResources.FirstOrDefault());
                }
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetCompoundConfigurationSettingsByGuidNonCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_compoundConfigurationSettingsCollection, new SemaphoreSlim(1, 1)));

                foreach (var expected in _compoundConfigurationSettingsCollection)
                {
                    var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsByGuidAsync(expected.Guid, true);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Code, result.Code);
                    Assert.AreEqual(expected.Description, result.Description);
                    Assert.AreEqual(expected.EthosResources.FirstOrDefault(), result.EthosResources.FirstOrDefault());
                }
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetsCompoundConfigurationSettingsOptionsCacheAync()
            {
                var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsOptionsAsync(false);
                for (int i = 0; i < _compoundConfigurationSettingsOptionsCollection.Count(); i++)
                {
                    Assert.AreEqual(_compoundConfigurationSettingsOptionsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_compoundConfigurationSettingsOptionsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_compoundConfigurationSettingsOptionsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetsCompoundConfigurationSettingsOptionsNonCacheAync()
            {
                var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsOptionsAsync(true);
                for (int i = 0; i < _compoundConfigurationSettingsOptionsCollection.Count(); i++)
                {
                    Assert.AreEqual(_compoundConfigurationSettingsOptionsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_compoundConfigurationSettingsOptionsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_compoundConfigurationSettingsOptionsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetsCompoundConfigurationSettingsOptionsByGuidCacheAsync()
            {
                var expected = _compoundConfigurationSettingsOptionsCollection.FirstOrDefault();
                var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsOptionsByGuidAsync(expected.Guid, false);

                Assert.AreEqual(expected.Code, result.Code);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.EthosResource, result.EthosResource);
                Assert.AreEqual(expected.EthosPropertyName, result.EthosPropertyName);
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetsCompoundConfigurationSettingsOptionsByGuidNonCacheAsync()
            {
                var expected = _compoundConfigurationSettingsOptionsCollection.FirstOrDefault();
                var result = await compoundConfigurationSettingsRepo.GetCompoundConfigurationSettingsOptionsByGuidAsync(expected.Guid, true);

                Assert.AreEqual(expected.Code, result.Code);
                Assert.AreEqual(expected.Description, result.Description);
                Assert.AreEqual(expected.EthosResource, result.EthosResource);
                Assert.AreEqual(expected.EthosPropertyName, result.EthosPropertyName);
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetBendedCodesDictionary()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                var bendedCodes = new Collection<BendedBase>();
                TestCollectionConfigurationSettingsRepository _testCollectionConfigurationSettingsRepository = new TestCollectionConfigurationSettingsRepository(); ;
                var dictBended = _testCollectionConfigurationSettingsRepository.GetAllBendedCodesAsync(false);
                foreach (var dict in dictBended)
                {
                    bendedCodes.Add(new BendedBase() { Recordkey = dict.Key, BdDesc = dict.Value, BdPeriodCode = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<BendedBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(bendedCodes.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<BendedBase>("BENDED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(bendedCodes);

                var result = await compoundConfigurationSettingsRepo.GetAllPeriodCodes(false);

                Assert.IsInstanceOfType(result, typeof(List<string>));
                Assert.AreEqual(result.Count, bendedCodes.Count());
            }

            [TestMethod]
            public async Task CompoundConfigurationSettingsRepoTests_GetBendedCodesDictionary_ExceptionReturnEmptyCollection()
            {

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<BendedBase>("BENDED", It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var result = await compoundConfigurationSettingsRepo.GetAllPeriodCodes(false);

                Assert.IsInstanceOfType(result, typeof(List<string>));
                Assert.IsTrue(result.Count == 0);
            }


            private CompoundConfigurationSettingsRepository BuildValidCompoundConfigurationSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_compoundConfigurationSettingsOptionsCollection, new SemaphoreSlim(1, 1)));

                // Construct repository
                var apiSettings = new ApiSettings("TEST");
                compoundConfigurationSettingsRepo = new CompoundConfigurationSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return compoundConfigurationSettingsRepo;
            }
        }
    }
}
