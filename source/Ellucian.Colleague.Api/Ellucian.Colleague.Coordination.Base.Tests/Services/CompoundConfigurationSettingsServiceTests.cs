//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CompoundConfigurationSettingsServiceTests
    {
        private const string compoundConfigurationSettingsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string compoundConfigurationSettingsCode = "1";
        private ICollection<Domain.Base.Entities.CompoundConfigurationSettings> _compoundConfigurationSettingsCollection;
        private ICollection<Domain.Base.Entities.CompoundConfigurationSettingsOptions> _compoundConfigurationSettingsOptionsCollection;
        private CompoundConfigurationSettingsService _compoundConfigurationSettingsService;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<ICompoundConfigurationSettingsRepository> _compoundConfigurationRepoMock;
        private Mock<ICompoundConfigurationSettingsRepository> _compoundConfigurationSettingsRepoMock;


        [TestInitialize]
        public void Initialize()
        {
            _compoundConfigurationSettingsRepoMock = new Mock<ICompoundConfigurationSettingsRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _compoundConfigurationRepoMock = new Mock<ICompoundConfigurationSettingsRepository>();

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

                new Domain.Base.Entities.CompoundConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "3",
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


            _compoundConfigurationSettingsRepoMock.Setup(repo => repo.GetCompoundConfigurationSettingsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_compoundConfigurationSettingsCollection);

            _compoundConfigurationSettingsRepoMock.Setup(repo => repo.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_compoundConfigurationSettingsCollection.FirstOrDefault(cs => cs.Code == compoundConfigurationSettingsCode));


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

                new Domain.Base.Entities.CompoundConfigurationSettingsOptions("1d820443-1466-444f-a2be-51a456201456", "3",
                "Colleague Subject/Department Mapping")
                {
                    EthosResource = "courses",
                    PrimarySource = "SUBJECTS",
                    PrimarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                    {
                       new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                       {
                           Value = "MATH",
                           Title = "Mathematics Subject"
                       },
                       new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                       {
                           Value = "ENGL",
                           Title = "English Subject"
                       },
                    },
                    SecondarySource = "DEPTS",
                    TertiarySourceData = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource>()
                    {
                       new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                       {
                           Value = "MATH",
                           Title = "Mathematics Department"
                       },
                       new Domain.Base.Entities.CompoundConfigurationSettingsOptionsSource()
                       {
                           Value = "ENGL",
                           Title = "English Department"
                       },
                    }
                }
            };


            _compoundConfigurationSettingsRepoMock.Setup(repo => repo.GetCompoundConfigurationSettingsOptionsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_compoundConfigurationSettingsOptionsCollection);

            _compoundConfigurationSettingsService = new CompoundConfigurationSettingsService(_referenceRepositoryMock.Object,
                _compoundConfigurationSettingsRepoMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _compoundConfigurationSettingsService = null;
            _compoundConfigurationSettingsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
            _compoundConfigurationRepoMock = null;
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsService_GetCompoundConfigurationSettingsAsync()
        {
            var results = await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsAsync
                (new List<CompoundConfigurationSettingsEthos>(), true);
            Assert.IsTrue(results is IEnumerable<Dtos.CompoundConfigurationSettings>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsService_GetCompoundConfigurationSettingsAsync_Count()
        {
            var results = await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsAsync
                (new List<CompoundConfigurationSettingsEthos>(), true);
            Assert.AreEqual(4, results.Count());
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsService_GetCompoundConfigurationSettingsAsync_Properties()
        {
            var result =
                (await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsAsync
                (new List<CompoundConfigurationSettingsEthos>(), true)).FirstOrDefault(x => x.Id == compoundConfigurationSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Description);
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsService_GetCompoundConfigurationSettingsAsync_Expected()
        {
            var expectedResults = _compoundConfigurationSettingsCollection.FirstOrDefault(c => c.Guid == compoundConfigurationSettingsGuid);
            var actualResult =
                (await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsAsync
                (new List<CompoundConfigurationSettingsEthos>(), true)).FirstOrDefault(x => x.Id == compoundConfigurationSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Description);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CompoundConfigurationSettingsService_GetCompoundConfigurationSettingsByGuidAsync_Empty()
        {
            _compoundConfigurationSettingsRepoMock.Setup(repo => repo.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ConfigurationSettingsService_GetCompoundConfigurationSettingsByGuidAsync_Null()
        {
            _compoundConfigurationSettingsRepoMock.Setup(repo => repo.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ConfigurationSettingsService_GetCompoundConfigurationSettingsByGuidAsync_InvalidId()
        {
            _compoundConfigurationSettingsRepoMock.Setup(repo => repo.GetCompoundConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsByGuidAsync("99");
        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetCompoundConfigurationSettingsByGuidAsync_Expected()
        {
            var expectedResults =
                _compoundConfigurationSettingsCollection.First(c => c.Guid == compoundConfigurationSettingsGuid);
            var actualResult =
                await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsByGuidAsync(compoundConfigurationSettingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Description);

        }

        [TestMethod]
        public async Task ConfigurationSettingsService_GetCompoundConfigurationSettingsByGuidAsync_Properties()
        {
            var result =
                await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsByGuidAsync(compoundConfigurationSettingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Description);
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsService_GetCompoundConfigurationSettingsOptionsAsync_Properties()
        {
            var resultCollection =
                (await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsOptionsAsync(new List<Dtos.DtoProperties.CompoundConfigurationSettingsEthos>(), true));

            foreach (var result in resultCollection)
            {
                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
            }
        }

        [TestMethod]
        public async Task CompoundConfigurationSettingsService_GetCompoundConfigurationSettingsOptionsByGuidAsync_Properties()
        {
            foreach (var setting in _compoundConfigurationSettingsCollection)
            {
                _compoundConfigurationSettingsRepoMock.Setup(repo => repo.GetCompoundConfigurationSettingsOptionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_compoundConfigurationSettingsOptionsCollection.FirstOrDefault(cs => cs.Guid == setting.Guid));

                var result =
                    (await _compoundConfigurationSettingsService.GetCompoundConfigurationSettingsOptionsByGuidAsync(setting.Guid, true));

                Assert.IsNotNull(result.Id, "Guid");
                Assert.IsNotNull(result.Ethos, "Ethos Resources");
            }
        }
    }
}