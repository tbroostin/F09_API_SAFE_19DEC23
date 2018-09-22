// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
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

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    public abstract class StudentUserSetup
    {
        protected Ellucian.Colleague.Domain.Entities.Role viewStudentProgramRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.PROGRAM");
        protected Ellucian.Colleague.Domain.Entities.Role updateStudentProgramRole = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.UPDATE.STUDENT.ACADEMIC.PROGRAM");
        protected Ellucian.Colleague.Domain.Entities.Role deleteStudentProgramRole = new Ellucian.Colleague.Domain.Entities.Role(1, "DELETE.STUDENT.ACADEMIC.PROGRAM");
        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Samwise",
                        PersonId = "STU1",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Samwise",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        // Represents a third party system like ILP
        public class ThirdPartyUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "ILP",
                        PersonId = "ILP",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "ILP",
                        Roles = new List<string>() { "CREATE.UPDATE.STUDENT.ACADEMIC.PROGRAM", "VIEW.STUDENT.ACADEMIC.PROGRAM", "DELETE.STUDENT.ACADEMIC.PROGRAM" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }

    [TestClass]
    public class StudentAcademicProgramServiceTests
    {
        #region GetTests
        [TestClass]
        public class StudentAcademicProgramServiceTests_Get : StudentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IStudentAcademicProgramRepository> studentAcademicProgramRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<ICatalogRepository> catalogRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private StudentAcademicProgramService StudentAcademicProgramService;

            StudentAcademicProgram StuAcadProgEntity;
            TestStudentProgramRepository repo;
            List<Domain.Student.Entities.AcademicProgram> acadProgs;
            List<Domain.Student.Entities.Requirements.Catalog> catalogs;
            List<Domain.Base.Entities.Location> locations;
            List<Domain.Base.Entities.Department> depts;
            List<Domain.Student.Entities.AcademicLevel> acadLevels;
            List<Domain.Student.Entities.Term> terms;
            ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection;
            List<Domain.Student.Entities.EnrollmentStatus> statusItems;
            private IEnumerable<AcadCredential> acadCredentails;

            //StudentProgram response;
            IEnumerable<Domain.Student.Entities.StudentAcademicProgram> stuAcadProgs;
            Tuple<IEnumerable<Domain.Student.Entities.StudentAcademicProgram>, int> stuAcadProgsTuple;

            private IEnumerable<Domain.Base.Entities.OtherHonor> allHonors;
            private IEnumerable<Domain.Base.Entities.OtherDegree> allDegrees;
            private IEnumerable<Domain.Base.Entities.OtherCcd> allCcds;
            private IEnumerable<Domain.Base.Entities.OtherMajor> allMajors;
            private IEnumerable<Domain.Base.Entities.OtherMinor> allMinors;
            private IEnumerable<Domain.Base.Entities.OtherSpecial> allSp;
            private string defaultInst = "0000043";

            [TestInitialize]
            public void Initialize()
            {

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentAcademicProgramRepositoryMock = new Mock<IStudentAcademicProgramRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();
                termRepositoryMock = new Mock<ITermRepository>();
                catalogRepositoryMock = new Mock<ICatalogRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                curntUserFactory = new StudentUserSetup.ThirdPartyUserFactory();
                repo = new TestStudentProgramRepository();
                BuildMocksForStudentProgramGet();
                StudentAcademicProgramService = new StudentAcademicProgramService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentAcademicProgramRepositoryMock.Object, termRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, catalogRepositoryMock.Object, personRepositoryMock.Object, referenceDataRepositoryMock.Object, curntUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);

            }

            private void BuildMocksForStudentProgramGet()
            {
                acadProgs = new TestAcademicProgramRepository().GetAsync().Result.ToList();
                allHonors = new TestAcademicCredentialsRepository().GetOtherHonors();
                allDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();
                allCcds = new TestAcademicCredentialsRepository().GetOtherCcds();
                allMajors = new TestAcademicDisciplineRepository().GetOtherMajors();
                allMinors = new TestAcademicDisciplineRepository().GetOtherMinors();
                allSp = new TestAcademicDisciplineRepository().GetOtherSpecials();
                locations = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("0d2f089a-b631-46cf-9884-e9d310eeb683","MC","MASTER CLASS"),
                    new Domain.Base.Entities.Location("171e5d1f-910b-4f1a-a771-5847f554e8ab","SBCD","SIMPLE CLASS")
                };

                catalogs = new List<Domain.Student.Entities.Requirements.Catalog>()
                {
                    new Domain.Student.Entities.Requirements.Catalog("10909901-3d7f-4e6b-89ca-79b164cbd8cc","2012", DateTime.Today),
                    new Domain.Student.Entities.Requirements.Catalog("25fa2969-ffc5-4b1e-aed6-77ab23621b57","2013", DateTime.Today),
                    new Domain.Student.Entities.Requirements.Catalog("2c892ac9-b118-4c81-af6e-f30ea7e5a608","2014", DateTime.Today)
                };

                depts = new List<Domain.Base.Entities.Department>()
                {
                    new Domain.Base.Entities.Department("dddf089a-b631-46cf-9884-e9d310eeb683","MATH","MATH DEPARTMENT", true),
                    new Domain.Base.Entities.Department("ddd5d1f-910b-4f1a-a771-5847f554e8ab","ART","ART DEPARTMENT", true),
                    new Domain.Base.Entities.Department("ddd5d1f-910b-4f1a-a771-5847f554e8ab","COMP","COMP SCIENCE DEPARTMENT", true)
                };
                acadLevels = new List<Domain.Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("aaaf089a-b631-46cf-9884-e9d310eeb683","UG","Under Graduate"),
                    new Domain.Student.Entities.AcademicLevel("aaae5d1f-910b-4f1a-a771-5847f554e8ab","GR","Graduate")
                };
                acadCredentails = new List<AcadCredential>()
                {
                    new AcadCredential(allDegrees.FirstOrDefault().Guid,allDegrees.FirstOrDefault().Code,allDegrees.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Degree),
                    new AcadCredential(allDegrees.ToList()[1].Guid,allDegrees.ToList()[1].Code,allDegrees.ToList()[1].Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Degree),
                    new AcadCredential(allCcds.FirstOrDefault().Guid,allCcds.FirstOrDefault().Code,allCcds.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Certificate),
                    new AcadCredential(allHonors.FirstOrDefault().Guid,allHonors.FirstOrDefault().Code,allHonors.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Honorary),
                    new AcadCredential ("diploma123456","DIP","DIPLOMA",Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Diploma)
                };

                terms = new TestTermRepository().Get().ToList();
                academicPeriodCollection = new TestAcademicPeriodRepository().Get().ToList();
                termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<bool>())).ReturnsAsync(terms);
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(terms)).Returns(academicPeriodCollection);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(allHonors);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreesAsync(It.IsAny<bool>())).ReturnsAsync(allDegrees);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsAsync(It.IsAny<bool>())).ReturnsAsync(allMajors);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMinorsAsync(It.IsAny<bool>())).ReturnsAsync(allMinors);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherSpecialsAsync(It.IsAny<bool>())).ReturnsAsync(allSp);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcdsAsync(It.IsAny<bool>())).ReturnsAsync(allCcds);
                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("12345678");
                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("12345678");
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(acadProgs);
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(false)).ReturnsAsync(acadProgs);
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(acadLevels);
                referenceDataRepositoryMock.Setup(loc => loc.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);
                referenceDataRepositoryMock.Setup(dept => dept.GetDepartments2Async(It.IsAny<bool>())).ReturnsAsync(depts);
                referenceDataRepositoryMock.Setup(repo => repo.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(acadCredentails);
                catalogRepositoryMock.Setup(cat => cat.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
                catalogRepositoryMock.Setup(cat => cat.GetAsync()).ReturnsAsync(catalogs);
                var defaultsConfig = new DefaultsConfiguration { HostInstitutionCodeId = "0000043" };
                configurationRepositoryMock.Setup(conf => conf.GetDefaultsConfiguration()).Returns(defaultsConfig);
                statusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();
                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(false)).ReturnsAsync(statusItems);
                stuAcadProgs = new TestStudentAcademicProgramRepository().GetStudentAcademicProgramsAsync(It.IsAny<bool>()).Result.ToList();
                stuAcadProgsTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(stuAcadProgs, 3);
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAcademicProgramService = null;
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
                var expected = stuAcadProgs.FirstOrDefault();
                expected.EndDate = DateTime.Parse("01/06/2018");
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid, defaultInst)).ReturnsAsync(expected);

                //Act
                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync(guid);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894jck");
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                foreach (var dis in result.Disciplines)
                {
                    var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (major != null)
                        Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                    var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (minor != null)
                        Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                    var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (sps != null)
                        Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                }

                foreach (var cred in result.Credentials)
                {
                    var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                    if (degree != null)
                        Assert.AreEqual(degree.Code, expected.DegreeCode);
                    var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                    if (ccds != null)
                        Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                }

            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_MajorNull_DiscilinesMinor()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
                var expected = stuAcadProgs.FirstOrDefault();
                expected.StudentProgramMajors.RemoveAt(0);
                expected.EndDate = DateTime.Parse("01/06/2018");
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid, defaultInst)).ReturnsAsync(expected);

                //Act
                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync(guid);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894jck");
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                foreach (var dis in result.Disciplines)
                {
                    var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    Assert.IsNull(major);
                    var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (minor != null)
                        Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                    var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (sps != null)
                        Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                }

                foreach (var cred in result.Credentials)
                {
                    var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                    if (degree != null)
                        Assert.AreEqual(degree.Code, expected.DegreeCode);
                    var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                    if (ccds != null)
                        Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                }

            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_MajorMinorNull_specializations()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
                var expected = stuAcadProgs.FirstOrDefault();
                expected.StudentProgramMajors.RemoveAt(0);
                expected.StudentProgramMinors.RemoveAt(0);
                expected.EndDate = DateTime.Parse("01/06/2018");
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid, defaultInst)).ReturnsAsync(expected);

                //Act
                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync(guid);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894jck");
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                foreach (var dis in result.Disciplines)
                {
                    var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    Assert.IsNull(major);

                    var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    Assert.IsNull(minor);

                    var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (sps != null)
                        Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                }

                foreach (var cred in result.Credentials)
                {
                    var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                    if (degree != null)
                        Assert.AreEqual(degree.Code, expected.DegreeCode);
                    var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                    if (ccds != null)
                        Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                }

            }

            //no default institution set
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_NoDefaultInstitution()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
                var expected = stuAcadProgs.FirstOrDefault();
                var defaultsConfig = new DefaultsConfiguration { HostInstitutionCodeId = null };
                configurationRepositoryMock.Setup(conf => conf.GetDefaultsConfiguration()).Returns(defaultsConfig);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(guid, defaultInst)).ReturnsAsync(stuAcadProgs.ToList()[0]);
                //Act
                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync(guid);
            }

            //bad guid
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_KeyNotFoundException_NullEntity()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), defaultInst)).ReturnsAsync(null);
                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync("abcd");

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_KeyNotFoundException_ConverttoDTO()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), defaultInst)).ReturnsAsync(stuAcadProgs.ToList()[0]);

                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync("abcd");

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_ArgumentNullException_NoGUID()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), defaultInst)).ReturnsAsync(stuAcadProgs.ToList()[0]);

                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync(null);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_PermissionsException()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), defaultInst)).ReturnsAsync(null);

                var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync("abcd");

            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_WrongStudentId()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "BadStudentId", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "ART", "MAC", "01/01/2001", "12344", "72b7737b-27db-4a06-944b-97d00c29b3ca")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_WrongCredentialsId()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", 
                    "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "", "", "", "", "72b7737b-27db-4a06-944b-97d00c29b3ca")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_Honorary_Credentials()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a",
                    "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "ART", "MAC", "01/01/2001", "12344", "9ae3a175-1dfd-4937-b97b-3c9ad596e023")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_Diploma_Credentials()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a",
                    "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "ART", "MAC", "01/01/2001", "12344", "diploma123456")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_Diploma_GraduatedOn_Exception()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a",
                    "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "ART", "MAC", "01/01/2001", "12344", "diploma123456")).Item1;
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_Diploma()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "dddf089a-b631-46cf-9884-e9d310eeb683", "0d2f089a-b631-46cf-9884-e9d310eeb683", "aaaf089a-b631-46cf-9884-e9d310eeb683", "dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "diploma123456")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_Honors()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "dddf089a-b631-46cf-9884-e9d310eeb683", "0d2f089a-b631-46cf-9884-e9d310eeb683", "aaaf089a-b631-46cf-9884-e9d310eeb683", "dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "9ae3a175-1dfd-4937-b97b-3c9ad596e023")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_CodeList_Empty()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(null);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "dddf089a-b631-46cf-9884-e9d310eeb683", "0d2f089a-b631-46cf-9884-e9d310eeb683", "aaaf089a-b631-46cf-9884-e9d310eeb683", "dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "9ae3a175-1dfd-4937-b97b-3c9ad596e023")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_BadGuid()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "1234567", "01/06/2016", "01/06/2017", "BadGuid", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "dddf089a-b631-46cf-9884-e9d310eeb683", "0d2f089a-b631-46cf-9884-e9d310eeb683", "aaaf089a-b631-46cf-9884-e9d310eeb683", "dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "9ae3a175-1dfd-4937-b97b-3c9ad596e023")).Item1;
                Assert.IsNotNull(acadProgEnroll);
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);

                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>())).Item1;

                //Assert
                for (var i = 0; i < stuAcadProgs.Count(); i++)
                {
                    var expected = stuAcadProgs.ToList()[i];
                    var result = acadProgEnroll.ToList()[i];
                    Assert.AreEqual(result.Id, expected.Guid);
                    var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
                    Assert.AreEqual(resProg.Code, expected.ProgramCode);
                    var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
                    Assert.AreEqual(resCata.Code, expected.CatalogCode);
                    var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
                    Assert.AreEqual(stat.Code, expected.Status);
                    var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
                    Assert.AreEqual(sites.Code, expected.Location);
                    var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
                    Assert.AreEqual(term.Code, expected.StartTerm);
                    Assert.AreEqual(result.StartDate, expected.StartDate);
                    if (result.EndDate.HasValue)
                    {
                        Assert.AreEqual(result.EndDate.Value.Date, expected.EndDate.Value.Date);
                    }
                    Assert.AreEqual(result.Student.Id, expected.StudentId);
                    Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                    if (result.CredentialsDate.HasValue)
                    {
                        Assert.AreEqual(result.CredentialsDate.Value.Date, expected.CredentialsDate.Value.Date);
                    }
                    Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                    if (result.GraduatedOn.HasValue)
                    {
                        Assert.AreEqual(result.GraduatedOn.Value.Date, expected.GraduationDate.Value.Date);
                    }
                    if (expected.GradGPA > 0)
                        Assert.AreEqual(result.PerformanceMeasure, string.Format("{0:N1}", expected.GradGPA));
                    Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                    if (result.Recognitions != null)
                    {
                        foreach (var hnr in result.Recognitions)
                        {
                            var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                            if (honor != null)
                                Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                        }
                    }
                    if (!string.IsNullOrEmpty(expected.ThesisTitle))
                        Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                    else
                        Assert.AreEqual(result.ThesisTitle, null);
                    if (result.Disciplines != null)
                    {
                        foreach (var dis in result.Disciplines)
                        {
                            var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (major != null)
                                Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                            var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (minor != null)
                                Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                            var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (sps != null)
                                Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                        }
                    }
                    if (result.Credentials != null)
                    {
                        foreach (var cred in result.Credentials)
                        {
                            var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                            if (degree != null)
                                Assert.AreEqual(degree.Code, expected.DegreeCode);
                            var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                            if (ccds != null)
                                Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                        }
                    }
                }

            }

            //no academic programs tuple returned
            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_NoTuple()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);

                var result = await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>());
                Assert.AreEqual(result.Item2, 0);

            }

            //returned a null tuple. 
            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_TupleWithNoEntity()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var stuProgTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(null, 0);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuProgTuple);
                var result = await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>());
                Assert.AreEqual(result.Item2, 0);

            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "dddf089a-b631-46cf-9884-e9d310eeb683", "0d2f089a-b631-46cf-9884-e9d310eeb683", "aaaf089a-b631-46cf-9884-e9d310eeb683", "dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "72b7737b-27db-4a06-944b-97d00c29b3db")).Item1;

                //Assert
                for (var i = 0; i < stuAcadProgs.Count(); i++)
                {
                    var expected = stuAcadProgs.ToList()[i];
                    var result = acadProgEnroll.ToList()[i];
                    Assert.AreEqual(result.Id, expected.Guid);
                    var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
                    Assert.AreEqual(resProg.Code, expected.ProgramCode);
                    var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
                    Assert.AreEqual(resCata.Code, expected.CatalogCode);
                    var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
                    Assert.AreEqual(stat.Code, expected.Status);
                    var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
                    Assert.AreEqual(sites.Code, expected.Location);
                    var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
                    Assert.AreEqual(term.Code, expected.StartTerm);
                    Assert.AreEqual(result.StartDate, expected.StartDate);
                    Assert.AreEqual(result.EndDate, expected.EndDate);
                    Assert.AreEqual(result.Student.Id, expected.StudentId);
                    Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                    Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                    Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                    Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                    if (expected.GradGPA > 0)
                        Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                    Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                    if (result.Recognitions != null)
                    {
                        foreach (var hnr in result.Recognitions)
                        {
                            var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                            if (honor != null)
                                Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                        }
                    }
                    if (!string.IsNullOrEmpty(expected.ThesisTitle))
                        Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                    else
                        Assert.AreEqual(result.ThesisTitle, null);
                    if (result.Disciplines != null)
                    {
                        foreach (var dis in result.Disciplines)
                        {
                            var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (major != null)
                                Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                            var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (minor != null)
                                Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                            var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (sps != null)
                                Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                        }
                    }
                    if (result.Credentials != null)
                    {
                        foreach (var cred in result.Credentials)
                        {
                            var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                            if (degree != null)
                                Assert.AreEqual(degree.Code, expected.DegreeCode);
                            var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                            if (ccds != null)
                                Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                        }
                    }
                }
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters_Degree()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("01/06/2016");
                //Act
                var acadProgEnroll = (await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(0, 1, true, "12345678", "01/06/2016", "01/06/2017", "1c5bbbbc-80e3-4042-8151-db9893ac337a", "10909901-3d7f-4e6b-89ca-79b164cbd8cc", "active", "dddf089a-b631-46cf-9884-e9d310eeb683", "0d2f089a-b631-46cf-9884-e9d310eeb683", "aaaf089a-b631-46cf-9884-e9d310eeb683", "dd0c42ca-c61d-4ca6-8d21-96ab5be35623", "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4")).Item1;

                //Assert
                //Assert
                for (var i = 0; i < stuAcadProgs.Count(); i++)
                {
                    var expected = stuAcadProgs.ToList()[i];
                    var result = acadProgEnroll.ToList()[i];
                    Assert.AreEqual(result.Id, expected.Guid);
                    var resProg = acadProgs.FirstOrDefault(p => p.Guid == result.Program.Id);
                    Assert.AreEqual(resProg.Code, expected.ProgramCode);
                    var resCata = catalogs.FirstOrDefault(p => p.Guid == result.Catalog.Id);
                    Assert.AreEqual(resCata.Code, expected.CatalogCode);
                    var stat = statusItems.FirstOrDefault(p => p.Guid == result.EnrollmentStatus.Detail.Id);
                    Assert.AreEqual(stat.Code, expected.Status);
                    var sites = locations.FirstOrDefault(p => p.Guid == result.Site.Id);
                    Assert.AreEqual(sites.Code, expected.Location);
                    var term = academicPeriodCollection.FirstOrDefault(p => p.Guid == result.StartTerm.Id);
                    Assert.AreEqual(term.Code, expected.StartTerm);
                    Assert.AreEqual(result.StartDate, expected.StartDate);
                    Assert.AreEqual(result.EndDate, expected.EndDate);
                    Assert.AreEqual(result.Student.Id, expected.StudentId);
                    Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                    Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                    Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                    Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                    if (expected.GradGPA > 0)
                        Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                    Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                    if (result.Recognitions != null)
                    {
                        foreach (var hnr in result.Recognitions)
                        {
                            var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                            if (honor != null)
                                Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                        }
                    }
                    if (!string.IsNullOrEmpty(expected.ThesisTitle))
                        Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                    else
                        Assert.AreEqual(result.ThesisTitle, null);
                    if (result.Disciplines != null)
                    {
                        foreach (var dis in result.Disciplines)
                        {
                            var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (major != null)
                                Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                            var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (minor != null)
                                Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                            var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                            if (sps != null)
                                Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                        }
                    }
                    if (result.Credentials != null)
                    {
                        foreach (var cred in result.Credentials)
                        {
                            var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                            if (degree != null)
                                Assert.AreEqual(degree.Code, expected.DegreeCode);
                            var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                            if (ccds != null)
                                Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                        }
                    }
                }

            }
        }

        #endregion

        #region Post/PutTests
        [TestClass]
        public class StudentAcademicProgramServiceTests_Post : StudentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IStudentAcademicProgramRepository> studentAcademicProgramRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<ICatalogRepository> catalogRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            ICurrentUserFactory curntUserFactory;
            private StudentAcademicProgramService StudentAcademicProgramService;

            StudentAcademicProgram StuAcadProgEntity;
            TestStudentProgramRepository repo;
            List<Domain.Student.Entities.AcademicProgram> acadProgs;
            List<Domain.Student.Entities.Requirements.Catalog> catalogs;
            List<Domain.Base.Entities.Location> locations;
            List<Domain.Base.Entities.Department> depts;
            List<Domain.Student.Entities.AcademicLevel> acadLevels;
            List<Domain.Student.Entities.Term> terms;
            ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection;
            List<Domain.Student.Entities.EnrollmentStatus> statusItems;
            //StudentProgram response;
            List<Domain.Student.Entities.StudentAcademicProgram> stuAcadProgs;
            Tuple<IEnumerable<Domain.Student.Entities.StudentAcademicProgram>, int> stuAcadProgsTuple;

            private IEnumerable<Domain.Base.Entities.OtherHonor> allHonors;
            private IEnumerable<Domain.Base.Entities.OtherDegree> allDegrees;
            private IEnumerable<Domain.Base.Entities.OtherCcd> allCcds;
            private IEnumerable<Domain.Base.Entities.OtherMajor> allMajors;
            private IEnumerable<Domain.Base.Entities.OtherMinor> allMinors;
            private IEnumerable<Domain.Base.Entities.OtherSpecial> allSp;
            private IEnumerable<AcadCredential> acadCredentails;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.AcademicDiscipline> acadDisciplines;
            private string defaultInst = "0000043";
            private List<Dtos.StudentAcademicPrograms> StuAcadProgDtos;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentAcademicProgramRepositoryMock = new Mock<IStudentAcademicProgramRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();
                termRepositoryMock = new Mock<ITermRepository>();
                catalogRepositoryMock = new Mock<ICatalogRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                curntUserFactory = new StudentUserSetup.ThirdPartyUserFactory();
                repo = new TestStudentProgramRepository();
                BuildMocksForStudentProgramGet();
                StudentAcademicProgramService = new StudentAcademicProgramService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentAcademicProgramRepositoryMock.Object, termRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, catalogRepositoryMock.Object, personRepositoryMock.Object, referenceDataRepositoryMock.Object, curntUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
                StuAcadProgDtos = BuildStudentAcademicProgramsDtos();
            }

            private void BuildMocksForStudentProgramGet()
            {
                acadProgs = new TestAcademicProgramRepository().GetAsync().Result.ToList();
                allHonors = new TestAcademicCredentialsRepository().GetOtherHonors();
                allDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();
                allCcds = new TestAcademicCredentialsRepository().GetOtherCcds();
                allMajors = new TestAcademicDisciplineRepository().GetOtherMajors();
                allMinors = new TestAcademicDisciplineRepository().GetOtherMinors();
                allSp = new TestAcademicDisciplineRepository().GetOtherSpecials();
                locations = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("0d2f089a-b631-46cf-9884-e9d310eeb683","MC","MASTER CLASS"),
                    new Domain.Base.Entities.Location("171e5d1f-910b-4f1a-a771-5847f554e8ab","SBCD","SIMPLE CLASS")
                };

                catalogs = new List<Domain.Student.Entities.Requirements.Catalog>()
                {
                    new Domain.Student.Entities.Requirements.Catalog("10909901-3d7f-4e6b-89ca-79b164cbd8cc","2012", DateTime.Today),
                    new Domain.Student.Entities.Requirements.Catalog("25fa2969-ffc5-4b1e-aed6-77ab23621b57","2013", DateTime.Today),
                    new Domain.Student.Entities.Requirements.Catalog("2c892ac9-b118-4c81-af6e-f30ea7e5a608","2014", DateTime.Today)
                };

                depts = new List<Domain.Base.Entities.Department>()
                {
                    new Domain.Base.Entities.Department("dddf089a-b631-46cf-9884-e9d310eeb683","MATH","MATH DEPARTMENT", true),
                    new Domain.Base.Entities.Department("ddd5d1f-910b-4f1a-a771-5847f554e8ab","ART","ART DEPARTMENT", true),
                    new Domain.Base.Entities.Department("ddd5d1f-910b-4f1a-a771-5847f554e8ac","COMP","COMP SCIENCE DEPARTMENT", true)
                };
                acadLevels = new List<Domain.Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("aaaf089a-b631-46cf-9884-e9d310eeb683","UG","Under Graduate"),
                    new Domain.Student.Entities.AcademicLevel("aaae5d1f-910b-4f1a-a771-5847f554e8ab","GR","Graduate")
                };
                acadCredentails = new List<AcadCredential>()
                {
                    new AcadCredential(allDegrees.FirstOrDefault().Guid,allDegrees.FirstOrDefault().Code,allDegrees.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Degree),
                    new AcadCredential(allDegrees.ToList()[1].Guid,allDegrees.ToList()[1].Code,allDegrees.ToList()[1].Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Degree),
                    new AcadCredential(allCcds.FirstOrDefault().Guid,allCcds.FirstOrDefault().Code,allCcds.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Certificate),
                    new AcadCredential(allHonors.FirstOrDefault().Guid,allHonors.FirstOrDefault().Code,allHonors.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Honorary),
                    new AcadCredential ("diploma123456","DIP","DIPLOMA",Ellucian.Colleague.Domain.Base.Entities.AcademicCredentialType.Diploma)

                };
                acadDisciplines = new List<Ellucian.Colleague.Domain.Base.Entities.AcademicDiscipline>()
                {
                    new Ellucian.Colleague.Domain.Base.Entities.AcademicDiscipline(allMajors.FirstOrDefault().Guid,allMajors.FirstOrDefault().Code,allMajors.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Ellucian.Colleague.Domain.Base.Entities.AcademicDiscipline(allMinors.FirstOrDefault().Guid,allMinors.FirstOrDefault().Code,allMinors.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicDisciplineType.Minor),
                    new Ellucian.Colleague.Domain.Base.Entities.AcademicDiscipline(allSp.FirstOrDefault().Guid,allSp.FirstOrDefault().Code,allSp.FirstOrDefault().Description,Ellucian.Colleague.Domain.Base.Entities.AcademicDisciplineType.Concentration),

                };
                terms = new TestTermRepository().Get().ToList();
                academicPeriodCollection = new TestAcademicPeriodRepository().Get().ToList();
                termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<bool>())).ReturnsAsync(terms);
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(terms)).Returns(academicPeriodCollection);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(allHonors);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreesAsync(It.IsAny<bool>())).ReturnsAsync(allDegrees);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsAsync(It.IsAny<bool>())).ReturnsAsync(allMajors);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMinorsAsync(It.IsAny<bool>())).ReturnsAsync(allMinors);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherSpecialsAsync(It.IsAny<bool>())).ReturnsAsync(allSp);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcdsAsync(It.IsAny<bool>())).ReturnsAsync(allCcds);
                referenceDataRepositoryMock.Setup(repo => repo.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(acadCredentails);
                referenceDataRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(acadDisciplines);
                personRepositoryMock.Setup(pr => pr.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("12345678");
                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("12345678");
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(acadProgs);
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(false)).ReturnsAsync(acadProgs);
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(acadLevels);
                referenceDataRepositoryMock.Setup(loc => loc.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);
                referenceDataRepositoryMock.Setup(dept => dept.GetDepartments2Async(It.IsAny<bool>())).ReturnsAsync(depts);
                catalogRepositoryMock.Setup(cat => cat.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
                catalogRepositoryMock.Setup(cat => cat.GetAsync()).ReturnsAsync(catalogs);
                var defaultsConfig = new DefaultsConfiguration { HostInstitutionCodeId = "0000043" };
                configurationRepositoryMock.Setup(conf => conf.GetDefaultsConfiguration()).Returns(defaultsConfig);
                statusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();
                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(statusItems);
                studentReferenceDataRepositoryMock.Setup(en => en.GetEnrollmentStatusesAsync(false)).ReturnsAsync(statusItems);
                stuAcadProgs = new TestStudentAcademicProgramRepository().GetStudentAcademicProgramsAsync(It.IsAny<bool>()).Result.ToList();
                stuAcadProgsTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(stuAcadProgs, 3);
            }

            private static List<StudentAcademicPrograms> BuildStudentAcademicProgramsDtos()
            {
                var StudentAcademicProgramDtos = new List<Dtos.StudentAcademicPrograms>();
                var acadProgEnrollDto1 = new Dtos.StudentAcademicPrograms()
                {
                    Id = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9",
                    Program = new Dtos.GuidObject2() { Id = "1c5bbbbc-80e3-4042-8151-db9893ac337a" },
                    Catalog = new Dtos.GuidObject2() { Id = "10909901-3d7f-4e6b-89ca-79b164cbd8cc" },
                    Student = new Dtos.GuidObject2() { Id = "12345678" },
                    Site = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
                    StartTerm = new Dtos.GuidObject2() { Id = "d1ef94c1-759c-4870-a3f4-34065bb522fe" },
                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
                    AcademicLevel = new GuidObject2() { Id = "aaaf089a-b631-46cf-9884-e9d310eeb683" },
                    CredentialsDate = new DateTimeOffset(DateTime.Parse("01/06/2017")),
                    CreditsEarned = 100m,
                    EndDate = new DateTimeOffset(DateTime.Parse("01/06/2018")),
                    GraduatedOn = new DateTimeOffset(DateTime.Parse("01/06/2018")),
                    PerformanceMeasure = "4.0",
                    ProgramOwner = new GuidObject2() { Id = "dddf089a-b631-46cf-9884-e9d310eeb683" },
                    Recognitions = new List<GuidObject2>() { new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } },
                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" }, new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } },
                    ThesisTitle = "this is aa good thesis",
                    Disciplines = new List<Dtos.StudentAcademicProgramDisciplines>() { new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "dddf089a-b631-46cf-9884-e9d310eeb683" } }, new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active, Detail = new GuidObject2() { Id = "3cf900894jck" } }

                };

                var acadProgEnrollDto2 = new Dtos.StudentAcademicPrograms()
                {
                    Id = "45d8557f-56a9-4abc-8308-ee026983080c",
                    Program = new Dtos.GuidObject2() { Id = "17a21cdc-7912-459e-a065-03895471a644" },
                    Catalog = new Dtos.GuidObject2() { Id = "25fa2969-ffc5-4b1e-aed6-77ab23621b57" },
                    Student = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
                    Site = new Dtos.GuidObject2() { Id = "0d2f089a-b631-46cf-9884-e9d310eeb683" },
                    StartTerm = new Dtos.GuidObject2() { Id = "d1ef94c1-759c-4870-a3f4-34065bb522fe" },
                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
                    EndDate = new DateTimeOffset(DateTime.Parse("01/06/2017")),
                    Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } },
                    //Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } }, new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } },
                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Inactive, Detail = new GuidObject2() { Id = "3cf900894alk" } }

                };
                //just required data
                var acadProgEnrollDto3 = new Dtos.StudentAcademicPrograms()
                {
                    Id = "688583fc-6499-4a05-90b0-685745d6b465",
                    Program = new Dtos.GuidObject2() { Id = "fbdfafd6-69a1-4362-88a0-62eac70da5c9" },
                    Catalog = new Dtos.GuidObject2() { Id = "2c892ac9-b118-4c81-af6e-f30ea7e5a608" },
                    Student = new Dtos.GuidObject2() { Id = "171e5d1f-910b-4f1a-a771-5847f554e8ab" },
                    StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016")),
                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active }

                };
                StudentAcademicProgramDtos.Add(acadProgEnrollDto1);
                StudentAcademicProgramDtos.Add(acadProgEnrollDto2);
                StudentAcademicProgramDtos.Add(acadProgEnrollDto3);
                return StudentAcademicProgramDtos;
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAcademicProgramService = null;
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_Active_WithAll()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs.FirstOrDefault();
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];

                //Act
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894jck");
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                foreach (var dis in result.Disciplines)
                {
                    var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (major != null)
                        Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                    var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (minor != null)
                        Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                    var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (sps != null)
                        Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                }

                foreach (var cred in result.Credentials)
                {
                    var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                    if (degree != null)
                        Assert.AreEqual(degree.Code, expected.DegreeCode);
                    var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                    if (ccds != null)
                        Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                }

            }

            [TestMethod]
            public async Task StudentAcademicProgramService_UpdateStudentAcademicProgramAsync_Active_WithAll()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs.FirstOrDefault();
                studentAcademicProgramRepositoryMock.Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];

                //Act
                var result = await StudentAcademicProgramService.UpdateStudentAcademicProgramAsync(dto);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894jck");
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                foreach (var dis in result.Disciplines)
                {
                    var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (major != null)
                        Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                    var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (minor != null)
                        Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                    var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                    if (sps != null)
                        Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                }

                foreach (var cred in result.Credentials)
                {
                    var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                    if (degree != null)
                        Assert.AreEqual(degree.Code, expected.DegreeCode);
                    var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                    if (ccds != null)
                        Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                }
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_Inactive_()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs[1];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                dto.EnrollmentStatus.Detail = null;
                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2099"));

                //Act
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Inactive);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894alk");
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                if (result.Disciplines != null)
                {
                    foreach (var dis in result.Disciplines)
                    {
                        var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (major != null)
                            Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                        var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (minor != null)
                            Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                        var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (sps != null)
                            Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                    }
                }

                if (result.Credentials != null)
                {
                    foreach (var cred in result.Credentials)
                    {
                        var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                        if (degree != null)
                            Assert.AreEqual(degree.Code, expected.DegreeCode);
                        var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                        if (ccds != null)
                            Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                    }
                }
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_EnrollStatus_Complete()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs[2];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                dto.EnrollmentStatus.Detail = null;
                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2099"));

                //Act
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Complete);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894kkj");
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, null);
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, null);
                if (result.Disciplines != null)
                {
                    foreach (var dis in result.Disciplines)
                    {
                        var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (major != null)
                            Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                        var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (minor != null)
                            Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                        var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (sps != null)
                            Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                    }
                }

                if (result.Credentials != null)
                {
                    foreach (var cred in result.Credentials)
                    {
                        var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                        if (degree != null)
                            Assert.AreEqual(degree.Code, expected.DegreeCode);
                        var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                        if (ccds != null)
                            Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                    }
                }
            }

            [TestMethod]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_RequiredDataDTO()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[2];
                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                dto.EnrollmentStatus.Detail = null;
                //Act
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

                //Assert
                Assert.AreEqual(result.Id, expected.Guid);
                Assert.AreEqual(result.Program.Id, (acadProgs.FirstOrDefault(ap => ap.Code == expected.ProgramCode)).Guid);
                Assert.AreEqual(result.Catalog.Id, (catalogs.FirstOrDefault(ap => ap.Code == expected.CatalogCode)).Guid);
                Assert.AreEqual(result.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active);
                Assert.AreEqual(result.EnrollmentStatus.Detail.Id, "3cf900894jck");
                Assert.AreEqual(result.StartDate, expected.StartDate);
                Assert.AreEqual(result.Site.Id, (locations.FirstOrDefault(ap => ap.Code == expected.Location)).Guid);
                Assert.AreEqual(result.StartTerm.Id, (academicPeriodCollection.FirstOrDefault(ap => ap.Code == expected.StartTerm)).Guid);
                Assert.AreEqual(result.EndDate, expected.EndDate);
                Assert.AreEqual(result.Student.Id, expected.StudentId);
                Assert.AreEqual(result.AcademicLevel.Id, (acadLevels.FirstOrDefault(ap => ap.Code == expected.AcademicLevelCode)).Guid);
                Assert.AreEqual(result.CredentialsDate, expected.CredentialsDate);
                Assert.AreEqual(result.CreditsEarned, expected.CreditsEarned);
                Assert.AreEqual(result.GraduatedOn, expected.GraduationDate);
                Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (expected.GradGPA > 0)
                    Assert.AreEqual(result.PerformanceMeasure, expected.GradGPA.ToString());
                Assert.AreEqual(result.ProgramOwner.Id, (depts.FirstOrDefault(de => de.Code == expected.DepartmentCode)).Guid);
                if (result.Recognitions != null)
                {
                    foreach (var hnr in result.Recognitions)
                    {
                        var honor = allHonors.FirstOrDefault(deg => deg.Guid == hnr.Id);
                        if (honor != null)
                            Assert.AreEqual(honor.Code, expected.StudentProgramHonors.FirstOrDefault());

                    }
                }
                Assert.AreEqual(result.ThesisTitle, expected.ThesisTitle);
                if (result.Disciplines != null)
                {
                    foreach (var dis in result.Disciplines)
                    {
                        var major = allMajors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (major != null)
                            Assert.AreEqual(major.Code, expected.StudentProgramMajors.FirstOrDefault());
                        var minor = allMinors.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (minor != null)
                            Assert.AreEqual(minor.Code, expected.StudentProgramMinors.FirstOrDefault());
                        var sps = allSp.FirstOrDefault(deg => deg.Guid == dis.Discipline.Id);
                        if (sps != null)
                            Assert.AreEqual(sps.Code, expected.StudentProgramSpecializations.FirstOrDefault());
                    }
                }

                if (result.Credentials != null)
                {
                    foreach (var cred in result.Credentials)
                    {
                        var degree = allDegrees.FirstOrDefault(deg => deg.Guid == cred.Id);
                        if (degree != null)
                            Assert.AreEqual(degree.Code, expected.DegreeCode);
                        var ccds = allCcds.FirstOrDefault(ccd => ccd.Guid == cred.Id);
                        if (ccds != null)
                            Assert.AreEqual(ccds.Code, expected.StudentProgramCcds.FirstOrDefault());
                    }
                }
            }

            ////this is caught in 
            //[TestMethod]
            //[ExpectedException(typeof(ArgumentNullException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentNullException_NoID()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.Id = null;
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}


            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadProgram()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.Program.Id = "abcd";
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NoStudent()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.Student = null;
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NoStartDate()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.StartDate = default(DateTime);
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NoStatus()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.EnrollmentStatus.EnrollStatus = null;
            //    dto.EnrollmentStatus.Detail = null;
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_EndDateBeforeStart()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.StartDate = new DateTimeOffset(DateTime.Parse("01/06/2016"));
            //    dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2015"));
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_InactiveNoEndDate()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
            //    dto.EndDate = null;
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_CompleteNoEndDate()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
            //    dto.EndDate = null;
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}
            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_ActiveEndDate()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
            //    var dto = acadProgEnrollDtos[0];
            //    dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
            //    dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2018"));
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullCredentials()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
            //    var dto = acadProgEnrollDtos[0];
            //    dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = null } };
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullDiscipline()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentProgram>())).ReturnsAsync(expected);
            //    var dto = acadProgEnrollDtos[0];
            //    dto.Disciplines = new List<AcademicProgramDiscipline>() { new AcademicProgramDiscipline() { Discipline = new GuidObject2() { Id = null } } };
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadPersonID()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadProgram()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var progs = new List<Domain.Student.Entities.AcademicProgram>()
                {
                    new Domain.Student.Entities.AcademicProgram("0d2f089a-b631-46cf-9884-e9d310eeb00","MC","MASTER CLASS")
                };
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(progs);
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullCatalog()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected); var dto = StuAcadProgDtos[0];
                dto.Catalog.Id = null;
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadCatalog()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var cata = new List<Domain.Student.Entities.Requirements.Catalog>() { };
                catalogRepositoryMock.Setup(cat => cat.GetAsync(It.IsAny<bool>())).ReturnsAsync(cata);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullLocation()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected); var dto = StuAcadProgDtos[0];
                dto.Site.Id = null;
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullProgramOwner()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected); var dto = StuAcadProgDtos[0];
                dto.ProgramOwner.Id = null;
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullAcadLevel()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected); var dto = StuAcadProgDtos[0];
                dto.AcademicLevel.Id = null;
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullStartTerm()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.StartTerm.Id = null;
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadLocation()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var sites = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("0d2f089a-b631-46cf-9884-e9d310eeb00","MC","MASTER CLASS")
                };
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(loc => loc.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(sites);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadProgramOwner()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var depts = new List<Domain.Base.Entities.Department>()
                {
                    new Domain.Base.Entities.Department("dddf089a-b631-46cf-9884-e9d310eeb633","MATH","MATH DEPARTMENT", true)
                };
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(dept => dept.GetDepartments2Async(It.IsAny<bool>())).ReturnsAsync(depts);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadAcademicLevel()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var acadLevel = new List<Domain.Student.Entities.AcademicLevel>()
                {
                    new Domain.Student.Entities.AcademicLevel("aaaf089a-b631-46cf-9884-e9d310eeb633","UG","Under Graduate")
                };
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(acadLevel);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadStartTerm()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs[0];
                var period = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("0d2f089a-b631-46cf-9884-e9d310eeb00","2002","fall",DateTime.Today,DateTime.Today,200,1,"2002","","",null)
                };
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(terms)).Returns(period);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadPerformanceMeasure()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.PerformanceMeasure = "abc";
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullStatusDetail()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected); var dto = StuAcadProgDtos[0];
                dto.EnrollmentStatus.Detail.Id = null;
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_PermissionsException()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);



            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadCredentials()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "13456" } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadRecognitions()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Recognitions = new List<GuidObject2>() { new GuidObject2() { Id = "13456" } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullRecognitions()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Recognitions = new List<GuidObject2>() { new GuidObject2() { Id = null } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }
            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadCredentials_AcadProgram_Degree()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" } };
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadCredentials_AcadProgram_Spec()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4" }, new GuidObject2() { Id = "31d8aa32-dbe6-83j7-a1c4-2cad39e232e4" } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadCredentials_Honorary()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadCredentials_2Degrees()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" }, new GuidObject2() { Id = "31d8aa32-dbe6-3b89-a1c4-2cad39e232e4" } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadCredentials_Diploma()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                var credential = new List<Dtos.AcademicCredential>()
                {
                    new Dtos.AcademicCredential(){Id = "123456", AcademicCredentialType = Dtos.AcademicCredentialType.Diploma}

                };
                //academicCredentialServiceMock.Setup(cred => cred.GetAcademicCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(credential);
                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Credentials = new List<GuidObject2>() { new GuidObject2() { Id = "diploma123456" } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadDisciplines()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Disciplines = new List<Dtos.StudentAcademicProgramDisciplines>() { new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e0" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "dddf089a-b631-46cf-9884-e9d310eeb6" } } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadDisciplines_NullAdministeringUnit()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Disciplines = new List<Dtos.StudentAcademicProgramDisciplines>() { new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" }, AdministeringInstitutionUnit = new GuidObject2() { Id = null } } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadDisciplines_BadAdministeringUnit()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Disciplines = new List<Dtos.StudentAcademicProgramDisciplines>() { new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "123456" } } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadDisciplines_BadAdministeringUnit_Dept()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Disciplines = new List<Dtos.StudentAcademicProgramDisciplines>() { new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "dddf089a-b631-46cf-9884-e9d310eeb683" } }, new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "ddd5d1f-910b-4f1a-a771-5847f554e8ab" } }, new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "72b7737b-27db-4a06-944b-97d00c29b3db" } } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadAdministeringUnit_DeptProg()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.ProgramOwner.Id = "ddd5d1f-910b-4f1a-a771-5847f554e8ab";
                dto.Disciplines = new List<Dtos.StudentAcademicProgramDisciplines>() { new Dtos.StudentAcademicProgramDisciplines() { Discipline = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" }, AdministeringInstitutionUnit = new GuidObject2() { Id = "dddf089a-b631-46cf-9884-e9d310eeb683" } } };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_NullDisciplineId()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.Disciplines = new List<Dtos.StudentAcademicProgramDisciplines>() { new Dtos.StudentAcademicProgramDisciplines() };
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadEnrollStatus()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                dto.EndDate = new DateTimeOffset(DateTime.Parse("01/06/2019"));
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_BadEnrollStatus_Detail()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[0];
                dto.EnrollmentStatus.Detail.Id = "12345";
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }
        }
        #endregion

    }

    [TestClass]
    public class StudentAcademicProgramServiceTests_V11
    {
        [TestClass]
        public class StudentAcademicProgramServiceTests_GET_AND_GETALL : StudentUserSetup
        {
            #region DECLARATION

            protected Domain.Entities.Role getStudentAcademicPrograms = new Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.PROGRAM");

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IStudentAcademicProgramRepository> studentAcademicProgramRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICatalogRepository> catalogRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ICurrentUserFactory currentUserFactory;

            private StudentAcademicProgramService studentAcademicProgramService;

            private DefaultsConfiguration defaultConfiguration;
            private StudentAcademicProgram studentAcademicProgramEntity;
            private List<Domain.Student.Entities.AcademicProgram> academicPrograms;
            private List<Domain.Student.Entities.Requirements.Catalog> catalogs;
            private List<Domain.Base.Entities.Location> locations;
            private List<Department> departments;
            private List<Term> terms;
            private List<Domain.Student.Entities.AcademicPeriod> academicPeriods;
            private List<Domain.Student.Entities.AcademicLevel> academicLevels;
            private List<Domain.Base.Entities.OtherDegree> otherDegrees;
            private List<Domain.Base.Entities.OtherMajor> otherMajors;
            private List<Domain.Base.Entities.OtherHonor> otherHonors;
            private List<Domain.Student.Entities.EnrollmentStatus> enrollmentStatuses;
            private List<Domain.Base.Entities.AcadCredential> academicCredentials;

            private Tuple<IEnumerable<StudentAcademicProgram>, int> studentAcademicPrograms;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            private Dictionary<string, string> personGuidCollection;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentAcademicProgramRepositoryMock = new Mock<IStudentAcademicProgramRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                catalogRepositoryMock = new Mock<ICatalogRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                currentUserFactory = new ThirdPartyUserFactory();

                InitializeTestData();

                InitializeMock();

                studentAcademicProgramService = new StudentAcademicProgramService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentAcademicProgramRepositoryMock.Object,
                    termRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, catalogRepositoryMock.Object, personRepositoryMock.Object, referenceDataRepositoryMock.Object,
                    currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                studentRepositoryMock = null;
                studentAcademicProgramRepositoryMock = null;
                termRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                catalogRepositoryMock = null;
                personRepositoryMock = null;
                referenceDataRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;
            }

            private void InitializeTestData()
            {
                defaultConfiguration = new DefaultsConfiguration() { HostInstitutionCodeId = "1" };
                personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", guid);

                academicPrograms = new List<Domain.Student.Entities.AcademicProgram>() { new Domain.Student.Entities.AcademicProgram(guid, "1", "desc") };

                catalogs = new List<Domain.Student.Entities.Requirements.Catalog>() { new Domain.Student.Entities.Requirements.Catalog(guid, "1", "desc", DateTime.Today) };

                locations = new List<Domain.Base.Entities.Location>() { new Domain.Base.Entities.Location(guid, "1", "desc") };

                departments = new List<Department>() { new Department(guid, "1", "desc", true) };

                terms = new List<Term>() { new Term(guid, "1", "desc", DateTime.Today, DateTime.Today.AddDays(100), 2017, 1, true, true, "1", true) };

                academicPeriods = new List<Domain.Student.Entities.AcademicPeriod>() { new Domain.Student.Entities.AcademicPeriod(guid, "1", "desc", DateTime.Today, DateTime.Today.AddDays(100), 2017,
                    1, "1", "1", "1", new List<RegistrationDate>() { })};

                academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(guid, "1", "desc") };

                otherDegrees = new List<Domain.Base.Entities.OtherDegree>() { new Domain.Base.Entities.OtherDegree(guid, "1", "desc") };

                otherMajors = new List<Domain.Base.Entities.OtherMajor>() { new Domain.Base.Entities.OtherMajor(guid, "1", "desc") };

                otherHonors = new List<Domain.Base.Entities.OtherHonor>() { new Domain.Base.Entities.OtherHonor(guid, "1", "desc") };

                academicCredentials = new List<Domain.Base.Entities.AcadCredential>() { new Domain.Base.Entities.AcadCredential(guid, "1", "desc", Domain.Base.Entities.AcademicCredentialType.Degree) };

                enrollmentStatuses = new List<Domain.Student.Entities.EnrollmentStatus>()
                {
                    new Domain.Student.Entities.EnrollmentStatus(guid, "1", "desc", Domain.Student.Entities.EnrollmentStatusType.active),
                    new Domain.Student.Entities.EnrollmentStatus(guid, "2", "desc", Domain.Student.Entities.EnrollmentStatusType.complete)
                };

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "1")
                {
                    GraduationDate = DateTime.Today,
                    IsPrimary = true,
                    Location = "1",
                    DepartmentCode = "1",
                    StartTerm = "1",
                    AnticipatedCompletionTerm = "1",
                    GradTerm = "1",
                    AcademicLevelCode = "1",
                    DegreeCode = "1",
                    CreditsEarned = 0
                };

                studentAcademicProgramEntity.AddMajors("1");
                studentAcademicProgramEntity.AddHonors("1");

                studentAcademicPrograms = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);
            }

            private void InitializeMock(bool bypassCache = true)
            {
                getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });
                configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultConfiguration);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsAsync(bypassCache)).ReturnsAsync(academicPrograms);
                catalogRepositoryMock.Setup(c => c.GetAsync(bypassCache)).ReturnsAsync(catalogs);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(bypassCache)).ReturnsAsync(locations);
                referenceDataRepositoryMock.Setup(r => r.GetDepartments2Async(bypassCache)).ReturnsAsync(departments);
                referenceDataRepositoryMock.Setup(r => r.GetAcadCredentialsAsync(bypassCache)).ReturnsAsync(academicCredentials);
                termRepositoryMock.Setup(t => t.GetAsync(bypassCache)).ReturnsAsync(terms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsAsync(bypassCache)).ReturnsAsync(academicLevels);
                referenceDataRepositoryMock.Setup(r => r.GetOtherDegreesAsync(bypassCache)).ReturnsAsync(otherDegrees);
                referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsAsync(bypassCache)).ReturnsAsync(otherMajors);
                referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(bypassCache)).ReturnsAsync(otherHonors);
                studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(bypassCache)).ReturnsAsync(enrollmentStatuses);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Today.ToString());

                //studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), bypassCache, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
                //    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                //    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAcademicPrograms);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid2Async_Guid_As_Null()
            {
                await studentAcademicProgramService.GetStudentAcademicProgramByGuid2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid2Async_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await studentAcademicProgramService.GetStudentAcademicProgramByGuid2Async(guid);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid2Async()
            {
                var result = await studentAcademicProgramService.GetStudentAcademicProgramByGuid2Async(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms2Async_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await studentAcademicProgramService.GetStudentAcademicPrograms2Async(0, 100);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms2Async_ArgumentException()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

                var results = await studentAcademicProgramService.GetStudentAcademicPrograms2Async(0, 100, true, Guid.NewGuid().ToString());

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms2Async_Repository_Returns_Null()
            {
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms2Async(0, 100, true, guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms2Async_Repository_Returns_EmptyResult()
            {
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { }, 0));

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms2Async(0, 100, true, guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms2Async_With_Invalid_StatusFilter()
            {
                InitializeMock(false);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms2Async(0, 100, false, status: "activ");
                Assert.IsNotNull(result);
            }

        }

        [TestClass]
        public class StudentAcademicProgramServiceTests_POST_AND_PUT : StudentUserSetup
        {
            #region DECLARATION

            protected Domain.Entities.Role getStudentAcademicPrograms = new Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.PROGRAM");
            protected Domain.Entities.Role createStudentAcademicPrograms = new Domain.Entities.Role(2, "CREATE.UPDATE.STUDENT.ACADEMIC.PROGRAM");

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IStudentAcademicProgramRepository> studentAcademicProgramRepositoryMock;
            private Mock<ITermRepository> termRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ICatalogRepository> catalogRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ICurrentUserFactory currentUserFactory;

            private StudentAcademicProgramService studentAcademicProgramService;

            private DefaultsConfiguration defaultConfiguration;
            private StudentAcademicProgram studentAcademicProgramEntity;
            private List<Domain.Student.Entities.AcademicProgram> academicPrograms;
            private List<Domain.Student.Entities.Requirements.Catalog> catalogs;
            private List<Domain.Base.Entities.Location> locations;
            private List<Department> departments;
            private List<Term> terms;
            private List<Domain.Student.Entities.AcademicPeriod> academicPeriods;
            private List<Domain.Student.Entities.AcademicLevel> academicLevels;
            private List<Domain.Base.Entities.OtherDegree> otherDegrees;
            private List<Domain.Base.Entities.OtherMajor> otherMajors;
            private List<Domain.Base.Entities.OtherHonor> otherHonors;
            private List<Domain.Student.Entities.EnrollmentStatus> enrollmentStatuses;
            private List<Domain.Base.Entities.AcadCredential> academicCredentials;
            private List<Domain.Base.Entities.AcademicDiscipline> disciplines;

            private StudentAcademicPrograms2 studentAcademicProgram;

            private Tuple<IEnumerable<StudentAcademicProgram>, int> studentAcademicPrograms;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            private Dictionary<string, string> personGuidCollection;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentAcademicProgramRepositoryMock = new Mock<IStudentAcademicProgramRepository>();
                termRepositoryMock = new Mock<ITermRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                catalogRepositoryMock = new Mock<ICatalogRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                currentUserFactory = new ThirdPartyUserFactory();

                InitializeTestData();

                InitializeMock();

                studentAcademicProgramService = new StudentAcademicProgramService(adapterRegistryMock.Object, studentRepositoryMock.Object, studentAcademicProgramRepositoryMock.Object,
                    termRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, catalogRepositoryMock.Object, personRepositoryMock.Object, referenceDataRepositoryMock.Object,
                    currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                studentRepositoryMock = null;
                studentAcademicProgramRepositoryMock = null;
                termRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                catalogRepositoryMock = null;
                personRepositoryMock = null;
                referenceDataRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;
            }

            private void InitializeTestData()
            {
                defaultConfiguration = new DefaultsConfiguration() { HostInstitutionCodeId = "1" };
                personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", guid);

                academicPrograms = new List<Domain.Student.Entities.AcademicProgram>() { new Domain.Student.Entities.AcademicProgram(guid, "1", "desc") };

                catalogs = new List<Domain.Student.Entities.Requirements.Catalog>() { new Domain.Student.Entities.Requirements.Catalog(guid, "1", "desc", DateTime.Today) };

                locations = new List<Domain.Base.Entities.Location>() { new Domain.Base.Entities.Location(guid, "1", "desc") };

                departments = new List<Department>()
                {
                    new Department(guid, "1", "desc", true),
                    new Department("1a59eed8-5fe7-4120-b1cf-f23266b9e876", "2", "desc", true)
                };

                terms = new List<Term>() { new Term(guid, "1", "desc", DateTime.Today, DateTime.Today.AddDays(100), 2017, 1, true, true, "1", true) };

                academicPeriods = new List<Domain.Student.Entities.AcademicPeriod>() { new Domain.Student.Entities.AcademicPeriod(guid, "1", "desc", DateTime.Today, DateTime.Today.AddDays(100), 2017,
                    1, "1", "1", "1", new List<RegistrationDate>() { })};

                academicLevels = new List<Domain.Student.Entities.AcademicLevel>() { new Domain.Student.Entities.AcademicLevel(guid, "1", "desc") };

                otherDegrees = new List<Domain.Base.Entities.OtherDegree>() { new Domain.Base.Entities.OtherDegree(guid, "1", "desc") };

                otherMajors = new List<Domain.Base.Entities.OtherMajor>() { new Domain.Base.Entities.OtherMajor(guid, "1", "desc") };

                otherHonors = new List<Domain.Base.Entities.OtherHonor>() { new Domain.Base.Entities.OtherHonor(guid, "1", "desc") };

                disciplines = new List<Domain.Base.Entities.AcademicDiscipline>()
                {
                    new Domain.Base.Entities.AcademicDiscipline(guid, "1", "desc", Domain.Base.Entities.AcademicDisciplineType.Concentration),
                    new Domain.Base.Entities.AcademicDiscipline("1a59eed8-5fe7-4120-b1cf-f23266b9e876", "2", "desc", Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Domain.Base.Entities.AcademicDiscipline("1a59eed8-5fe7-4120-b1cf-f23266b9e877", "3", "desc", Domain.Base.Entities.AcademicDisciplineType.Minor)
                };

                academicCredentials = new List<Domain.Base.Entities.AcadCredential>()
                {
                    new Domain.Base.Entities.AcadCredential(guid, "1", "desc", Domain.Base.Entities.AcademicCredentialType.Degree),
                    new Domain.Base.Entities.AcadCredential("1a59eed8-5fe7-4120-b1cf-f23266b9e875", "2", "desc", Domain.Base.Entities.AcademicCredentialType.Certificate),
                    new Domain.Base.Entities.AcadCredential("1a59eed8-5fe7-4120-b1cf-f23266b9e876", "3", "desc", Domain.Base.Entities.AcademicCredentialType.Honorary),
                    new Domain.Base.Entities.AcadCredential("1a59eed8-5fe7-4120-b1cf-f23266b9e877", "4", "desc", Domain.Base.Entities.AcademicCredentialType.Diploma)
                };

                enrollmentStatuses = new List<Domain.Student.Entities.EnrollmentStatus>()
                {
                    new Domain.Student.Entities.EnrollmentStatus(guid, "1", "desc", Domain.Student.Entities.EnrollmentStatusType.active),
                    new Domain.Student.Entities.EnrollmentStatus("1a59eed8-5fe7-4120-b1cf-f23266b9e875", "2", "desc", Domain.Student.Entities.EnrollmentStatusType.complete)
                };

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "1")
                {
                    GraduationDate = DateTime.Today,
                    IsPrimary = true,
                    Location = "1",
                    DepartmentCode = "1",
                    StartTerm = "1",
                    AnticipatedCompletionTerm = "1",
                    GradTerm = "1",
                    AcademicLevelCode = "1",
                    DegreeCode = "1",
                    CreditsEarned = 0
                };

                studentAcademicProgramEntity.AddMajors("1");
                studentAcademicProgramEntity.AddHonors("1");

                studentAcademicPrograms = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgram = new StudentAcademicPrograms2()
                {
                    Id = guid,
                    StartDate = DateTime.Today,
                    Student = new GuidObject2(guid),
                    AcademicProgram = new GuidObject2(guid),
                    AcademicCatalog = new GuidObject2(guid),
                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active },
                    ProgramOwner = new GuidObject2(guid),
                    Preference = Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary,
                    Site = new GuidObject2(guid),
                    AcademicLevel = new GuidObject2(guid),
                    ExpectedGraduationDate = DateTime.Today.AddDays(100),
                    EndDate = DateTime.Today.AddDays(100),
                    GraduatedOn = DateTime.Today.AddDays(-10),
                    CredentialsDate = DateTime.Today.AddDays(10),
                    AcademicPeriods = new StudentAcademicProgramsAcademicPeriods()
                    {
                        ActualGraduation = new GuidObject2(guid),
                        Starting = new GuidObject2(guid),
                        ExpectedGraduation = new GuidObject2(guid)
                    },
                    PerformanceMeasure = "5",
                    Credentials = new List<GuidObject2>() { new GuidObject2(guid), new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e875") },
                    Disciplines = new List<StudentAcademicProgramDisciplines>()
                    {
                        new StudentAcademicProgramDisciplines()
                        {
                            Discipline = new GuidObject2(guid),
                            AdministeringInstitutionUnit = new GuidObject2(guid)
                        }
                    },
                    Recognitions = new List<GuidObject2>() { new GuidObject2(guid) },
                };
            }

            private void InitializeMock(bool bypassCache = true)
            {
                getStudentAcademicPrograms.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                createStudentAcademicPrograms.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms, createStudentAcademicPrograms });
                configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultConfiguration);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsAsync(bypassCache)).ReturnsAsync(academicPrograms);
                catalogRepositoryMock.Setup(c => c.GetAsync(bypassCache)).ReturnsAsync(catalogs);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(bypassCache)).ReturnsAsync(locations);
                referenceDataRepositoryMock.Setup(r => r.GetDepartments2Async(bypassCache)).ReturnsAsync(departments);
                referenceDataRepositoryMock.Setup(r => r.GetAcadCredentialsAsync(bypassCache)).ReturnsAsync(academicCredentials);
                termRepositoryMock.Setup(t => t.GetAsync(bypassCache)).ReturnsAsync(terms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsAsync(bypassCache)).ReturnsAsync(academicLevels);
                referenceDataRepositoryMock.Setup(r => r.GetOtherDegreesAsync(bypassCache)).ReturnsAsync(otherDegrees);
                referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsAsync(bypassCache)).ReturnsAsync(otherMajors);
                referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(true)).ReturnsAsync(otherHonors);
                referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(false)).ReturnsAsync(otherHonors);
                studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(bypassCache)).ReturnsAsync(enrollmentStatuses);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Today.ToString());
                referenceDataRepositoryMock.Setup(r => r.GetAcademicDisciplinesAsync(bypassCache)).ReturnsAsync(disciplines);

                //studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), bypassCache, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
                //    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                //    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAcademicPrograms);

                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<StudentAcademicProgram>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
                studentAcademicProgramRepositoryMock.Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<StudentAcademicProgram>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram2Async_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram2Async_Invalid_StudentId()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram2Async_Invalid_AcademicProgramId()
            {
                studentAcademicProgram.AcademicProgram.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_AcademicCatalogId_As_Null()
            {
                studentAcademicProgram.AcademicCatalog.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_AcademicCatalogId()
            {
                studentAcademicProgram.AcademicCatalog.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_EnrollmentStatusId_Null()
            {
                studentAcademicProgram.EnrollmentStatus.Detail = new GuidObject2(null);
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_EnrollmentStatus_And_Detail_NotMatch()
            {
                studentAcademicProgram.EnrollmentStatus.Detail = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e875");
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_EnrollmentStatusDetailId()
            {
                studentAcademicProgram.EnrollmentStatus.Detail = new GuidObject2(Guid.NewGuid().ToString());
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_ProgramOwnerId_Null()
            {
                studentAcademicProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
                studentAcademicProgram.ProgramOwner.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_ProgramOwnerId()
            {
                studentAcademicProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
                studentAcademicProgram.ProgramOwner.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_SiteId_Null()
            {
                studentAcademicProgram.Site.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_SiteId()
            {
                studentAcademicProgram.Site.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_AcademicLevelId_Null()
            {
                studentAcademicProgram.AcademicLevel.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_AcademicLevelId()
            {
                studentAcademicProgram.AcademicLevel.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_AcademicPeriodsStartingId_Null()
            {
                studentAcademicProgram.AcademicPeriods.Starting.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_AcademicPeriodsStartingId()
            {
                studentAcademicProgram.AcademicPeriods.Starting.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_AcademicPeriodsExpectedGraduationId_Null()
            {
                studentAcademicProgram.AcademicPeriods.ExpectedGraduation.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_AcademicPeriodsExpectedGraduationId()
            {
                studentAcademicProgram.AcademicPeriods.ExpectedGraduation.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_AcademicPeriodsActualGraduationId_Null()
            {
                studentAcademicProgram.AcademicPeriods.ActualGraduation.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_AcademicPeriodsActualGraduationId()
            {
                studentAcademicProgram.AcademicPeriods.ActualGraduation.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_CredentialId()
            {
                studentAcademicProgram.Credentials.FirstOrDefault().Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Credentials_As_Honorary()
            {
                studentAcademicProgram.Credentials.FirstOrDefault().Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e876";
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Credentials_As_Diploma()
            {
                studentAcademicProgram.Credentials.FirstOrDefault().Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e877";
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_CredentialsDegree_MoreThanOne()
            {
                studentAcademicProgram.Credentials.Add(new GuidObject2(guid));
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_DisciplineId_As_Null()
            {
                studentAcademicProgram.Disciplines.FirstOrDefault().Discipline.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_DisciplineId()
            {
                studentAcademicProgram.Disciplines.FirstOrDefault().Discipline.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_AdministeringInstitutionUnitId_As_Null()
            {
                studentAcademicProgram.Disciplines.FirstOrDefault().AdministeringInstitutionUnit.Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_AdministeringInstitutionUnitId()
            {
                studentAcademicProgram.Disciplines.FirstOrDefault().AdministeringInstitutionUnit.Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Disciplines_With_MorethanOne_AdministerDepts()
            {
                studentAcademicProgram.Disciplines.Add(new StudentAcademicProgramDisciplines()
                {
                    AdministeringInstitutionUnit = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e876"),
                    Discipline = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e877")
                });
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_DeptCode_NotMatchWith_DisciplinesAdministerDepts()
            {
                studentAcademicProgram.ProgramOwner.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e876";
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_PerformanceMeasure()
            {
                studentAcademicProgram.PerformanceMeasure = "First";
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_RecognitionsId_As_Null()
            {
                studentAcademicProgram.Recognitions.FirstOrDefault().Id = null;
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram_Invalid_RecognitionsId()
            {
                studentAcademicProgram.Recognitions.FirstOrDefault().Id = Guid.NewGuid().ToString();
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram()
            {
                var result = await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StuAcadPrgm_UpdateStudentAcademicProgram2Async_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await studentAcademicProgramService.UpdateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            public async Task StuAcadPrgm_UpdateStudentAcademicProgram2Async()
            {
                studentAcademicProgram.Disciplines.FirstOrDefault().Discipline.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e876";
                studentAcademicProgram.EnrollmentStatus.Detail = new GuidObject2(guid);
                var result = await studentAcademicProgramService.UpdateStudentAcademicProgram2Async(studentAcademicProgram);

                Assert.IsNotNull(result);
            }
        }
    }
}