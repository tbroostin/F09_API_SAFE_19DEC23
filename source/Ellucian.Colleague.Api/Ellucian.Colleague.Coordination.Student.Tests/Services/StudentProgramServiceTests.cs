//using AutoMapper;
// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
//using Ellucian.Colleague.Coordination.Base.Services;
//using Ellucian.Colleague.Coordination.Student.Services;
//using Ellucian.Colleague.Data.Student.DataContracts;
//using Ellucian.Colleague.Data.Student.Transactions;
//using Ellucian.Colleague.Domain.Base.Repositories;
//using Ellucian.Colleague.Domain.Base.Tests;
//using Ellucian.Colleague.Domain.Repositories;
//using Ellucian.Colleague.Domain.Student.Entities;
//using Ellucian.Colleague.Domain.Student.Repositories;
//using Ellucian.Colleague.Domain.Student.Tests;
//using Ellucian.Colleague.Dtos;
//using Ellucian.Data.Colleague;
//using Ellucian.Data.Colleague.Repositories;
//using Ellucian.Web.Adapters;
//using Ellucian.Web.Http.Exceptions;
//using Ellucian.Web.Security;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Newtonsoft.Json;
//using slf4net;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Ellucian.Colleague.Coordination.Base.Tests.Services
//{
//    [TestClass]
//    public class StudentProgramServiceTests
//    {
//        public abstract class CurrentUserSetup
//        {
//            protected Ellucian.Colleague.Domain.Entities.Role viewStudentProgramRole = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.UPDATE.ACADEMIC.PROGRAM.ENROLLMENT");
//            protected Ellucian.Colleague.Domain.Entities.Role updateStudentProgramRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.ACADEMIC.PROGRAM.ENROLLMENT");

//            public class StudentUserFactory : ICurrentUserFactory
//            {
//                public ICurrentUser CurrentUser
//                {
//                    get
//                    {
//                        return new CurrentUser(new Claims()
//                        {
//                            ControlId = "123",
//                            Name = "Samwise",
//                            PersonId = "STU1",
//                            SecurityToken = "321",
//                            SessionTimeout = 30,
//                            UserName = "Samwise",
//                            Roles = new List<string>() { },
//                            SessionFixationId = "abc123"
//                        });
//                    }
//                }
//            }

//             Represents a third party system like ILP
//            public class ThirdPartyUserFactory : ICurrentUserFactory
//            {
//                public ICurrentUser CurrentUser
//                {
//                    get
//                    {
//                        return new CurrentUser(new Claims()
//                        {
//                            ControlId = "123",
//                            Name = "ILP",
//                            PersonId = "ILP",
//                            SecurityToken = "321",
//                            SessionTimeout = 30,
//                            UserName = "ILP",
//                            Roles = new List<string>() { "CREATE.UPDATE.ACADEMIC.PROGRAM.ENROLLMENT", "VIEW.ACADEMIC.PROGRAM.ENROLLMENT", "DELETE.ACADEMIC.PROGRAM.ENROLLMENT" },
//                            SessionFixationId = "abc123"
//                        });
//                    }
//                }
//            }
//        }

//        #region GetTests
//        [TestClass]
//        public class StudentProgramServiceTests_Get : CurrentUserSetup
//        {
//            private Mock<IAdapterRegistry> adapterRegistryMock;
//            private Mock<IPersonRepository> personRepositoryMock;
//            private Mock<IStudentRepository> studentRepositoryMock;
//            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
//            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
//            private Mock<ICurrentUserFactory> currentUserFactoryMock;
//            private Mock<IRoleRepository> roleRepositoryMock;
//            private Mock<ILogger> loggerMock;
//            private Mock<ITermRepository> termRepositoryMock;
//            ICurrentUserFactory curntUserFactory;
//            private StudentProgramService StudentProgramService;

//            StudentProgram StuProgEntity;
//            TestStudentProgramRepository repo;
//            List<Domain.Student.Entities.AcademicProgram> acadProgs;
//            private Task<IEnumerable<Domain.Student.Entities.AcademicProgram> acadProgs;
//            List<Domain.Student.Entities.Requirements.Catalog> catalogs;
//            List<Domain.Base.Entities.Location> locations;
//            List<Domain.Student.Entities.Term> terms;
//            ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection;
//            List<Dtos.AcademicCredential> credentials;
//            List<Dtos.AcademicDiscipline> disciplines;
//            Dtos.EnrollmentStatus status;
//            List<Domain.Student.Entities.EnrollmentStatus> statusItems;
//            StudentProgram response;
//            IEnumerable<Domain.Student.Entities.StudentProgram> stuAcadProgs;
//            Tuple<IEnumerable<Domain.Student.Entities.StudentProgram>, int> stuAcadProgsTuple;

//            private IEnumerable<Domain.Base.Entities.OtherHonor> allHonors;
//            private IEnumerable<Domain.Base.Entities.OtherDegree> allDegrees;
//            private IEnumerable<Domain.Base.Entities.OtherCcd> allCcds;
//            private IEnumerable<Domain.Base.Entities.OtherMajor> allMajors;
//            private IEnumerable<Domain.Base.Entities.OtherMinor> allMinors;
//            private IEnumerable<Domain.Base.Entities.OtherSpecial> allSp;

//            [TestInitialize]
//            public void Initialize()
//            {

//                adapterRegistryMock = new Mock<IAdapterRegistry>();
//                personRepositoryMock = new Mock<IPersonRepository>();
//                studentRepositoryMock = new Mock<IStudentRepository>();
//                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
//                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
//                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
//                roleRepositoryMock = new Mock<IRoleRepository>();
//                loggerMock = new Mock<ILogger>();
//                termRepositoryMock = new Mock<ITermRepository>();
//                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
//                repo = new TestStudentProgramRepository();

//                response = repo.GetAcademicProgramEnrollmentsAsync(It.IsAny<bool>());
//                BuildMocksForStudentProgramGet();

//                StudentProgramService = new StudentProgramService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentProgramRepositoryMock.Object, termRepositoryMock.Object, curntUserFactory, roleRepositoryMock.Object, loggerMock.Object);

//            }

//            private void BuildMocksForStudentProgramGet()
//            {
//                acadProgs = new TestAcademicProgramRepository().GetAsync().Result.ToList();
//                allHonors = new TestAcademicCredentialsRepository().GetOtherHonors();
//                allDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();
//                allCcds = new TestAcademicCredentialsRepository().GetOtherCcds();
//                credentials = new List<AcademicCredential>();
//                foreach (var source in allHonors)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Honorary
//                    };

//                    credentials.Add(academicCredential);
//                }

//                foreach (var source in allDegrees)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Degree
//                    };


//                    credentials.Add(academicCredential);
//                }

//                foreach (var source in allCcds)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Certificate
//                    };

//                    credentials.Add(academicCredential);
//                }
//                allMajors = new TestAcademicDisciplineRepository().GetOtherMajors();
//                allMinors = new TestAcademicDisciplineRepository().GetOtherMinors();
//                allSp = new TestAcademicDisciplineRepository().GetOtherSpecials();
//                disciplines = new List<AcademicDiscipline>();
//                foreach (var source in allMajors)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Major
//                    };

//                    disciplines.Add(academicDiscipline);
//                }

//                foreach (var source in allMinors)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Minor
//                    };

//                    disciplines.Add(academicDiscipline);
//                }

//                foreach (var source in allSp)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Concentration
//                    };

//                    disciplines.Add(academicDiscipline);
//                }
//                locations = new List<Domain.Base.Entities.Location>()
//                {
//                    new Domain.Base.Entities.Location("0d2f089a-b631-46cf-9884-e9d310eeb683","MC","MASTER CLASS"),
//                    new Domain.Base.Entities.Location("171e5d1f-910b-4f1a-a771-5847f554e8ab","SBCD","SIMPLE CLASS")
//                };

//                catalogs = new List<Domain.Student.Entities.Requirements.Catalog>()
//                {
//                    new Domain.Student.Entities.Requirements.Catalog("10909901-3d7f-4e6b-89ca-79b164cbd8cc","2012", DateTime.Today),
//                    new Domain.Student.Entities.Requirements.Catalog("25fa2969-ffc5-4b1e-aed6-77ab23621b57","2013", DateTime.Today),
//                    new Domain.Student.Entities.Requirements.Catalog("2c892ac9-b118-4c81-af6e-f30ea7e5a608","2014", DateTime.Today)
//                };
//                terms = new TestTermRepository().Get().ToList();
//                academicPeriodCollection = new TestAcademicPeriodRepository().Get().ToList();
//                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(terms);
//                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(terms)).Returns(academicPeriodCollection);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(false)).ReturnsAsync(allHonors);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegrees(false)).Returns(allDegrees);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcds(false)).Returns(allCcds);
//                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
//                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0012297");
//                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(acadProgs);
//                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(false)).ReturnsAsync(acadProgs);
//                referenceDataRepositoryMock.Setup(loc => loc.GetLocations(It.IsAny<bool>())).Returns(locations);
//                catalogRepositoryMock.Setup(cat => cat.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
//                catalogRepositoryMock.Setup(cat => cat.GetAsync()).ReturnsAsync(catalogs);
//                academicCredentialServiceMock.Setup(cred => cred.GetAcademicCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(credentials);
//                academicDisciplineServiceMock.Setup(disp => disp.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(disciplines);
//                statusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(false)).ReturnsAsync(statusItems);
//                stuAcadProgs = new TestStudentProgramRepository().GetAcademicProgramEnrollmentsAsync(It.IsAny<bool>()).Result.ToList();
//                stuAcadProgsTuple = new Tuple<IEnumerable<StudentProgram>, int>(stuAcadProgs, 3);
//            }

//            [TestCleanup]
//            public void Cleanup()
//            {
//                StudentProgramService = null;
//            }

//            [TestMethod]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentByGuidAsync()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
//                var expected = stuAcadProgs.FirstOrDefault();
//                expected.EndDate = DateTime.Parse("01/06/2018");
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(guid)).ReturnsAsync(expected);

//                Act
//                var result = await StudentProgramService.GetAcademicProgramEnrollmentByGuidAsync(guid);

//                Assert
//                Assert.AreEqual(result.Id, expected.Guid);
//                Assert.AreEqual(result.Program.Id, "1c5bbbbc-80e3-4042-8151-db9893ac337a");
//                Assert.AreEqual(result.Catalog.Id, "10909901-3d7f-4e6b-89ca-79b164cbd8cc");
//                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);
//                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894jck");
//                Assert.AreEqual(result.Site.Id, "0d2f089a-b631-46cf-9884-e9d310eeb683");
//                Assert.AreEqual(result.StartTerm.Id, "d1ef94c1-759c-4870-a3f4-34065bb522fe");
//                Assert.AreEqual(result.StartDate, expected.StartDate);
//                Assert.AreEqual(result.EndDate, expected.EndDate);
//                Assert.AreEqual(result.Student.Id, "0012297");
//                foreach (var dis in result.Disciplines)
//                {
//                    var acadDisp = disciplines.FirstOrDefault(disp => disp.Id == dis.Discipline.Id);
//                    if (acadDisp.Type == AcademicDisciplineType.Major)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "MATH");
//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Minor)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "HIST");
//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Concentration)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "CERT");
//                    }
//                }
//                foreach (var cred in result.Credentials)
//                {
//                    var acadCred = credentials.FirstOrDefault(cd => cd.Id == cred.Id);
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Degree)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, expected.DegreeCode);
//                    }
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Certificate)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, "ELE");
//                    }

//                }

//            }
//            [TestMethod]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentByGuidAsync_NoStatus()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
//                var expected = stuAcadProgs.FirstOrDefault();
//                expected.Status = "";
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(null);

//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(guid)).ReturnsAsync(expected);

//                Act
//                var result = await StudentProgramService.GetAcademicProgramEnrollmentByGuidAsync(guid);

//                Assert

//                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);



//            }
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentByGuidAsync_ArgumentNullException_NoGuid()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(It.IsAny<string>()))
//                .ThrowsAsync(new ArgumentNullException());
//                var result = await StudentProgramService.GetAcademicProgramEnrollmentByGuidAsync(null);

//            }


//            [TestMethod]
//            [ExpectedException(typeof(KeyNotFoundException))]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentByGuidAsync_KeyNotFoundException_NullEntity()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

//                var result = await StudentProgramService.GetAcademicProgramEnrollmentByGuidAsync("abcd");

//            }

//            [TestMethod]
//            [ExpectedException(typeof(KeyNotFoundException))]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentByGuidAsync_KeyNotFoundException_ConverttoDTO()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(false)).ThrowsAsync(new Exception());
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(stuAcadProgs.ToList()[0]);

//                var result = await StudentProgramService.GetAcademicProgramEnrollmentByGuidAsync("abcd");

//            }

//            [TestMethod]
//            [ExpectedException(typeof(PermissionsException))]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentByGuidAsync_PermissionsException()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

//                var result = await StudentProgramService.GetAcademicProgramEnrollmentByGuidAsync("abcd");

//            }

//            [TestMethod]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentsAsync()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);

//                Act
//                var acadProgEnroll = (await StudentProgramService.GetAcademicProgramEnrollmentsAsync(It.IsAny<int>(), It.IsAny<int>())).Item1;

//                Assert
//                for (var i = 0; i < stuAcadProgs.Count(); i++)
//                {
//                    var expected = stuAcadProgs.ToList()[i];
//                    var result = acadProgEnroll.ToList()[i];
//                    Assert.AreEqual(result.Id, expected.Guid);
//                    var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
//                    Assert.AreEqual(resProg.Code, expected.ProgramCode);
//                    var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
//                    Assert.AreEqual(resCata.Code, expected.CatalogCode);
//                    var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
//                    Assert.AreEqual(stat.Code, expected.Status);
//                    var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
//                    Assert.AreEqual(sites.Code, expected.Location);
//                    var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
//                    Assert.AreEqual(term.Code, expected.StartTerm);
//                    Assert.AreEqual(result.StartDate, expected.StartDate);
//                    Assert.AreEqual(result.EndDate, expected.EndDate);
//                    Assert.AreEqual(result.Student.Id, "0012297");
//                    foreach (var dis in result.Disciplines)
//                    {
//                        var acadDisp = disciplines.FirstOrDefault(disp => disp.Id == dis.Discipline.Id);
//                        if (acadDisp.Type == AcademicDisciplineType.Major)
//                        {
//                            Assert.AreEqual(acadDisp.Abbreviation, "MATH");

//                        }
//                        if (acadDisp.Type == AcademicDisciplineType.Minor)
//                        {
//                            Assert.AreEqual(acadDisp.Abbreviation, "HIST");
//                        }
//                        if (acadDisp.Type == AcademicDisciplineType.Concentration)
//                        {
//                            Assert.AreEqual(acadDisp.Abbreviation, "CERT");
//                        }
//                    }
//                    foreach (var cred in result.Credentials)
//                    {
//                        var acadCred = credentials.FirstOrDefault(cd => cd.Id == cred.Id);
//                        if (acadCred.AcademicCredentialType == AcademicCredentialType.Degree)
//                        {
//                            Assert.AreEqual(acadCred.Abbreviation, expected.DegreeCode);
//                        }
//                        if (acadCred.AcademicCredentialType == AcademicCredentialType.Certificate)
//                        {
//                            Assert.AreEqual(acadCred.Abbreviation, "ELE");
//                        }

//                    }
//                }

//            }

//            [TestMethod]
//            [ExpectedException(typeof(KeyNotFoundException))]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentsAsync_KeyNotFoundException_NullEntity()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);

//                var result = await StudentProgramService.GetAcademicProgramEnrollmentsAsync(It.IsAny<int>(), It.IsAny<int>());

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentsAsync_ArgumentException_BadStartDate()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                studentProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);

//                var acadProgEnroll = await StudentProgramService.GetAcademicProgramEnrollmentsAsync(0, 1, true, "0012297", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "3cf900894jck");

//            }

//            [TestMethod]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentsAsync_WithFilters()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
//                studentProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
//                Act
//                var acadProgEnroll = (await StudentProgramService.GetAcademicProgramEnrollmentsAsync(0, 1, true, "0012297", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active")).Item1;

//                Assert
//                for (var i = 0; i < stuAcadProgs.Count(); i++)
//                {
//                    var expected = stuAcadProgs.ToList()[i];
//                    var result = acadProgEnroll.ToList()[i];
//                    Assert.AreEqual(result.Id, expected.Guid);
//                    var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
//                    Assert.AreEqual(resProg.Code, expected.ProgramCode);
//                    var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
//                    Assert.AreEqual(resCata.Code, expected.CatalogCode);
//                    var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
//                    Assert.AreEqual(stat.Code, expected.Status);
//                    var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
//                    Assert.AreEqual(sites.Code, expected.Location);
//                    var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
//                    Assert.AreEqual(term.Code, expected.StartTerm);
//                    Assert.AreEqual(result.StartDate, expected.StartDate);
//                    Assert.AreEqual(result.EndDate, expected.EndDate);
//                    Assert.AreEqual(result.Student.Id, "0012297");
//                    foreach (var dis in result.Disciplines)
//                    {
//                        var acadDisp = disciplines.FirstOrDefault(disp => disp.Id == dis.Discipline.Id);
//                        if (acadDisp.Type == AcademicDisciplineType.Major)
//                        {
//                            Assert.AreEqual(acadDisp.Abbreviation, "MATH");

//                        }
//                        if (acadDisp.Type == AcademicDisciplineType.Minor)
//                        {
//                            Assert.AreEqual(acadDisp.Abbreviation, "HIST");
//                        }
//                        if (acadDisp.Type == AcademicDisciplineType.Concentration)
//                        {
//                            Assert.AreEqual(acadDisp.Abbreviation, "CERT");
//                        }
//                    }
//                    foreach (var cred in result.Credentials)
//                    {
//                        var acadCred = credentials.FirstOrDefault(cd => cd.Id == cred.Id);
//                        if (acadCred.AcademicCredentialType == AcademicCredentialType.Degree)
//                        {
//                            Assert.AreEqual(acadCred.Abbreviation, expected.DegreeCode);
//                        }
//                        if (acadCred.AcademicCredentialType == AcademicCredentialType.Certificate)
//                        {
//                            Assert.AreEqual(acadCred.Abbreviation, "ELE");
//                        }

//                    }
//                }

//            }

//            [TestMethod]
//            public async Task StudentProgramService_GetAcademicProgramEnrollmentsAsync_WithFilters_NoStartDate()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                we are setting the start date for the first entity to null. so this will not be returned, only 2 out of there will be returned.
//                stuAcadProgs.ToList()[0].StartDate = null;
//                input.StartDate = null;
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
//                studentProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
//                Act
//                var acadProgEnroll = (await StudentProgramService.GetAcademicProgramEnrollmentsAsync(0, 1, true, "0012297", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active")).Item1;
//                Assert
//                Assert.AreEqual(acadProgEnroll.Count(), 2);


//            }

//        }

//        #endregion

//        #region Post/PutTests
//        [TestClass]
//        public class StudentProgramServiceTests_Post : CurrentUserSetup
//        {
//            private Mock<IAdapterRegistry> adapterRegistryMock;
//            private Mock<IPersonRepository> personRepositoryMock;
//            private Mock<IStudentRepository> studentRepositoryMock;
//            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
//            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
//            private Mock<ICurrentUserFactory> currentUserFactoryMock;
//            private Mock<IRoleRepository> roleRepositoryMock;
//            private Mock<ILogger> loggerMock;
//            private Mock<ICatalogRepository> catalogRepositoryMock;
//            private Mock<ITermRepository> termRepositoryMock;
//            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
//            private Mock<IAcademicCredentialService> academicCredentialServiceMock;
//            private Mock<IAcademicDisciplineService> academicDisciplineServiceMock;
//            ICurrentUserFactory curntUserFactory;
//            private StudentProgramService StudentProgramService;

//            StudentProgram StuProgEntity;
//            TestStudentProgramRepository repo;
//            List<Domain.Student.Entities.AcademicProgram> acadProgs;
//            List<Domain.Student.Entities.Requirements.Catalog> catalogs;
//            List<Domain.Base.Entities.Location> locations;
//            List<Domain.Student.Entities.Term> terms;
//            ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection;
//            List<Dtos.AcademicCredential> credentials;
//            List<Dtos.AcademicDiscipline> disciplines;
//            List<Domain.Student.Entities.EnrollmentStatus> statusItems;
//            List<Domain.Student.Entities.StudentProgram> stuAcadProgs;
//            List<Dtos.StudentAcademicPrograms> acadProgEnrollDtos;
//            private IEnumerable<Domain.Base.Entities.OtherHonor> allHonors;
//            private IEnumerable<Domain.Base.Entities.OtherDegree> allDegrees;
//            private IEnumerable<Domain.Base.Entities.OtherCcd> allCcds;
//            private IEnumerable<Domain.Base.Entities.OtherMajor> allMajors;
//            private IEnumerable<Domain.Base.Entities.OtherMinor> allMinors;
//            private IEnumerable<Domain.Base.Entities.OtherSpecial> allSp;
//            [TestInitialize]
//            public void Initialize()
//            {

//                adapterRegistryMock = new Mock<IAdapterRegistry>();
//                personRepositoryMock = new Mock<IPersonRepository>();
//                studentRepositoryMock = new Mock<IStudentRepository>();
//                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
//                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
//                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
//                roleRepositoryMock = new Mock<IRoleRepository>();
//                loggerMock = new Mock<ILogger>();
//                termRepositoryMock = new Mock<ITermRepository>();
//                catalogRepositoryMock = new Mock<ICatalogRepository>();
//                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
//                academicCredentialServiceMock = new Mock<IAcademicCredentialService>();
//                academicDisciplineServiceMock = new Mock<IAcademicDisciplineService>();
//                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
//                repo = new TestStudentProgramRepository();

//                response = repo.GetAcademicProgramEnrollmentsAsync(It.IsAny<bool>());
//                BuildMocksForStudentProgramPost();
//                StudentProgramService = new StudentProgramService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentProgramRepositoryMock.Object, termRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, catalogRepositoryMock.Object, personRepositoryMock.Object,
//                                                                            referenceDataRepositoryMock.Object, academicCredentialServiceMock.Object, academicDisciplineServiceMock.Object, curntUserFactory, roleRepositoryMock.Object, loggerMock.Object);
//                acadProgEnrollDtos = BuildAcademicProgramEnrollmentsDtos();
//            }

//            private void BuildMocksForStudentProgramPost()
//            {
//                acadProgs = new TestAcademicProgramRepository().GetAsync().Result.ToList();
//                allHonors = new TestAcademicCredentialsRepository().GetOtherHonors();
//                allDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();
//                allCcds = new TestAcademicCredentialsRepository().GetOtherCcds();
//                credentials = new List<AcademicCredential>();
//                foreach (var source in allHonors)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Honorary
//                    };

//                    credentials.Add(academicCredential);
//                }

//                foreach (var source in allDegrees)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Degree
//                    };


//                    credentials.Add(academicCredential);
//                }

//                foreach (var source in allCcds)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Certificate
//                    };

//                    credentials.Add(academicCredential);
//                }
//                allMajors = new TestAcademicDisciplineRepository().GetOtherMajors();
//                allMinors = new TestAcademicDisciplineRepository().GetOtherMinors();
//                allSp = new TestAcademicDisciplineRepository().GetOtherSpecials();
//                disciplines = new List<AcademicDiscipline>();
//                foreach (var source in allMajors)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Major
//                    };

//                    disciplines.Add(academicDiscipline);
//                }

//                foreach (var source in allMinors)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Minor
//                    };

//                    disciplines.Add(academicDiscipline);
//                }

//                foreach (var source in allSp)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Concentration
//                    };

//                    disciplines.Add(academicDiscipline);
//                }
//                locations = new List<Domain.Base.Entities.Location>()
//                {
//                    new Domain.Base.Entities.Location("0d2f089a-b631-46cf-9884-e9d310eeb683","MC","MASTER CLASS"),
//                    new Domain.Base.Entities.Location("171e5d1f-910b-4f1a-a771-5847f554e8ab","SBCD","SIMPLE CLASS")
//                };
//                catalogs = new List<Domain.Student.Entities.Requirements.Catalog>()
//                {
//                    new Domain.Student.Entities.Requirements.Catalog("10909901-3d7f-4e6b-89ca-79b164cbd8cc","2012", DateTime.Today),
//                    new Domain.Student.Entities.Requirements.Catalog("25fa2969-ffc5-4b1e-aed6-77ab23621b57","2013", DateTime.Today),
//                    new Domain.Student.Entities.Requirements.Catalog("2c892ac9-b118-4c81-af6e-f30ea7e5a608","2014", DateTime.Today)
//                };

//                terms = new TestTermRepository().Get().ToList();
//                academicPeriodCollection = new TestAcademicPeriodRepository().Get().ToList();
//                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(terms);
//                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(terms)).Returns(academicPeriodCollection);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(false)).ReturnsAsync(allHonors);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegrees(false)).Returns(allDegrees);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcds(false)).Returns(allCcds);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsAsync(false)).ReturnsAsync(allMajors);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMinorsAsync(false)).ReturnsAsync(allMinors);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcds(false)).Returns(allCcds);
//                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
//                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0012297");
//                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(acadProgs);
//                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(false)).ReturnsAsync(acadProgs);
//                referenceDataRepositoryMock.Setup(loc => loc.GetLocations(It.IsAny<bool>())).Returns(locations);
//                catalogRepositoryMock.Setup(cat => cat.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
//                catalogRepositoryMock.Setup(cat => cat.GetAsync()).ReturnsAsync(catalogs);
//                academicCredentialServiceMock.Setup(cred => cred.GetAcademicCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(credentials);
//                academicDisciplineServiceMock.Setup(disp => disp.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(disciplines);
//                statusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(false)).ReturnsAsync(statusItems);
//                stuAcadProgs = new TestStudentProgramRepository().GetAcademicProgramEnrollmentsAsync(It.IsAny<bool>()).Result.ToList();
//            }

//            private static List<StudentAcademicPrograms> BuildAcademicProgramEnrollmentsDtos()
//            {
//                var academicProgramEnrollmentDtos = new List<Dtos.StudentAcademicPrograms>();
//                var acadProgEnrollDto1 = new Dtos.StudentAcademicPrograms()
//                {
//                    Id = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9",
//                    Program = new Dtos.GuidObject2() { Id = "1c5bbbbc-80e3-4042-8151-db9893ac337a" },
//                    Catalog = new Dtos.GuidObject2() { Id = "10909901-3d7f-4e6b-89ca-79b164cbd8cc" },
//                    Student = new Dtos.GuidObject2() { Id = "0012297" },
//                    Site = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
//                    StartTerm = new Dtos.GuidObject2() { Id = "d1ef94c1-759c-4870-a3f4-34065bb522fe" },
//                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
//                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4" }, new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } },
//                    Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
//                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2() { Id = "3cf900894jck" } }

//                };
//                var acadProgEnrollDto2 = new Dtos.StudentAcademicPrograms()
//                {
//                    Id = "45d8557f-56a9-4abc-8308-ee026983080c",
//                    Program = new Dtos.GuidObject2() { Id = "17a21cdc-7912-459e-a065-03895471a644" },
//                    Catalog = new Dtos.GuidObject2() { Id = "25fa2969-ffc5-4b1e-aed6-77ab23621b57" },
//                    Student = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
//                    Site = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
//                    StartTerm = new Dtos.GuidObject2() { Id = "d1ef94c1-759c-4870-a3f4-34065bb522fe" },
//                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
//                    EndDate = new DateTimeOffset(DateTime.Parse("01/06/2017")),
//                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } },
//                    Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
//                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Inactive, Detail = new GuidObject2() { Id = "3cf900894alk" } }

//                };
//                var acadProgEnrollDto3 = new Dtos.StudentAcademicPrograms()
//                {
//                    Id = "688583fc-6499-4a05-90b0-685745d6b465",
//                    Program = new Dtos.GuidObject2() { Id = "fbdfafd6-69a1-4362-88a0-62eac70da5c9" },
//                    Catalog = new Dtos.GuidObject2() { Id = "2c892ac9-b118-4c81-af6e-f30ea7e5a608" },
//                    Student = new Dtos.GuidObject2() { Id = "171e5d1f-910b-4f1a-a771-5847f554e8ab" },
//                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
//                    StartTerm = new Dtos.GuidObject2() { Id = "d1ef94c1-759c-4870-a3f4-34065bb522fe" },
//                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } },
//                     Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
//                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active }

//                };
//                academicProgramEnrollmentDtos.Add(acadProgEnrollDto1);
//                academicProgramEnrollmentDtos.Add(acadProgEnrollDto2);
//                academicProgramEnrollmentDtos.Add(acadProgEnrollDto3);
//                return academicProgramEnrollmentDtos;
//            }

//            [TestCleanup]
//            public void Cleanup()
//            {
//                StudentProgramService = null;
//            }

//            [TestMethod]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_Active_WithAll()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];

//                Act
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//                Assert
//                Assert.AreEqual(result.Id, expected.Guid);
//                var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
//                Assert.AreEqual(resProg.Code, expected.ProgramCode);
//                var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
//                Assert.AreEqual(resCata.Code, expected.CatalogCode);
//                var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
//                Assert.AreEqual(stat.Code, expected.Status);
//                var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
//                Assert.AreEqual(sites.Code, expected.Location);
//                var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
//                Assert.AreEqual(term.Code, expected.StartTerm);
//                Assert.AreEqual(result.StartDate, expected.StartDate);
//                Assert.AreEqual(result.EndDate, expected.EndDate);
//                Assert.AreEqual(result.Student.Id, "0012297");
//                foreach (var dis in result.Disciplines)
//                {
//                    var acadDisp = disciplines.FirstOrDefault(disp => disp.Id == dis.Discipline.Id);
//                    if (acadDisp.Type == AcademicDisciplineType.Major)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "MATH");

//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Minor)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "HIST");
//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Concentration)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "CERT");
//                    }
//                }
//                foreach (var cred in result.Credentials)
//                {
//                    var acadCred = credentials.FirstOrDefault(cd => cd.Id == cred.Id);
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Degree)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, expected.DegreeCode);
//                    }
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Certificate)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, "ELE");
//                    }

//                }

//            }

//            [TestMethod]
//            public async Task StudentProgramService_UpdateAcademicProgramEnrollmentAsync_Active_WithAll()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.UpdateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];

//                Act
//                var result = await StudentProgramService.UpdateAcademicProgramEnrollmentAsync(dto);

//                Assert
//                Assert.AreEqual(result.Id, expected.Guid);
//                var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
//                Assert.AreEqual(resProg.Code, expected.ProgramCode);
//                var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
//                Assert.AreEqual(resCata.Code, expected.CatalogCode);
//                var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
//                Assert.AreEqual(stat.Code, expected.Status);
//                var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
//                var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
//                Assert.AreEqual(term.Code, expected.StartTerm);
//                Assert.AreEqual(sites.Code, expected.Location);
//                Assert.AreEqual(result.StartDate, expected.StartDate);
//                Assert.AreEqual(result.EndDate, expected.EndDate);
//                Assert.AreEqual(result.Student.Id, "0012297");
//                foreach (var dis in result.Disciplines)
//                {
//                    var acadDisp = disciplines.FirstOrDefault(disp => disp.Id == dis.Discipline.Id);
//                    if (acadDisp.Type == AcademicDisciplineType.Major)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "MATH");

//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Minor)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "HIST");
//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Concentration)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "CERT");
//                    }
//                }
//                foreach (var cred in result.Credentials)
//                {
//                    var acadCred = credentials.FirstOrDefault(cd => cd.Id == cred.Id);
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Degree)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, expected.DegreeCode);
//                    }
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Certificate)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, "ELE");
//                    }

//                }

//            }
//            [TestMethod]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_Inactive_()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
//                dto.EnrollmentStatus.Detail = null;
//                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2019"));

//                Act
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//                Assert
//                Assert.AreEqual(result.Id, expected.Guid);
//                var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
//                Assert.AreEqual(resProg.Code, expected.ProgramCode);
//                var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
//                Assert.AreEqual(resCata.Code, expected.CatalogCode);
//                var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
//                Assert.AreEqual(stat.Code, expected.Status);
//                var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
//                Assert.AreEqual(sites.Code, expected.Location);
//                var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
//                Assert.AreEqual(term.Code, expected.StartTerm);
//                Assert.AreEqual(result.StartDate, expected.StartDate);
//                Assert.AreEqual(result.EndDate, expected.EndDate);
//                Assert.AreEqual(result.Student.Id, "0012297");
//                foreach (var dis in result.Disciplines)
//                {
//                    var acadDisp = disciplines.FirstOrDefault(disp => disp.Id == dis.Discipline.Id);
//                    if (acadDisp.Type == AcademicDisciplineType.Major)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "MATH");

//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Minor)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "HIST");
//                    }
//                    if (acadDisp.Type == AcademicDisciplineType.Concentration)
//                    {
//                        Assert.AreEqual(acadDisp.Abbreviation, "CERT");
//                    }
//                }
//                foreach (var cred in result.Credentials)
//                {
//                    var acadCred = credentials.FirstOrDefault(cd => cd.Id == cred.Id);
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Degree)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, expected.DegreeCode);
//                    }
//                    if (acadCred.AcademicCredentialType == AcademicCredentialType.Certificate)
//                    {
//                        Assert.AreEqual(acadCred.Abbreviation, "ELE");
//                    }

//                }

//            }

//            [TestMethod]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_EnrollStatus_Complete()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var expected = stuAcadProgs[1];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
//                dto.EnrollmentStatus.Detail = null;
//                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2019"));

//                Act
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//                Assert
//                var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
//                Assert.AreEqual(stat.Code, expected.Status);
//            }

//            [TestMethod]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_EnrollStatus_Active()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var expected = stuAcadProgs[1];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
//                dto.EnrollmentStatus.Detail = null;
//                Act
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//                Assert
//                var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
//                Assert.AreEqual(stat.Code, expected.Status);
//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentNullException_NoDTO()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);

//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(null);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentNullException_NoID()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Id = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }


//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NoProgram()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Program = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NoStudent()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Student = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NoStartDate()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.StartDate = default(DateTime);
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NoStatus()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = null;
//                dto.EnrollmentStatus.Detail = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_EndDateBeforeStart()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016"));
//                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2015"));
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_InactiveNoEndDate()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
//                dto.EndDate = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_CompleteNoEndDate()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
//                dto.EndDate = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }
//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_ActiveEndDate()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
//                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NullCredentials()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = null } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NullDiscipline()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = null } } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadPersonID()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
//                var dto = acadProgEnrollDtos[0];
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadProgram()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var progs = new List<Domain.Student.Entities.AcademicProgram>()
//                {
//                    new Domain.Student.Entities.AcademicProgram("0d2f089a-b631-46cf-9884-e9d310eeb00","MC","MASTER CLASS")
//                };
//                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(progs);
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);

//                var dto = acadProgEnrollDtos[0];
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }


//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NullCatalog()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Catalog.Id = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadCatalog()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                catalogRepositoryMock.Setup(cat => cat.GetAsync()).ReturnsAsync(null);
//                var dto = acadProgEnrollDtos[0];
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NullLocation()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Site.Id = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NullStartTerm()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.StartTerm.Id = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadLocation()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var sites = new List<Domain.Base.Entities.Location>()
//                {
//                    new Domain.Base.Entities.Location("0d2f089a-b631-46cf-9884-e9d310eeb00","MC","MASTER CLASS")
//                };
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                referenceDataRepositoryMock.Setup(loc => loc.GetLocations(false)).Returns(sites);
//                var dto = acadProgEnrollDtos[0];
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadStartTerm()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var expected = stuAcadProgs[0];
//                var period = new List<Domain.Student.Entities.AcademicPeriod>()
//                {
//                    new Domain.Student.Entities.AcademicPeriod("0d2f089a-b631-46cf-9884-e9d310eeb00","2002","fall",DateTime.Today,DateTime.Today,200,1,"2002","","")
//                };
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(terms)).Returns(period);
//                var dto = acadProgEnrollDtos[0];
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NullStatusDetail()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.Detail.Id = null;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(PermissionsException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_PermissionsException()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);



//            }


//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadCredentials()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "13456" } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadCredentials_AcadProgram_Degree()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadCredentials_AcadProgram_Spec()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4" }, new GuidObject2() { Id = "31d8aa32-dbe6-83j7-a1c4-2cad39e232e4" } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadCredentials_Honorary()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadCredentials_2Degrees()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" }, new GuidObject2() { Id = "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4" } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadCredentials_Diploma()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                var credential = new List<Dtos.AcademicCredential>()
//                {
//                    new Dtos.AcademicCredential(){Id = "123456", AcademicCredentialType = Dtos.AcademicCredentialType.Diploma}

//                };
//                academicCredentialServiceMock.Setup(cred => cred.GetAcademicCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(credential);
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "123456" } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadDisciplines()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "fd-4937-b97b-3c9ad596e023" } } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadDisciplines_AcadProg_Major()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadDisciplines_AcadProg_Minor()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadDisciplines_AcadProg_Specilization()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "31d8aa32-dbe6-83j7-a1c4-2cad39e232e4" } } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadSubDisciplines()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                var subDisp = new List<GuidObject2>() { new GuidObject2() { Id = "1234" } };
//                dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { SubDisciplines = subDisp, Discipline = new GuidObject2() { Id = "fd-4937-b97b-3c9ad596e023" } } };
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }



//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_NullDisciplines()
//            {
//                //Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = null } } ;
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadEnrollStatus()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
//                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2019"));
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentException))]
//            public async Task StudentProgramService_CreateAcademicProgramEnrollmentAsync_ArgumentException_BadEnrollStatus_Detail()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.CreateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];
//                dto.EnrollmentStatus.Detail.Id = "12345";
//                var result = await StudentProgramService.CreateAcademicProgramEnrollmentAsync(dto);

//            }


//        }
//        #endregion

//        #region DeleteTests
//        [TestClass]
//        public class StudentProgramServiceTests_Delete : CurrentUserSetup
//        {
//            private Mock<IAdapterRegistry> adapterRegistryMock;
//            private Mock<IPersonRepository> personRepositoryMock;
//            private Mock<IStudentRepository> studentRepositoryMock;
//            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
//            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
//            private Mock<ICurrentUserFactory> currentUserFactoryMock;
//            private Mock<IRoleRepository> roleRepositoryMock;
//            private Mock<ILogger> loggerMock;
//            private Mock<ICatalogRepository> catalogRepositoryMock;
//            private Mock<ITermRepository> termRepositoryMock;
//            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
//            private Mock<IAcademicCredentialService> academicCredentialServiceMock;
//            private Mock<IAcademicDisciplineService> academicDisciplineServiceMock;
//            ICurrentUserFactory curntUserFactory;
//            private StudentProgramService StudentProgramService;

//            StudentProgram StuProgEntity;
//            TestStudentProgramRepository repo;
//            List<Domain.Student.Entities.AcademicProgram> acadProgs;
//            List<Domain.Student.Entities.Requirements.Catalog> catalogs;
//            List<Domain.Base.Entities.Location> locations;
//            List<Domain.Student.Entities.Term> terms;
//            ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection;
//            List<Dtos.AcademicCredential> credentials;
//            List<Dtos.AcademicDiscipline> disciplines;
//            List<Domain.Student.Entities.EnrollmentStatus> statusItems;
//            List<Domain.Student.Entities.StudentProgram> stuAcadProgs;
//            List<Dtos.StudentAcademicPrograms> acadProgEnrollDtos;
//            private IEnumerable<Domain.Base.Entities.OtherHonor> allHonors;
//            private IEnumerable<Domain.Base.Entities.OtherDegree> allDegrees;
//            private IEnumerable<Domain.Base.Entities.OtherCcd> allCcds;
//            private IEnumerable<Domain.Base.Entities.OtherMajor> allMajors;
//            private IEnumerable<Domain.Base.Entities.OtherMinor> allMinors;
//            private IEnumerable<Domain.Base.Entities.OtherSpecial> allSp;
//            [TestInitialize]
//            public void Initialize()
//            {

//                adapterRegistryMock = new Mock<IAdapterRegistry>();
//                personRepositoryMock = new Mock<IPersonRepository>();
//                studentRepositoryMock = new Mock<IStudentRepository>();
//                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
//                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
//                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
//                roleRepositoryMock = new Mock<IRoleRepository>();
//                loggerMock = new Mock<ILogger>();
//                termRepositoryMock = new Mock<ITermRepository>();
//                catalogRepositoryMock = new Mock<ICatalogRepository>();
//                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
//                academicCredentialServiceMock = new Mock<IAcademicCredentialService>();
//                academicDisciplineServiceMock = new Mock<IAcademicDisciplineService>();
//                curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
//                repo = new TestStudentProgramRepository();

//                response = repo.GetAcademicProgramEnrollmentsAsync(It.IsAny<bool>());
//                BuildMocksForStudentProgramDelete();
//                StudentProgramService = new StudentProgramService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentProgramRepositoryMock.Object, termRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, catalogRepositoryMock.Object, personRepositoryMock.Object,
//                                                                            referenceDataRepositoryMock.Object, academicCredentialServiceMock.Object, academicDisciplineServiceMock.Object, curntUserFactory, roleRepositoryMock.Object, loggerMock.Object);
//                acadProgEnrollDtos = BuildAcademicProgramEnrollmentsDtos();
//            }

//            private void BuildMocksForStudentProgramDelete()
//            {
//                acadProgs = new TestAcademicProgramRepository().GetAsync().Result.ToList();
//                allHonors = new TestAcademicCredentialsRepository().GetOtherHonors();
//                allDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();
//                allCcds = new TestAcademicCredentialsRepository().GetOtherCcds();
//                credentials = new List<AcademicCredential>();
//                foreach (var source in allHonors)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Honorary
//                    };

//                    credentials.Add(academicCredential);
//                }

//                foreach (var source in allDegrees)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Degree
//                    };


//                    credentials.Add(academicCredential);
//                }

//                foreach (var source in allCcds)
//                {
//                    var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        AcademicCredentialType = Dtos.AcademicCredentialType.Certificate
//                    };

//                    credentials.Add(academicCredential);
//                }
//                allMajors = new TestAcademicDisciplineRepository().GetOtherMajors();
//                allMinors = new TestAcademicDisciplineRepository().GetOtherMinors();
//                allSp = new TestAcademicDisciplineRepository().GetOtherSpecials();
//                disciplines = new List<AcademicDiscipline>();
//                foreach (var source in allMajors)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Major
//                    };

//                    disciplines.Add(academicDiscipline);
//                }

//                foreach (var source in allMinors)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Minor
//                    };

//                    disciplines.Add(academicDiscipline);
//                }

//                foreach (var source in allSp)
//                {
//                    var academicDiscipline = new Ellucian.Colleague.Dtos.AcademicDiscipline
//                    {
//                        Id = source.Guid,
//                        Abbreviation = source.Code,
//                        Title = source.Description,
//                        Description = null,
//                        Type = Dtos.AcademicDisciplineType.Concentration
//                    };

//                    disciplines.Add(academicDiscipline);
//                }
//                locations = new List<Domain.Base.Entities.Location>()
//                {
//                    new Domain.Base.Entities.Location("0d2f089a-b631-46cf-9884-e9d310eeb683","MC","MASTER CLASS"),
//                    new Domain.Base.Entities.Location("171e5d1f-910b-4f1a-a771-5847f554e8ab","SBCD","SIMPLE CLASS")
//                };
//                catalogs = new List<Domain.Student.Entities.Requirements.Catalog>()
//                {
//                    new Domain.Student.Entities.Requirements.Catalog("10909901-3d7f-4e6b-89ca-79b164cbd8cc","2012", DateTime.Today),
//                    new Domain.Student.Entities.Requirements.Catalog("25fa2969-ffc5-4b1e-aed6-77ab23621b57","2013", DateTime.Today),
//                    new Domain.Student.Entities.Requirements.Catalog("2c892ac9-b118-4c81-af6e-f30ea7e5a608","2014", DateTime.Today)
//                };
//                terms = new TestTermRepository().Get().ToList();
//                academicPeriodCollection = new TestAcademicPeriodRepository().Get().ToList();
//                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(terms);
//                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(terms)).Returns(academicPeriodCollection);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(false)).ReturnsAsync(allHonors);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegrees(false)).Returns(allDegrees);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcds(false)).Returns(allCcds);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsAsync(false)).ReturnsAsync(allMajors);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMinorsAsync(false)).ReturnsAsync(allMinors);
//                referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcds(false)).Returns(allCcds);
//                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0012297");
//                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0012297");
//                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(acadProgs);
//                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(false)).ReturnsAsync(acadProgs);
//                referenceDataRepositoryMock.Setup(loc => loc.GetLocations(It.IsAny<bool>())).Returns(locations);
//                catalogRepositoryMock.Setup(cat => cat.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
//                catalogRepositoryMock.Setup(cat => cat.GetAsync()).ReturnsAsync(catalogs);
//                academicCredentialServiceMock.Setup(cred => cred.GetAcademicCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(credentials);
//                academicDisciplineServiceMock.Setup(disp => disp.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(disciplines);
//                statusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
//                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(false)).ReturnsAsync(statusItems);
//                stuAcadProgs = new TestStudentProgramRepository().GetAcademicProgramEnrollmentsAsync(It.IsAny<bool>()).Result.ToList();
//            }

//            private static List<StudentAcademicPrograms> BuildAcademicProgramEnrollmentsDtos()
//            {
//                var academicProgramEnrollmentDtos = new List<Dtos.StudentAcademicPrograms>();
//                var acadProgEnrollDto1 = new Dtos.StudentAcademicPrograms()
//                {
//                    Id = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9",
//                    Program = new Dtos.GuidObject2() { Id = "1c5bbbbc-80e3-4042-8151-db9893ac337a" },
//                    Catalog = new Dtos.GuidObject2() { Id = "10909901-3d7f-4e6b-89ca-79b164cbd8cc" },
//                    Student = new Dtos.GuidObject2() { Id = "0012297" },
//                    Site = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
//                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
//                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4" }, new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } },
//                    Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
//                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2() { Id = "3cf900894jck" } }

//                };
//                var acadProgEnrollDto2 = new Dtos.StudentAcademicPrograms()
//                {
//                    Id = "45d8557f-56a9-4abc-8308-ee026983080c",
//                    Program = new Dtos.GuidObject2() { Id = "17a21cdc-7912-459e-a065-03895471a644" },
//                    Catalog = new Dtos.GuidObject2() { Id = "25fa2969-ffc5-4b1e-aed6-77ab23621b57" },
//                    Student = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
//                    Site = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
//                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
//                    EndDate = new DateTimeOffset(DateTime.Parse("01/06/2017")),
//                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } },
//                    Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
//                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Inactive, Detail = new GuidObject2() { Id = "3cf900894alk" } }

//                };
//                var acadProgEnrollDto3 = new Dtos.StudentAcademicPrograms()
//                {
//                    Id = "688583fc-6499-4a05-90b0-685745d6b465",
//                    Program = new Dtos.GuidObject2() { Id = "fbdfafd6-69a1-4362-88a0-62eac70da5c9" },
//                    Catalog = new Dtos.GuidObject2() { Id = "2c892ac9-b118-4c81-af6e-f30ea7e5a608" },
//                    Student = new Dtos.GuidObject2() { Id = "171e5d1f-910b-4f1a-a771-5847f554e8ab" },
//                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
//                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } },
//                     Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
//                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active }

//                };
//                academicProgramEnrollmentDtos.Add(acadProgEnrollDto1);
//                academicProgramEnrollmentDtos.Add(acadProgEnrollDto2);
//                academicProgramEnrollmentDtos.Add(acadProgEnrollDto3);
//                return academicProgramEnrollmentDtos;
//            }

//            [TestCleanup]
//            public void Cleanup()
//            {
//                StudentProgramService = null;
//            }

//            [TestMethod]
//            public async Task StudentProgramService_DeleteAcademicProgramEnrollmentsAsync()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.DeleteAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
//                var expected = stuAcadProgs[0];
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(guid)).ReturnsAsync(expected);
//                studentProgramRepositoryMock.Setup(s => s.UpdateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];

//                Act
//                await StudentProgramService.DeleteAcademicProgramEnrollmentsAsync(guid);

//                Assert
//                Assert.AreEqual(dto.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Inactive);
//                Assert.AreEqual(dto.EndDate, DateTime.Today);


//            }

//            [TestMethod]
//            public async Task StudentProgramService_DeleteAcademicProgramEnrollmentsAsync_FutureStartDate()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.DeleteAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
//                var expected = stuAcadProgs[0];
//                expected.StartDate = DateTime.Parse("01/06/2017");
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(guid)).ReturnsAsync(expected);
//                studentProgramRepositoryMock.Setup(s => s.UpdateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];

//                Act
//                await StudentProgramService.DeleteAcademicProgramEnrollmentsAsync(guid);

//                Assert
//                Assert.AreEqual(dto.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Inactive);
//                Assert.AreEqual(dto.EndDate, DateTime.Today);


//            }

//            [TestMethod]
//            [ExpectedException(typeof(ArgumentNullException))]
//            public async Task StudentProgramService_DeleteAcademicProgramEnrollmentsAsync_ArgumentNullException_NoGuid()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.DeleteAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
//                var expected = stuAcadProgs[0];
//                expected.StartDate = DateTime.Parse("01/06/2017");
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(guid)).ReturnsAsync(expected);
//                studentProgramRepositoryMock.Setup(s => s.UpdateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];

//                Act
//                await StudentProgramService.DeleteAcademicProgramEnrollmentsAsync(null);

//            }


//            [TestMethod]
//            [ExpectedException(typeof(KeyNotFoundException))]
//            public async Task StudentProgramService_DeleteAcademicProgramEnrollmentsAsync_KeyNotFoundException_BadGuid()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.DeleteAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77";
//                var expected = stuAcadProgs[0];
//                expected.StartDate = DateTime.Parse("01/06/2017");
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(guid)).ReturnsAsync(null);
//                studentProgramRepositoryMock.Setup(s => s.UpdateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];

//                Act
//                await StudentProgramService.DeleteAcademicProgramEnrollmentsAsync(guid);

//            }

//            [TestMethod]
//            [ExpectedException(typeof(PermissionsException))]
//            public async Task StudentProgramService_DeleteAcademicProgramEnrollmentsAsync_PermissionsException()
//            {
//                Arrange
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.DeleteAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAcademicProgramEnrollmentConsent));
//                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewAcademicProgramEnrollmentConsent));
//                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
//                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77";
//                var expected = stuAcadProgs[0];
//                expected.StartDate = DateTime.Parse("01/06/2017");
//                studentProgramRepositoryMock.Setup(s => s.GetAcademicProgramEnrollmentByGuidAsync(guid)).ReturnsAsync(null);
//                studentProgramRepositoryMock.Setup(s => s.UpdateAcademicProgramEnrollmentAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
//                var dto = acadProgEnrollDtos[0];

//                Act
//                await StudentProgramService.DeleteAcademicProgramEnrollmentsAsync(guid);

//            }

//        }

//        #endregion
//    }
//}