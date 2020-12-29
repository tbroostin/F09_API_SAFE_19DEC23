// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AcademicProgramServiceTests
    {
        [TestClass]
        public class GetAcademicProgram
        {
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> _currentUserRepositoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepositoryMock;
            private Mock<ICatalogRepository> _catalogRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private Mock<IInstitutionRepository> _institutionRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private IAdapterRegistry _adapterRegistry;
            private IStudentReferenceDataRepository _studentReferenceDataRepository;
            private IReferenceDataRepository _referenceDataRepository;
            private ICatalogRepository _catalogRepository;
            private IInstitutionRepository _institutionRepository;

            private IPersonRepository _personRepository;
            private ILogger _logger;
            private AcademicProgramService _academicProgramService;
            private ICollection<Domain.Student.Entities.AcademicProgram> _academicProgramCollection = new List<Domain.Student.Entities.AcademicProgram>();
            private ICollection<AcademicCredential> _credentialCollection = new List<AcademicCredential>();
            private ICollection<AcademicDiscipline> _disciplineCollection = new List<AcademicDiscipline>();
            private ICollection<Domain.Student.Entities.AcademicLevel> _academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
            private ICollection<Domain.Base.Entities.Institution> _institutionCollection = new List<Domain.Base.Entities.Institution>();

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            [TestInitialize]
            public void Initialize()
            {
                SetupValidRepositoryData();

                _academicProgramService = new AcademicProgramService(_adapterRegistry,  _studentReferenceDataRepository,
                    _referenceDataRepository, _personRepository, _catalogRepository,  _institutionRepository,
                    _configurationRepositoryMock.Object, _currentUserRepositoryMock.Object, _roleRepositoryMock.Object, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _academicProgramCollection = null;
                _academicProgramService = null;
            }

            [TestMethod]
            public async Task AcademicProgramService__AcademicPrograms_NonHeDM()
            {
                var results = await _academicProgramService.GetAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.Student.AcademicProgram>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms_NonHeDM_Count()
            {
                var results = await _academicProgramService.GetAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms_NonHeDM_Properties()
            {
                var results = await _academicProgramService.GetAsync();
                var academicProgram = results.First(x => x.Code == "GR-UNDEC");
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.Description);
                Assert.IsNotNull(academicProgram.MinorCodes);

                academicProgram = results.First(x => x.Code == "BA-EDUC");
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.Description);
                Assert.IsNotNull(academicProgram.MajorCodes);

                academicProgram = results.First(x => x.Code == "MA-HIST");
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.Description);
                Assert.IsNotNull(academicProgram.SpecializationCodes);
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms_NonHeDM_Expected()
            {
                var expectedResults = _academicProgramCollection.First(c => c.Code == "GR-UNDEC");
                var results = await _academicProgramService.GetAsync();
                var academicProgram = results.First(s => s.Code == "GR-UNDEC");
                Assert.AreEqual(expectedResults.Code, academicProgram.Code);
                Assert.AreEqual(expectedResults.Description, academicProgram.Description);
                Assert.AreEqual(expectedResults.MinorCodes.FirstOrDefault(), academicProgram.MinorCodes.FirstOrDefault());

                expectedResults = _academicProgramCollection.First(c => c.Code == "BA-EDUC");
                academicProgram = results.First(s => s.Code == "BA-EDUC");
                Assert.AreEqual(expectedResults.Code, academicProgram.Code);
                Assert.AreEqual(expectedResults.Description, academicProgram.Description);
                Assert.AreEqual(expectedResults.MajorCodes.FirstOrDefault(), academicProgram.MajorCodes.FirstOrDefault());
                
                expectedResults = _academicProgramCollection.First(c => c.Code == "MA-HIST");
                academicProgram = results.First(s => s.Code == "MA-HIST");
                Assert.AreEqual(expectedResults.Code, academicProgram.Code);
                Assert.AreEqual(expectedResults.Description, academicProgram.Description);
                Assert.AreEqual(expectedResults.CertificateCodes.FirstOrDefault(), academicProgram.CertificateCodes.FirstOrDefault());
                Assert.AreEqual(expectedResults.SpecializationCodes.FirstOrDefault(), academicProgram.SpecializationCodes.FirstOrDefault());
            }

            #region v6 tests

            [TestMethod]
            public async Task AcademicProgramService__AcademicPrograms()
            {
                var results = await _academicProgramService.GetAcademicProgramsV6Async(false);
                Assert.IsTrue(results is IEnumerable<AcademicProgram2>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms_Count()
            {
                var results = await _academicProgramService.GetAcademicProgramsV6Async(false);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms_Properties()
            {
                var results = await _academicProgramService.GetAcademicProgramsV6Async(false);
                var academicProgram = results.First(x => x.Code == "GR-UNDEC");
                Assert.IsNotNull(academicProgram.Id);
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.StartDate);
                Assert.IsNotNull(academicProgram.EndDate);
                Assert.IsNotNull(academicProgram.Status);
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms_Expected()
            {
                var expectedResults = _academicProgramCollection.First(c => c.Code == "MA-HIST");
                var results = await _academicProgramService.GetAcademicProgramsV6Async(false);
                var academicProgram = results.First(s => s.Code == "MA-HIST");
                Assert.AreEqual(expectedResults.Guid, academicProgram.Id);
                Assert.AreEqual(expectedResults.Code, academicProgram.Code);
                Assert.AreEqual(expectedResults.StartDate, academicProgram.StartDate);
                Assert.AreEqual(expectedResults.EndDate, academicProgram.EndDate);
                Assert.AreEqual(AcademicProgramStatus.active, academicProgram.Status);
            }

            [TestMethod]
            public async Task AcademicProgramService_GetAcademicProgramByGuidV6_Expected()
            {
                var expectedResults =
                    _academicProgramCollection.First(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8");
                var academicProgram =
                    await _academicProgramService.GetAcademicProgramByGuidV6Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, academicProgram.Id);
                Assert.AreEqual(expectedResults.Code, academicProgram.Code);
                Assert.AreEqual(expectedResults.StartDate, academicProgram.StartDate);
                Assert.AreEqual(expectedResults.EndDate, academicProgram.EndDate);
                Assert.AreEqual(AcademicProgramStatus.inactive, academicProgram.Status);
            }

            [TestMethod]
            public async Task AcademicProgramService_GetAcademicProgramByGuid_Properties()
            {
                var academicProgram =
                   await _academicProgramService.GetAcademicProgramByGuidV6Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(academicProgram.Id);
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.StartDate);
                Assert.IsNotNull(academicProgram.EndDate);
                Assert.IsNotNull(academicProgram.Status);
            }
            #endregion v6 tests

            #region V10 GET/GET ALL

            [TestMethod]
            public async Task AcademicProgramService_GetAcademicProgramByGuid3_Expected()
            {
                var expectedResults =
                    _academicProgramCollection.First(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8");
                var academicProgram =
                    await _academicProgramService.GetAcademicProgramByGuid3Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, academicProgram.Id);
                Assert.AreEqual(expectedResults.Code, academicProgram.Code);
                Assert.AreEqual(expectedResults.StartDate, academicProgram.StartDate);
                Assert.AreEqual(expectedResults.EndDate, academicProgram.EndDate);
                Assert.AreEqual(AcademicProgramStatus.inactive, academicProgram.Status);
            }

            [TestMethod]
            public async Task AcademicProgramService_GetAcademicProgramByGuid3_Properties()
            {
                var academicProgram =
                   await _academicProgramService.GetAcademicProgramByGuid3Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(academicProgram.Id);
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.StartDate);
                Assert.IsNotNull(academicProgram.EndDate);
                Assert.IsNotNull(academicProgram.Status);
            }

            #endregion

            #region V15 GET/GET ALL

            [TestMethod]
            public async Task AcademicProgramService__AcademicPrograms4()
            {
                var results = await _academicProgramService.GetAcademicPrograms4Async("","", null, false);
                Assert.IsTrue(results is IEnumerable<AcademicProgram4>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_Count()
            {
                var results = await _academicProgramService.GetAcademicPrograms4Async("", "active", null, false);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_Properties()
            {
                AcademicProgram4 ap4 = new AcademicProgram4() { AcademicLevel = new GuidObject2("74588696-D1EC-2267-A0B7-DE602533E3A6") };
                var results = await _academicProgramService.GetAcademicPrograms4Async("", "", ap4, false);
                var academicProgram = results.First(x => x.Code == "BA-EDUC");
                Assert.IsNotNull(academicProgram.Id);
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.StartDate);
                Assert.IsNotNull(academicProgram.Status);
            }

            [TestMethod]
            public async Task AcademicProgramService_GetAcademicProgramByGuid4_Expected()
            {
                var expectedResults =
                    _academicProgramCollection.First(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8");
                var academicProgram =
                    await _academicProgramService.GetAcademicProgramByGuid4Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, academicProgram.Id);
                Assert.AreEqual(expectedResults.Code, academicProgram.Code);
                Assert.AreEqual(expectedResults.StartDate, academicProgram.StartDate);
                Assert.AreEqual(expectedResults.EndDate, academicProgram.EndDate);
                Assert.AreEqual(AcademicProgramStatus.inactive, academicProgram.Status);
            }
        
            [TestMethod]
            public async Task AcademicProgramService_GetAcademicProgramByGuid4_Properties()
            {
                var academicProgram =
                   await _academicProgramService.GetAcademicProgramByGuid4Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(academicProgram.Id);
                Assert.IsNotNull(academicProgram.Code);
                Assert.IsNotNull(academicProgram.StartDate);
                Assert.IsNotNull(academicProgram.EndDate);
                Assert.IsNotNull(academicProgram.Status);
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_Zero_Count()
            {
                var results = await _academicProgramService.GetAcademicPrograms4Async("", "actives", null, false);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_With_AcadCatalog_Count()
            {
                var results = await _academicProgramService.GetAcademicPrograms4Async("74588696-D1EC-2267-A0B7-DE602533E3A6", "", null, false);
                Assert.AreEqual(1, results.Count());
            }
                       
            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_Null_Academic_Programs()
            {
                _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(false)).ReturnsAsync(null);
                var results = await _academicProgramService.GetAcademicPrograms4Async("", "", null, false);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_Null_Academic_Catalogs()
            {
                _catalogRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(null);
                var results = await _academicProgramService.GetAcademicPrograms4Async("74588696-D1EC-2267-A0B7-DE602533E3A6", "", null, false);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_WrongGuid_Academic_Catalogs()
            {
                var results = await _academicProgramService.GetAcademicPrograms4Async("BAD_GUID", "", null, false);
                Assert.AreEqual(0, results.Count());
            }
            
            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_Null_Academic_Levels()
            {
                _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(null);
                var results = await _academicProgramService.GetAcademicPrograms4Async("", "",new AcademicProgram4() { AcademicLevel = new GuidObject2("BAD_GUID") }, false);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task AcademicProgramService_AcademicPrograms4_BAD_AcadLevel_Guid_Academic_Levels()
            {
                var results = await _academicProgramService.GetAcademicPrograms4Async("", "", new AcademicProgram4() { AcademicLevel = new GuidObject2("BAD_GUID") }, false);
                Assert.AreEqual(0, results.Count());
            }
            #endregion

            private void SetupValidRepositoryData()
            {                
                _catalogRepositoryMock = new Mock<ICatalogRepository>();
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                _currentUserRepositoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _institutionRepositoryMock = new Mock<IInstitutionRepository>();
                _institutionRepository = _institutionRepositoryMock.Object;
                _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _studentReferenceDataRepository = _studentReferenceDataRepositoryMock.Object;

                var personGuidCollection = new Dictionary<string, string>() { };
                 personGuidCollection.Add("0000123", guid);
                _personRepositoryMock = new Mock<IPersonRepository>();
                _personRepository = _personRepositoryMock.Object;
                _personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);

                List<Ellucian.Colleague.Domain.Student.Entities.Requirements.Catalog> catalogList = new List<Domain.Student.Entities.Requirements.Catalog>()
                {
                    new Domain.Student.Entities.Requirements.Catalog("74588696-D1EC-2267-A0B7-DE602533E3A6", "MA-HIST", DateTime.Today)
                    {
                        AcadPrograms = new List<string>(){ "MA-HIST" }
                    }
                };

                _catalogRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogList);

                _academicProgramCollection.Add(new Domain.Student.Entities.AcademicProgram("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "BA-EDUC", "BA in Education")
                    {
                        StartDate = new DateTime(2011, 12, 31),
                        DeptartmentCodes = new List<string>() { "HIST", "ENGL" },
                        DegreeCode = "BA",
                        AcadLevelCode = "UG",
                        MajorCodes = new List<string>() { "HIST" },
                        AuthorizingInstitute = new List<string>() {  "0000123" }
                    });
                _academicProgramCollection.Add(new Domain.Student.Entities.AcademicProgram("73244057-D1EC-4094-A0B7-DE602533E3A6", "MA-HIST", "MA History")
                    {                    
                        StartDate = new DateTime(2012, 12, 31),
                        CertificateCodes = new List<string>() { "TEACH" },
                        AcadLevelCode = "CE",
                        SpecializationCodes = new List<string>() { "CHIL" },
                        AuthorizingInstitute = new List<string>()
                    });
                _academicProgramCollection.Add(new Domain.Student.Entities.AcademicProgram("1df164eb-8178-4321-a9f7-24f12d3991d8", "GR-UNDEC", "Graduate Undecided")
                    {
                        StartDate = new DateTime(2012, 12, 31),
                        EndDate = new DateTime(2016, 12, 31),
                        HonorCode = "ACA",
                        AcadLevelCode = "GR",
                        MinorCodes = new List<string>() { "ENGL" },
                        AuthorizingInstitute = new List<string>()
                    });

                foreach (var acadProg in _academicProgramCollection)
                {
                    _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramByGuidAsync(acadProg.Guid)).ReturnsAsync(acadProg);
                }

                _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(false)).ReturnsAsync(_academicProgramCollection);
                _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(true)).ReturnsAsync(_academicProgramCollection);

                _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(_academicProgramCollection);

                _institutionCollection.Add(new Domain.Base.Entities.Institution("0000123", Domain.Base.Entities.InstType.College) { IsHostInstitution = false });
                _institutionRepositoryMock.Setup(repo => repo.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(_institutionCollection);
                _institutionRepositoryMock.Setup(repo => repo.GetInstitutionIdsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(new string[] { "0000123" });


                // Mock Reference Repository for Academic Level Entities
                _academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("74588696-D1EC-2267-A0B7-DE602533E3A6", "UG", "Undergraduate"));
                _academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("74826546-D1EC-2267-A0B7-DE602533E3A6", "GR", "Graduate"));
                _academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("54364536-D1EC-2267-A0B7-DE602533E3A6", "CE", "Continuing Education"));
                _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(_academicLevelCollection);
                
                // Mock Academic Credentials Service
               _credentialCollection.Add(new AcademicCredential()
                    {
                        Abbreviation = "BA",
                        Id = "1df164eb-8178-4321-a9f7-24f27f4456d8",
                        Title = "Bachelor of Arts",
                        AcademicCredentialType = AcademicCredentialType.Degree
                    });
                _credentialCollection.Add(new AcademicCredential()
                    {
                        Abbreviation = "ACA",
                        Id = "2br164eb-8178-4321-a9f7-24f27f4456d8",
                        Title = "General Academic Honors",
                        AcademicCredentialType = AcademicCredentialType.Honorary
                    });
                _credentialCollection.Add(new AcademicCredential()
                    {
                        Abbreviation = "TEACH",
                        Id = "2br164eb-8178-4321-a9f7-24f27f2276d8",
                        Title = "Teaching Certificate",
                        AcademicCredentialType = AcademicCredentialType.Certificate
                    });

                _disciplineCollection.Add(new AcademicDiscipline()
                    {
                        Abbreviation = "HIST",
                        Title = "History",
                        Id = "2br164eb-8178-4321-a9f7-24f27f2276d8",
                        Type = AcademicDisciplineType.Major
                    });
                _disciplineCollection.Add(new AcademicDiscipline()
                    {
                        Abbreviation = "ENGL",
                        Title = "English",
                        Id = "2fd164eb-8178-4333-a9f7-24f27f2276d8",
                        Type = AcademicDisciplineType.Minor
                    });
                _disciplineCollection.Add(new AcademicDiscipline()
                    {
                        Abbreviation = "CHIL",
                        Title = "Child Development",
                        Id = "2fd164eb-8178-4487-a9f7-24f27f2276d8",
                        Type = AcademicDisciplineType.Concentration
                    });
         
                var testRefDataRepo = new Domain.Base.Tests.TestAcademicDisciplineRepository();
                
                Mock<IReferenceDataRepository> refdataRepo = new Mock<IReferenceDataRepository>();
                refdataRepo.Setup(rd => rd.GetOtherMajorsAsync(false)).ReturnsAsync(testRefDataRepo.GetOtherMajors());
                refdataRepo.Setup(rd => rd.GetOtherMinorsAsync(false)).ReturnsAsync(testRefDataRepo.GetOtherMinors());
                refdataRepo.Setup(rd => rd.GetOtherSpecialsAsync(false)).ReturnsAsync(testRefDataRepo.GetOtherSpecials());

                _referenceDataRepository = refdataRepo.Object;
                _catalogRepository = _catalogRepositoryMock.Object;
                _loggerMock = new Mock<ILogger>();
                _logger = _loggerMock.Object;

                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                var academicProgramAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicProgram, Dtos.Student.AcademicProgram>(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicProgram, Dtos.Student.AcademicProgram>()).Returns(academicProgramAdapter);
            }
        }
    }
}