// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
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
                    new Domain.Base.Entities.Department("ddd5d1f-910b-4f1a-a771-5847f554e8ac","ENG","ENGLISH DEPARTMENT", true),
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
                foreach (var acadProg in acadProgs)
                {
                    studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(acadProg.Code)).ReturnsAsync(acadProg.Guid);
                }
                foreach (var catalog in catalogs)
                {
                    catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(catalog.Code)).ReturnsAsync(catalog.Guid);
                }
                foreach (var location in locations)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(location.Code)).ReturnsAsync(location.Guid);
                }
                foreach (var dept in depts)
                {
                    referenceDataRepositoryMock.Setup(d => d.GetDepartments2GuidAsync(dept.Code)).ReturnsAsync(dept.Guid);
                }
                foreach (var academicPeriod in academicPeriodCollection)
                {
                    termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                foreach (var acadLevel in acadLevels)
                {
                    studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(acadLevel.Code)).ReturnsAsync(acadLevel.Guid);
                }
                foreach (var honor in allHonors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(honor.Code)).ReturnsAsync(honor.Guid);
                }
                foreach (var degree in allDegrees)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(degree.Code)).ReturnsAsync(degree.Guid);
                }
                foreach (var major in allMajors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(major.Code)).ReturnsAsync(major.Guid);
                }
                foreach (var minor in allMinors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherMinorsGuidAsync(minor.Code)).ReturnsAsync(minor.Guid);
                }
                foreach (var sp in allSp)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherSpecialsGuidAsync(sp.Code)).ReturnsAsync(sp.Guid);
                }
                foreach (var ccd in allCcds)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcdsGuidAsync(ccd.Code)).ReturnsAsync(ccd.Guid);
                }
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
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), defaultInst)).ReturnsAsync(() => null);
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

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_PermissionsException()
            //{
            //    //Arrange
            //    viewStudentProgramRole = new Domain.Entities.Role(1, "INVALID");
            //    // viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

            //    studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuidAsync(It.IsAny<string>(), defaultInst)).ReturnsAsync(() => null);

            //    var result = await StudentAcademicProgramService.GetStudentAcademicProgramByGuidAsync("abcd");

            //}

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
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
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
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
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
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
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
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(() => null);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
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

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
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
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);

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

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);

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
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuProgTuple);
                var result = await StudentAcademicProgramService.GetStudentAcademicProgramsAsync(It.IsAny<int>(), It.IsAny<int>());
                Assert.AreEqual(result.Item2, 0);

            }

            [TestMethod]
            public async Task StudentAcademicProgramService_GetStudentAcademicProgramsAsync_WithFilters()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
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
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(defaultInst, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(stuAcadProgsTuple);
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

        #region Post/Put Tests
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
                    new Domain.Base.Entities.Department("dddf089a-b631-46cf-9884-e9d310eeb685","ENG","English DEPARTMENT", true),
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

                personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);

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
                foreach (var acadProg in acadProgs)
                {
                    studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(acadProg.Code)).ReturnsAsync(acadProg.Guid);
                }
                foreach (var catalog in catalogs)
                {
                    catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(catalog.Code)).ReturnsAsync(catalog.Guid);
                }
                foreach (var location in locations)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(location.Code)).ReturnsAsync(location.Guid);
                }
                foreach (var dept in depts)
                {
                    referenceDataRepositoryMock.Setup(d => d.GetDepartments2GuidAsync(dept.Code)).ReturnsAsync(dept.Guid);
                }
                foreach (var academicPeriod in academicPeriodCollection)
                {
                    termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                foreach (var acadLevel in acadLevels)
                {
                    studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(acadLevel.Code)).ReturnsAsync(acadLevel.Guid);
                }
                foreach (var honor in allHonors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(honor.Code)).ReturnsAsync(honor.Guid);
                }
                foreach (var degree in allDegrees)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(degree.Code)).ReturnsAsync(degree.Guid);
                }
                foreach (var major in allMajors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(major.Code)).ReturnsAsync(major.Guid);
                }
                foreach (var minor in allMinors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherMinorsGuidAsync(minor.Code)).ReturnsAsync(minor.Guid);
                }
                foreach (var sp in allSp)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherSpecialsGuidAsync(sp.Code)).ReturnsAsync(sp.Guid);
                }
                foreach (var ccd in allCcds)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherCcdsGuidAsync(ccd.Code)).ReturnsAsync(ccd.Guid);
                }
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
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto, true);

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
                var result = await StudentAcademicProgramService.UpdateStudentAcademicProgramAsync(dto, true);

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
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto, true);

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
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto, true);

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
                studentAcademicProgramRepositoryMock.Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
                var dto = StuAcadProgDtos[2];
                dto.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
                dto.EnrollmentStatus.Detail = null;
                //Act
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto, true);

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
                personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var dto = StuAcadProgDtos[0];
                var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_ArgumentException_InvalidPersonID()
            {
                //Arrange
                viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });

                var expected = stuAcadProgs[0];
                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);

                personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);

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

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task StudentAcademicProgramService_CreateStudentAcademicProgramAsync_PermissionsException()
            //{
            //    //Arrange
            //    viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
            //    var expected = stuAcadProgs[0];
            //    studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<Domain.Student.Entities.StudentAcademicProgram>(), defaultInst)).ReturnsAsync(expected);
            //    var dto = StuAcadProgDtos[0];
            //    var result = await StudentAcademicProgramService.CreateStudentAcademicProgramAsync(dto);



            //}

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

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid2Async_PermissionException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await studentAcademicProgramService.GetStudentAcademicProgramByGuid2Async(guid);
            //}

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid2Async()
            {
                var result = await studentAcademicProgramService.GetStudentAcademicProgramByGuid2Async(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task StuAcadPrgm_GetStudentAcademicPrograms2Async_PermissionException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await studentAcademicProgramService.GetStudentAcademicPrograms2Async(0, 100);
            //}

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
                   It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);

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

                personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);

                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(bypassCache)).ReturnsAsync(locations);
                referenceDataRepositoryMock.Setup(r => r.GetDepartments2Async(bypassCache)).ReturnsAsync(departments);
                referenceDataRepositoryMock.Setup(r => r.GetAcadCredentialsAsync(bypassCache)).ReturnsAsync(academicCredentials);
                termRepositoryMock.Setup(t => t.GetAsync(bypassCache)).ReturnsAsync(terms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsAsync(bypassCache)).ReturnsAsync(academicLevels);
                referenceDataRepositoryMock.Setup(r => r.GetOtherDegreesAsync(bypassCache)).ReturnsAsync(otherDegrees);
                referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsAsync(bypassCache)).ReturnsAsync(otherMajors);
                referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(bypassCache)).ReturnsAsync(otherHonors);
                referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(false)).ReturnsAsync(otherHonors);
                studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(bypassCache)).ReturnsAsync(enrollmentStatuses);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Today.ToString());
                referenceDataRepositoryMock.Setup(r => r.GetAcademicDisciplinesAsync(bypassCache)).ReturnsAsync(disciplines);

                //studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), bypassCache, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
                //    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                //    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAcademicPrograms);

                studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgramAsync(It.IsAny<StudentAcademicProgram>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
                studentAcademicProgramRepositoryMock.Setup(s => s.UpdateStudentAcademicProgramAsync(It.IsAny<StudentAcademicProgram>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
                foreach (var acadProg in academicPrograms)
                {
                    studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(acadProg.Code)).ReturnsAsync(acadProg.Guid);
                }
                foreach (var catalog in catalogs)
                {
                    catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(catalog.Code)).ReturnsAsync(catalog.Guid);
                }
                foreach (var location in locations)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(location.Code)).ReturnsAsync(location.Guid);
                }
                foreach (var dept in departments)
                {
                    referenceDataRepositoryMock.Setup(d => d.GetDepartments2GuidAsync(dept.Code)).ReturnsAsync(dept.Guid);
                }
                foreach (var academicPeriod in academicPeriods)
                {
                    termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                foreach (var acadLevel in academicLevels)
                {
                    studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(acadLevel.Code)).ReturnsAsync(acadLevel.Guid);
                }
                foreach (var honor in otherHonors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(honor.Code)).ReturnsAsync(honor.Guid);
                }
                foreach (var degree in otherDegrees)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(degree.Code)).ReturnsAsync(degree.Guid);
                }
                foreach (var major in otherMajors)
                {
                    referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(major.Code)).ReturnsAsync(major.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicPrograms.FirstOrDefault().Guid);
                catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(It.IsAny<string>())).ReturnsAsync(catalogs.FirstOrDefault().Guid);
                referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(It.IsAny<string>())).ReturnsAsync(locations.FirstOrDefault().Guid);
                referenceDataRepositoryMock.Setup(dept => dept.GetDepartments2GuidAsync(It.IsAny<string>())).ReturnsAsync(departments.FirstOrDefault().Guid);
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicPeriods.FirstOrDefault().Guid);
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicLevels.FirstOrDefault().Guid);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(It.IsAny<string>())).ReturnsAsync(otherHonors.FirstOrDefault().Guid);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(It.IsAny<string>())).ReturnsAsync(otherDegrees.FirstOrDefault().Guid);
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(It.IsAny<string>())).ReturnsAsync(otherMajors.FirstOrDefault().Guid);
            }

            #endregion

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task StuAcadPrgm_CreateStudentAcademicProgram2Async_PermissionException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram2Async_Invalid_StudentId()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StuAcadPrgm_CreateStudentAcademicProgram2Async_Invalid_StudentId_IsCorp()
            {
                personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);
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
                var result = await studentAcademicProgramService.CreateStudentAcademicProgram2Async(studentAcademicProgram, true);

                Assert.IsNotNull(result);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task StuAcadPrgm_UpdateStudentAcademicProgram2Async_PermissionException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await studentAcademicProgramService.UpdateStudentAcademicProgram2Async(studentAcademicProgram);
            //}

            [TestMethod]
            public async Task StuAcadPrgm_UpdateStudentAcademicProgram2Async()
            {
                studentAcademicProgram.Disciplines.FirstOrDefault().Discipline.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e876";
                studentAcademicProgram.EnrollmentStatus.Detail = new GuidObject2(guid);
                var result = await studentAcademicProgramService.UpdateStudentAcademicProgram2Async(studentAcademicProgram, true);

                Assert.IsNotNull(result);
            }
        }
    }

    [TestClass]
    public class StudentAcademicProgramServiceTests_V16
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
            private List<Domain.Base.Entities.OtherMinor> otherMinors;
            private List<Domain.Base.Entities.OtherSpecial> otherSpecials;
            private List<Domain.Base.Entities.OtherHonor> otherHonors;
            private List<Domain.Student.Entities.EnrollmentStatus> enrollmentStatuses;
            private List<Domain.Base.Entities.AcadCredential> academicCredentials;

            private Tuple<IEnumerable<StudentAcademicProgram>, int> studentAcademicPrograms;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            private Dictionary<string, string> personGuidCollection;

            private DateTime today = DateTime.Today;

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

                otherMajors = new List<Domain.Base.Entities.OtherMajor>() { new Domain.Base.Entities.OtherMajor(Guid.NewGuid().ToString(), "1", "major1") };

                otherMinors = new List<Domain.Base.Entities.OtherMinor>() { new Domain.Base.Entities.OtherMinor(Guid.NewGuid().ToString(), "2", "minor2") };

                otherSpecials = new List<Domain.Base.Entities.OtherSpecial>() { new Domain.Base.Entities.OtherSpecial(Guid.NewGuid().ToString(), "3", "special3") };

                otherHonors = new List<Domain.Base.Entities.OtherHonor>() { new Domain.Base.Entities.OtherHonor(guid, "1", "desc") };

                academicCredentials = new List<Domain.Base.Entities.AcadCredential>() { new Domain.Base.Entities.AcadCredential(guid, "1", "desc", Domain.Base.Entities.AcademicCredentialType.Degree) };

                enrollmentStatuses = new List<Domain.Student.Entities.EnrollmentStatus>()
                {
                    new Domain.Student.Entities.EnrollmentStatus(Guid.NewGuid().ToString(), "A", "active", Domain.Student.Entities.EnrollmentStatusType.active),
                    new Domain.Student.Entities.EnrollmentStatus(Guid.NewGuid().ToString(), "C", "complete", Domain.Student.Entities.EnrollmentStatusType.complete),
                    new Domain.Student.Entities.EnrollmentStatus(Guid.NewGuid().ToString(), "I", "inactive", Domain.Student.Entities.EnrollmentStatusType.inactive)
                };

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "A")
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

                //studentAcademicProgramEntity.AddMajors("1");
                //studentAcademicProgramEntity.AddHonors("1");

                studentAcademicPrograms = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);
            }

            private void InitializeMock(bool bypassCache = true)
            {
                getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });
                configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultConfiguration);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgramEntity);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicPrograms);
                foreach (var academicProgram in academicPrograms)
                {
                    studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramByGuidAsync(academicProgram.Code)).ReturnsAsync(academicProgram);
                    studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsGuidAsync(academicProgram.Code)).ReturnsAsync(academicProgram.Guid);
                }

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);
                foreach (var location in locations)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetLocationsGuidAsync(location.Code)).ReturnsAsync(location.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetDepartments2Async(It.IsAny<bool>())).ReturnsAsync(departments);
                foreach (var department in departments)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetDepartments2GuidAsync(department.Code)).ReturnsAsync(department.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(academicCredentials);
               
                termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(terms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
                foreach (var academicPeriod in academicPeriods)
                {
                    termRepositoryMock.Setup(t => t.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);
                foreach (var acadLevel in academicLevels)
                {
                    studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsGuidAsync(acadLevel.Code)).ReturnsAsync(acadLevel.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetOtherDegreesAsync(It.IsAny<bool>())).ReturnsAsync(otherDegrees);

                foreach (var otherDegree in otherDegrees)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherDegreeGuidAsync(otherDegree.Code)).ReturnsAsync(otherDegree.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsAsync(It.IsAny<bool>())).ReturnsAsync(otherMajors);
                foreach (var major in otherMajors)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsGuidAsync(major.Code)).ReturnsAsync(major.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetOtherMinorsAsync(It.IsAny<bool>())).ReturnsAsync(otherMinors);
                foreach (var minor in otherMinors)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherMinorsGuidAsync(minor.Code)).ReturnsAsync(minor.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetOtherSpecialsAsync(It.IsAny<bool>())).ReturnsAsync(otherSpecials);
                foreach (var special in otherSpecials)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherSpecialsGuidAsync(special.Code)).ReturnsAsync(special.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(otherHonors);
                foreach (var honor in otherHonors)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsGuidAsync(honor.Code)).ReturnsAsync(honor.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatuses);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Today.ToString());
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid3Async_Guid_As_Null()
            {
                await studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(null);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid3Async_PermissionException()
            //{
            //    getStudentAcademicPrograms = new Domain.Entities.Role(1, "INVALID");
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

            //    await studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(guid);
            //}

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid3Async_ViewPermissions()
            {
                getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

                var result = await studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid3Async_CreatePermissions()
            {
                
                getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

                var result = await studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid3Async_RepositoryException()
            {
                var exception = new RepositoryException("Repository Exception") {  };

                exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception"));

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
                catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
                try
                {
                    await studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Errors.Count == 1);
                    Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid3Async_RepositoryException2()
            {
                var exception = new RepositoryException("Repository Exception Message") { };

                exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception 1"));

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
                catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
                try
                {
                    await studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Errors.Count == 2);
                    Assert.AreEqual(ex.Errors[0].Message, "Repository Exception 1", "Repository Exception 1");
                    Assert.AreEqual(ex.Errors[1].Message, "Repository Exception Message", "Repository Exception Message");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid3Async_RepositoryException3()
            {
                var exception = new RepositoryException("Repository Exception") { };

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
                catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
                try
                {
                    await studentAcademicProgramService.GetStudentAcademicProgramByGuid3Async(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Errors.Count == 1);
                    Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                    throw ex;
                }
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_ArgumentException()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms2Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

                var results = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { Id = Guid.NewGuid().ToString() });

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_Repository_Returns_Null()
            {
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), CurriculumObjectiveCategory.NotSet, It.IsAny<bool>())).ReturnsAsync(() => null);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { Id = guid });

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_Repository_Returns_EmptyResult()
            {
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), CurriculumObjectiveCategory.NotSet, It.IsAny<bool>())).ReturnsAsync(new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { }, 0));

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { Id = guid });

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.GraduatedOn, DateTime.Today, "GraduatedOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_EnrollmentStatusType_Complete()
            {
                var allStatusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "C")
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
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Complete, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.complete);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_EnrollmentStatusType_Inactive()
            {
                var allStatusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "I")
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
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Inactive, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.inactive);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_Disciplines()
            {
                
                studentAcademicProgramEntity.AddMajors("1", today.AddDays(-1), today);
                studentAcademicProgramEntity.AddMinors("2", today.AddDays(-1), today);
                studentAcademicProgramEntity.AddSpecializations("3", today.AddDays(-1), today);
    
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>
                    (new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                foreach (var discipline in response.Disciplines)
                {
                    var disciplineResponse = GetDiscipline(discipline.Discipline.Id, studentAcademicProgramEntity);
                    Assert.AreEqual(discipline.Discipline.Id, disciplineResponse.Item1, "Discipline.Id");
                    if (disciplineResponse.Item2 != null)
                    {
                        Assert.AreEqual(discipline.StartOn, disciplineResponse.Item2, "startOn");
                    }
                    if (disciplineResponse.Item3 != null)
                    {
                        Assert.AreEqual(discipline.EndOn, disciplineResponse.Item3, "endOn");
                    }
                }
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_Disciplines2()
            {

                studentAcademicProgramEntity.AddMajors("1");
                studentAcademicProgramEntity.AddMinors("2");
                studentAcademicProgramEntity.AddSpecializations("3");

                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>
                    (new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                foreach (var discipline in response.Disciplines)
                {
                    var disciplineResponse = GetDiscipline2(discipline.Discipline.Id, studentAcademicProgramEntity);
                    Assert.AreEqual(discipline.Discipline.Id, disciplineResponse, "Discipline.Id");
                    
                }
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_AdmitStatus()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>
                  (new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramEntity.AdmitStatus = "CR";

                var admissionPopulationEntityList = new List<Domain.Student.Entities.AdmissionPopulation>()
            {
                new Domain.Student.Entities.AdmissionPopulation("03ef76f3-61be-4990-8a9d-9a80282fc420", "CR", "Certificate"),
                new Domain.Student.Entities.AdmissionPopulation("d2f4f0af-6714-48c7-88dd-1c40cb407b6c", "FH", "Freshman Honors"),
                new Domain.Student.Entities.AdmissionPopulation("c517d7a5-f06a-42c8-85ad-b6320e1c0c2a", "FR", "First Time Freshman"),
                new Domain.Student.Entities.AdmissionPopulation("6c591aaa-5d33-4b19-b5ed-f6cf8956ef0a", "GD", "Graduate"),
                new Domain.Student.Entities.AdmissionPopulation("81cd5b52-9705-4b1b-8eed-669c63db05e2", "ND", "Non-Degree"),
                new Domain.Student.Entities.AdmissionPopulation("164dc1ad-4d72-4dae-987d-52f761bb0132", "TR", "Transfer"),
            };

                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntityList);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3());

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.AdmissionClassification.AdmissionCategory.Id, (admissionPopulationEntityList.FirstOrDefault(x => x.Code == "CR")).Guid, "AdmissionCategory.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_PersonFilter()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramsPersonFilterAsync( It.IsAny<int>(), It.IsAny<int>(),
                           It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadProgramTuple);


                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3()
                    {  });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.GraduatedOn, DateTime.Today, "GraduatedOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_StudentFilter()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);               

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { Student = new GuidObject2(guid) });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.GraduatedOn, DateTime.Today, "GraduatedOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_AcademicProgramFilter()
            {

                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { AcademicProgram = new GuidObject2(guid) });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.AcademicProgram.Id, guid, "AcademicProgram.Id");

            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_EnrollStatusFilter()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active } });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.GraduatedOn, DateTime.Today, "GraduatedOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_SiteFilter()
            {
                studentAcademicProgramEntity.Location = "1";
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { Site = new GuidObject2(guid) });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.Site.Id, guid, "Site.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_AcademicLevelFilter()
            {
                studentAcademicProgramEntity.AcademicLevelCode = "1";
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { AcademicLevel = new GuidObject2(guid) });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.AcademicLevel.Id, guid, "AcademicLevel.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_AcademicPeriodsFilter()
            {
                studentAcademicProgramEntity.StartTerm = "1";
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3()
                {
                    AcademicPeriods = new StudentAcademicProgramsAcademicPeriods()
                    {
                        ActualGraduation = new GuidObject2(guid)
                    }
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.AcademicPeriods.ActualGraduation.Id, guid, "AcademicPeriods.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_CredentialsFilter()
            {
                var academicCredential = academicCredentials.FirstOrDefault();

                studentAcademicProgramEntity.DegreeCode = "1";

                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, 
                    new StudentAcademicPrograms3()
                    {
                        Credentials = new List<GuidObject2>()
                        { new GuidObject2(academicCredential.Guid) }
                    });

               
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.Credentials[0].Id, academicCredential.Guid, "Credentials[0].Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_CurriculumObjective2_Applied()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Applied;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3()
                {
                   CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Applied
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Applied, "CurriculumObjective");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_CurriculumObjective2_Matriculated()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Matriculated;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated, "CurriculumObjective");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_CurriculumObjective2_Outcome()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Outcome;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome, "CurriculumObjective");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms3Async_CurriculumObjective2_Recruited()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Recruited;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Recruited
                });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Recruited, "CurriculumObjective");
            }
            
            /// <summary>
            /// When validating if the discipline.id is valid in the response, you must also check if it has a
            /// corresponding tuple object in studentAcadProgram containing a criteria start/end date.
            /// 
            /// This is made more difficult because there is no indicator in the DTO discipline response
            /// to indicate which displine the record is associated with
            /// </summary>
            /// <param name="disciplineId"></param>
            /// <param name="studentAcadProgram"></param>
            /// <returns></returns>
            private Tuple<string, DateTime?, DateTime?> GetDiscipline(string disciplineId, StudentAcademicProgram studentAcadProgram)
            {
                if (string.IsNullOrEmpty(disciplineId))
                    return new Tuple<string, DateTime?, DateTime?>(null, null, null);

                Tuple<string, DateTime?, DateTime?> disciplineTuple = null;
               

                var major = otherMajors.FirstOrDefault(x => x.Guid == disciplineId);
                if (major != null)
                {
                    if (studentAcadProgram.StudentProgramMajorsTuple != null)
                    {
                        disciplineTuple = studentAcadProgram.StudentProgramMajorsTuple.FirstOrDefault(t => t.Item1 == major.Code);
                    }
                    if (disciplineTuple != null)
                    {
                        return new Tuple<string, DateTime?, DateTime?>(major.Guid, disciplineTuple.Item2, disciplineTuple.Item3);
                    }
                    else
                        return new Tuple<string, DateTime?, DateTime?>(major.Guid, null, null);
                }

                var minor = otherMinors.FirstOrDefault(x => x.Guid == disciplineId);
                if (minor != null)
                {
                    if (studentAcadProgram.StudentProgramMinorsTuple != null)
                    {
                        disciplineTuple = studentAcadProgram.StudentProgramMinorsTuple.FirstOrDefault(t => t.Item1 == minor.Code);
                    }
                    if (disciplineTuple != null)
                    {
                        return new Tuple<string, DateTime?, DateTime?>(minor.Guid, disciplineTuple.Item2, disciplineTuple.Item3);
                    }
                    else
                        return new Tuple<string, DateTime?, DateTime?>(minor.Guid, null, null);
                }


                var special = otherSpecials.FirstOrDefault(x => x.Guid == disciplineId);
                if (special != null)
                {
                    if (studentAcadProgram.StudentProgramSpecializationsTuple != null)
                    {
                        disciplineTuple = studentAcadProgram.StudentProgramSpecializationsTuple.FirstOrDefault(t => t.Item1 == special.Code);
                    }
                    if (disciplineTuple != null)
                    {
                        return new Tuple<string, DateTime?, DateTime?>(special.Guid, disciplineTuple.Item2, disciplineTuple.Item3);
                    }
                    else
                        return new Tuple<string, DateTime?, DateTime?>(minor.Guid, null, null);
                }

                return new Tuple<string, DateTime?, DateTime?>(null, null, null);
            }

            private string GetDiscipline2(string disciplineId, StudentAcademicProgram studentAcadProgram)
            {
                if (string.IsNullOrEmpty(disciplineId))
                    return string.Empty;
         
                var major = otherMajors.FirstOrDefault(x => x.Guid == disciplineId);
                if (major != null)
                {
                    return major.Guid;
                }

                var minor = otherMinors.FirstOrDefault(x => x.Guid == disciplineId);
                if (minor != null)
                {
                    return minor.Guid;
                }
                var special = otherSpecials.FirstOrDefault(x => x.Guid == disciplineId);
                if (special != null)
                {
                    return special.Guid;
                }

                return string.Empty;
            }
        }
    }

    [TestClass]
    public class StudentAcademicProgramServiceTests_V17
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
            private List<Domain.Base.Entities.OtherMinor> otherMinors;
            private List<Domain.Base.Entities.OtherSpecial> otherSpecials;
            private List<Domain.Base.Entities.OtherHonor> otherHonors;
            private List<Domain.Student.Entities.EnrollmentStatus> enrollmentStatuses;
            private List<Domain.Base.Entities.AcadCredential> academicCredentials;

            private Tuple<IEnumerable<StudentAcademicProgram>, int> studentAcademicPrograms;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";
            private Dictionary<string, string> personGuidCollection;

            private DateTime today = DateTime.Today;

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

                otherMajors = new List<Domain.Base.Entities.OtherMajor>() { new Domain.Base.Entities.OtherMajor(Guid.NewGuid().ToString(), "1", "major1") };

                otherMinors = new List<Domain.Base.Entities.OtherMinor>() { new Domain.Base.Entities.OtherMinor(Guid.NewGuid().ToString(), "2", "minor2") };

                otherSpecials = new List<Domain.Base.Entities.OtherSpecial>() { new Domain.Base.Entities.OtherSpecial(Guid.NewGuid().ToString(), "3", "special3") };

                otherHonors = new List<Domain.Base.Entities.OtherHonor>() { new Domain.Base.Entities.OtherHonor(guid, "1", "desc") };

                academicCredentials = new List<Domain.Base.Entities.AcadCredential>() { new Domain.Base.Entities.AcadCredential(guid, "1", "desc", Domain.Base.Entities.AcademicCredentialType.Degree) };

                enrollmentStatuses = new List<Domain.Student.Entities.EnrollmentStatus>()
                {
                    new Domain.Student.Entities.EnrollmentStatus(Guid.NewGuid().ToString(), "A", "active", Domain.Student.Entities.EnrollmentStatusType.active),
                    new Domain.Student.Entities.EnrollmentStatus(Guid.NewGuid().ToString(), "C", "complete", Domain.Student.Entities.EnrollmentStatusType.complete),
                    new Domain.Student.Entities.EnrollmentStatus(Guid.NewGuid().ToString(), "I", "inactive", Domain.Student.Entities.EnrollmentStatusType.inactive)
                };

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "A")
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

                //studentAcademicProgramEntity.AddMajors("1");
                //studentAcademicProgramEntity.AddHonors("1");

                studentAcademicPrograms = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);
            }

            private void InitializeMock(bool bypassCache = true)
            {
                getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));

                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });
                configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultConfiguration);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgramEntity);
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicPrograms);
                foreach (var academicProgram in academicPrograms)
                {
                    studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramByGuidAsync(academicProgram.Code)).ReturnsAsync(academicProgram);
                    studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsGuidAsync(academicProgram.Code)).ReturnsAsync(academicProgram.Guid);
                }

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);
                foreach (var location in locations)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetLocationsGuidAsync(location.Code)).ReturnsAsync(location.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetDepartments2Async(It.IsAny<bool>())).ReturnsAsync(departments);
                foreach (var department in departments)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetDepartments2GuidAsync(department.Code)).ReturnsAsync(department.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(academicCredentials);

                termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(terms);
                termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
                foreach (var academicPeriod in academicPeriods)
                {
                    termRepositoryMock.Setup(t => t.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);
                foreach (var acadLevel in academicLevels)
                {
                    studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsGuidAsync(acadLevel.Code)).ReturnsAsync(acadLevel.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetOtherDegreesAsync(It.IsAny<bool>())).ReturnsAsync(otherDegrees);

                foreach (var otherDegree in otherDegrees)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherDegreeGuidAsync(otherDegree.Code)).ReturnsAsync(otherDegree.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsAsync(It.IsAny<bool>())).ReturnsAsync(otherMajors);
                foreach (var major in otherMajors)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsGuidAsync(major.Code)).ReturnsAsync(major.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetOtherMinorsAsync(It.IsAny<bool>())).ReturnsAsync(otherMinors);
                foreach (var minor in otherMinors)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherMinorsGuidAsync(minor.Code)).ReturnsAsync(minor.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetOtherSpecialsAsync(It.IsAny<bool>())).ReturnsAsync(otherSpecials);
                foreach (var special in otherSpecials)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherSpecialsGuidAsync(special.Code)).ReturnsAsync(special.Guid);
                }
                referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(otherHonors);
                foreach (var honor in otherHonors)
                {
                    referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsGuidAsync(honor.Code)).ReturnsAsync(honor.Guid);
                }
                studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatuses);
                studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Today.ToString());
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_Guid_As_Null()
            {
                await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(null);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_PermissionException()
            //{
            //    getStudentAcademicPrograms = new Domain.Entities.Role(1, "INVALID");
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

            //    await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
            //}

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_ViewPermissions()
            {
                getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

                var result = await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_CreatePermissions()
            {

                getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

                var result = await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_RepositoryException()
            {
                var exception = new RepositoryException("Repository Exception") { };

                exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception"));

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
                catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
                try
                {
                    await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Errors.Count == 1);
                    Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_RepositoryException2()
            {
                var exception = new RepositoryException("Repository Exception Message") { };

                exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception 1"));

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
                catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
                try
                {
                    await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Errors.Count == 2);
                    Assert.AreEqual(ex.Errors[0].Message, "Repository Exception 1", "Repository Exception 1");
                    Assert.AreEqual(ex.Errors[1].Message, "Repository Exception Message", "Repository Exception Message");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_RepositoryException3()
            {
                var exception = new RepositoryException("Repository Exception") { };

                catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
                catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
                try
                {
                    await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex);
                    Assert.IsTrue(ex.Errors.Count == 1);
                    Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                    throw ex;
                }
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_PermissionException()
            //{
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            //    await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, null, "");
            //}

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_ArgumentException()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), CurriculumObjectiveCategory.NotSet, It.IsAny<bool>()))
                   .ThrowsAsync(new Exception());

                var results = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4() { Id = Guid.NewGuid().ToString() }, "");

                Assert.IsNotNull(results);
                Assert.AreEqual(0, results.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_Repository_Returns_Null()
            {
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), CurriculumObjectiveCategory.NotSet, It.IsAny<bool>())).ReturnsAsync(() => null);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4() { Id = guid }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_Repository_Returns_EmptyResult()
            {
                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), CurriculumObjectiveCategory.NotSet, It.IsAny<bool>())).ReturnsAsync(new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { }, 0));

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4() { Id = guid }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4(), "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_EnrollmentStatusType_Complete()
            {
                var allStatusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "C")
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
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);


                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4(), "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Complete, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.complete);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_EnrollmentStatusType_Inactive()
            {
                var allStatusItems = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(It.IsAny<bool>()).Result.ToList();

                studentAcademicProgramEntity = new StudentAcademicProgram("1", "1", "1", guid, DateTime.Today, "I")
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
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4(), "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Inactive, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.inactive);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_Disciplines()
            {

                studentAcademicProgramEntity.AddMajors("1", today.AddDays(-1), today);
                studentAcademicProgramEntity.AddMinors("2", today.AddDays(-1), today);
                studentAcademicProgramEntity.AddSpecializations("3", today.AddDays(-1), today);

                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>
                    (new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4(), "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                foreach (var discipline in response.Disciplines)
                {
                    var disciplineResponse = GetDiscipline(discipline.Discipline.Id, studentAcademicProgramEntity);
                    Assert.AreEqual(discipline.Discipline.Id, disciplineResponse.Item1, "Discipline.Id");
                    if (disciplineResponse.Item2 != null)
                    {
                        Assert.AreEqual(discipline.StartOn, disciplineResponse.Item2, "startOn");
                    }
                    if (disciplineResponse.Item3 != null)
                    {
                        Assert.AreEqual(discipline.EndOn, disciplineResponse.Item3, "endOn");
                    }
                }
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_Disciplines2()
            {

                studentAcademicProgramEntity.AddMajors("1");
                studentAcademicProgramEntity.AddMinors("2");
                studentAcademicProgramEntity.AddSpecializations("3");

                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>
                    (new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4(), "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                foreach (var discipline in response.Disciplines)
                {
                    var disciplineResponse = GetDiscipline2(discipline.Discipline.Id, studentAcademicProgramEntity);
                    Assert.AreEqual(discipline.Discipline.Id, disciplineResponse, "Discipline.Id");

                }
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_AdmitStatus()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>
                  (new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramEntity.AdmitStatus = "CR";

                var admissionPopulationEntityList = new List<Domain.Student.Entities.AdmissionPopulation>()
            {
                new Domain.Student.Entities.AdmissionPopulation("03ef76f3-61be-4990-8a9d-9a80282fc420", "CR", "Certificate"),
                new Domain.Student.Entities.AdmissionPopulation("d2f4f0af-6714-48c7-88dd-1c40cb407b6c", "FH", "Freshman Honors"),
                new Domain.Student.Entities.AdmissionPopulation("c517d7a5-f06a-42c8-85ad-b6320e1c0c2a", "FR", "First Time Freshman"),
                new Domain.Student.Entities.AdmissionPopulation("6c591aaa-5d33-4b19-b5ed-f6cf8956ef0a", "GD", "Graduate"),
                new Domain.Student.Entities.AdmissionPopulation("81cd5b52-9705-4b1b-8eed-669c63db05e2", "ND", "Non-Degree"),
                new Domain.Student.Entities.AdmissionPopulation("164dc1ad-4d72-4dae-987d-52f761bb0132", "TR", "Transfer"),
            };

                studentReferenceDataRepositoryMock.Setup(i => i.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulationEntityList);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4(),"");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.AdmissionClassification.AdmissionCategory.Id, (admissionPopulationEntityList.FirstOrDefault(x => x.Code == "CR")).Guid, "AdmissionCategory.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_PersonFilter()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramsPersonFilterAsync( It.IsAny<int>(), It.IsAny<int>(),
                           It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcadProgramTuple);


                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4(){ }, "test");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_StudentFilter()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4() { Student = new GuidObject2(guid) }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_AcademicProgramFilter()
            {

                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms3Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);
               

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms3Async(0, 100, new StudentAcademicPrograms3() { AcademicProgram = new GuidObject2(guid) });

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.AcademicProgram.Id, guid, "AcademicProgram.Id");

            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_EnrollStatusFilter()
            {
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4() { EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active } }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.EnrollmentStatus.EnrollStatus, Dtos.EnrollmentStatusType.Active, "EnrollStatus");
                var enrollmentStatus = enrollmentStatuses.FirstOrDefault(x => x.EnrollmentStatusType == Domain.Student.Entities.EnrollmentStatusType.active);
                Assert.AreEqual(response.EnrollmentStatus.Detail.Id, enrollmentStatus.Guid, "EnrollmentStatus.Detail.Id");
                Assert.AreEqual(response.Student.Id, guid, "Student.Id");
                Assert.AreEqual(response.StartOn, DateTime.Today, "StartOn");
                Assert.AreEqual(response.Preference, Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary, "Preference");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_SiteFilter()
            {
                studentAcademicProgramEntity.Location = "1";
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4() { Site = new GuidObject2(guid) }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.Site.Id, guid, "Site.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_AcademicLevelFilter()
            {
                studentAcademicProgramEntity.AcademicLevelCode = "1";
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4() { AcademicLevel = new GuidObject2(guid) }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.AcademicLevel.Id, guid, "AcademicLevel.Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_CredentialsFilter()
            {
                var academicCredential = academicCredentials.FirstOrDefault();

                studentAcademicProgramEntity.DegreeCode = "1";

                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100,
                    new StudentAcademicPrograms4()
                    {
                        Credentials = new List<GuidObject2>()
                        { new GuidObject2(academicCredential.Guid) }
                    }, "");


                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.Credentials[0].Id, academicCredential.Guid, "Credentials[0].Id");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_CurriculumObjective2_Applied()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Applied;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Applied
                }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Applied, "CurriculumObjective");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_CurriculumObjective2_Matriculated()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Matriculated;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated
                }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated, "CurriculumObjective");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_CurriculumObjective2_Outcome()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Outcome;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome
                }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome, "CurriculumObjective");
            }

            [TestMethod]
            public async Task StuAcadPrgm_GetStudentAcademicPrograms4Async_CurriculumObjective2_Recruited()
            {
                studentAcademicProgramEntity.CurriculumObjective = CurriculumObjectiveCategory.Recruited;
                var studentAcadProgramTuple = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

                studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicPrograms4Async(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CurriculumObjectiveCategory>(), It.IsAny<bool>()))
                    .ReturnsAsync(studentAcadProgramTuple);

                var result = await studentAcademicProgramService.GetStudentAcademicPrograms4Async(0, 100, new StudentAcademicPrograms4()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Recruited
                }, "");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item2);
                var response = result.Item1.FirstOrDefault();
                Assert.AreEqual(response.Id, studentAcademicProgramEntity.Guid, "guid");
                Assert.AreEqual(response.CurriculumObjective, Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Recruited, "CurriculumObjective");
            }

            /// <summary>
            /// When validating if the discipline.id is valid in the response, you must also check if it has a
            /// corresponding tuple object in studentAcadProgram containing a criteria start/end date.
            /// 
            /// This is made more difficult because there is no indicator in the DTO discipline response
            /// to indicate which displine the record is associated with
            /// </summary>
            /// <param name="disciplineId"></param>
            /// <param name="studentAcadProgram"></param>
            /// <returns></returns>
            private Tuple<string, DateTime?, DateTime?> GetDiscipline(string disciplineId, StudentAcademicProgram studentAcadProgram)
            {
                if (string.IsNullOrEmpty(disciplineId))
                    return new Tuple<string, DateTime?, DateTime?>(null, null, null);

                Tuple<string, DateTime?, DateTime?> disciplineTuple = null;


                var major = otherMajors.FirstOrDefault(x => x.Guid == disciplineId);
                if (major != null)
                {
                    if (studentAcadProgram.StudentProgramMajorsTuple != null)
                    {
                        disciplineTuple = studentAcadProgram.StudentProgramMajorsTuple.FirstOrDefault(t => t.Item1 == major.Code);
                    }
                    if (disciplineTuple != null)
                    {
                        return new Tuple<string, DateTime?, DateTime?>(major.Guid, disciplineTuple.Item2, disciplineTuple.Item3);
                    }
                    else
                        return new Tuple<string, DateTime?, DateTime?>(major.Guid, null, null);
                }

                var minor = otherMinors.FirstOrDefault(x => x.Guid == disciplineId);
                if (minor != null)
                {
                    if (studentAcadProgram.StudentProgramMinorsTuple != null)
                    {
                        disciplineTuple = studentAcadProgram.StudentProgramMinorsTuple.FirstOrDefault(t => t.Item1 == minor.Code);
                    }
                    if (disciplineTuple != null)
                    {
                        return new Tuple<string, DateTime?, DateTime?>(minor.Guid, disciplineTuple.Item2, disciplineTuple.Item3);
                    }
                    else
                        return new Tuple<string, DateTime?, DateTime?>(minor.Guid, null, null);
                }


                var special = otherSpecials.FirstOrDefault(x => x.Guid == disciplineId);
                if (special != null)
                {
                    if (studentAcadProgram.StudentProgramSpecializationsTuple != null)
                    {
                        disciplineTuple = studentAcadProgram.StudentProgramSpecializationsTuple.FirstOrDefault(t => t.Item1 == special.Code);
                    }
                    if (disciplineTuple != null)
                    {
                        return new Tuple<string, DateTime?, DateTime?>(special.Guid, disciplineTuple.Item2, disciplineTuple.Item3);
                    }
                    else
                        return new Tuple<string, DateTime?, DateTime?>(minor.Guid, null, null);
                }

                return new Tuple<string, DateTime?, DateTime?>(null, null, null);
            }

            private string GetDiscipline2(string disciplineId, StudentAcademicProgram studentAcadProgram)
            {
                if (string.IsNullOrEmpty(disciplineId))
                    return string.Empty;

                var major = otherMajors.FirstOrDefault(x => x.Guid == disciplineId);
                if (major != null)
                {
                    return major.Guid;
                }

                var minor = otherMinors.FirstOrDefault(x => x.Guid == disciplineId);
                if (minor != null)
                {
                    return minor.Guid;
                }
                var special = otherSpecials.FirstOrDefault(x => x.Guid == disciplineId);
                if (special != null)
                {
                    return special.Guid;
                }

                return string.Empty;
            }
        }
    }

    [TestClass]
    public class StudentAcademicProgramSubmissionTests : StudentUserSetup
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
        private List<Domain.Student.Entities.AdmissionPopulation> admissionPopulations;
        private List<Domain.Base.Entities.OtherDegree> otherDegrees;
        private List<Domain.Base.Entities.OtherMajor> otherMajors;
        private List<Domain.Base.Entities.OtherHonor> otherHonors;
        private List<Domain.Student.Entities.EnrollmentStatus> enrollmentStatuses;
        private List<Domain.Base.Entities.AcadCredential> academicCredentials;
        private List<Domain.Base.Entities.AcademicDiscipline> disciplines;

        private StudentAcademicProgramsSubmissions studentAcademicProgramSubmissions;

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

            admissionPopulations = new List<Domain.Student.Entities.AdmissionPopulation>() { new Domain.Student.Entities.AdmissionPopulation(guid, "1", "desc") };
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
                CreditsEarned = 0,
                AdmitStatus = "1"
            };

            studentAcademicProgramEntity.AddMajors("1");
            studentAcademicProgramEntity.AddHonors("1");

            studentAcademicPrograms = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

            studentAcademicProgramSubmissions = new StudentAcademicProgramsSubmissions()
            {
                Id = guid,
                StartOn = DateTime.Today,
                Student = new GuidObject2(guid),
                CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated,
                AcademicProgram = new GuidObject2(guid),
                AcademicCatalog = new GuidObject2(guid),
                EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active },
                ProgramOwner = new GuidObject2(guid),
                Site = new GuidObject2(guid),
                AcademicLevel = new GuidObject2(guid),
                ExpectedGraduationDate = DateTime.Today.AddDays(100),
               
                AcademicPeriods = new StudentAcademicProgramsAcademicPeriods2()
                {
                    Starting = new GuidObject2(guid),
                    ExpectedGraduation = new GuidObject2(guid)
                },
                Credentials = new List<GuidObject2>() { new GuidObject2(guid), new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e875") },
                Disciplines = new List<StudentAcademicProgramDisciplines2>()
                    {
                        new StudentAcademicProgramDisciplines2()
                        {
                            Discipline = new GuidObject2(guid),
                            AdministeringInstitutionUnit = new GuidObject2(guid),
                            StartOn = DateTime.Today,
                        }
                    },
                AdmissionClassification = new Dtos.DtoProperties.AdmissionClassificationDtoProperty()
                {
                    AdmissionCategory = new GuidObject2(guid)
                }
            };
        }

        private void InitializeMock(bool bypassCache = true)
        {
            getStudentAcademicPrograms.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            createStudentAcademicPrograms.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.CreateStudentAcademicProgramConsent));

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms, createStudentAcademicPrograms });
            configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultConfiguration);
            studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgramEntity);
            studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicPrograms);
            catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);

            referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);
            referenceDataRepositoryMock.Setup(r => r.GetDepartments2Async(It.IsAny<bool>())).ReturnsAsync(departments);
            referenceDataRepositoryMock.Setup(r => r.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(academicCredentials);
            termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(terms);
            termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
            studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);
            studentReferenceDataRepositoryMock.Setup(s => s.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulations);
            referenceDataRepositoryMock.Setup(r => r.GetOtherDegreesAsync(It.IsAny<bool>())).ReturnsAsync(otherDegrees);
            referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsAsync(It.IsAny<bool>())).ReturnsAsync(otherMajors);
            referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(otherHonors);
            referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(false)).ReturnsAsync(otherHonors);
            studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatuses);
            studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatuses);
            studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Today.ToString());
            referenceDataRepositoryMock.Setup(r => r.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(disciplines);

            //studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), bypassCache, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
            //    , It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            //    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAcademicPrograms);

            studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicProgram>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
            studentAcademicProgramRepositoryMock.Setup(s => s.UpdateStudentAcademicProgram2Async(It.IsAny<StudentAcademicProgram>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
            foreach (var acadProg in academicPrograms)
            {
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(acadProg.Code)).ReturnsAsync(acadProg.Guid);
            }
            foreach (var catalog in catalogs)
            {
                catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(catalog.Code)).ReturnsAsync(catalog.Guid);
            }
            foreach (var location in locations)
            {
                referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(location.Code)).ReturnsAsync(location.Guid);
            }
            foreach (var dept in departments)
            {
                referenceDataRepositoryMock.Setup(d => d.GetDepartments2GuidAsync(dept.Code)).ReturnsAsync(dept.Guid);
            }
            foreach (var academicPeriod in academicPeriods)
            {
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
            }
            foreach (var acadLevel in academicLevels)
            {
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(acadLevel.Code)).ReturnsAsync(acadLevel.Guid);
            }
            foreach (var admissionPopulation in admissionPopulations)
            {
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAdmissionPopulationsGuidAsync(admissionPopulation.Code)).ReturnsAsync(admissionPopulation.Guid);
            }
            foreach (var honor in otherHonors)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(honor.Code)).ReturnsAsync(honor.Guid);
            }
            foreach (var degree in otherDegrees)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(degree.Code)).ReturnsAsync(degree.Guid);
            }
            foreach (var major in otherMajors)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(major.Code)).ReturnsAsync(major.Guid);
            }
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicPrograms.FirstOrDefault().Guid);
            catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(It.IsAny<string>())).ReturnsAsync(catalogs.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(It.IsAny<string>())).ReturnsAsync(locations.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(dept => dept.GetDepartments2GuidAsync(It.IsAny<string>())).ReturnsAsync(departments.FirstOrDefault().Guid);
            termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicPeriods.FirstOrDefault().Guid);
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicLevels.FirstOrDefault().Guid);
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAdmissionPopulationsGuidAsync(It.IsAny<string>())).ReturnsAsync(admissionPopulations.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(It.IsAny<string>())).ReturnsAsync(otherHonors.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(It.IsAny<string>())).ReturnsAsync(otherDegrees.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(It.IsAny<string>())).ReturnsAsync(otherMajors.FirstOrDefault().Guid);
        }

        #endregion

# region GetStudentAcademicProgramSubmissionsByGuid

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_Guid_As_Null()
        {
            await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(null);
        }


        [TestMethod]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_ViewPermissions()
        {
            getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

            var result = await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_Entity_no_Guid()
        {
            getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });
            var studentAcademicProgramEntity1 = new StudentAcademicProgram("1", "1", "1", "", DateTime.Today, "1");
            studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgramEntity1);
            var result = await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            
        }

        //no default institution set
        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentAcademicProgramService_GetStudentAcademicProgramSubmissionsByGuidAsync_NoDefaultInstitution()
        {
            //Arrange
            viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
            string guid = "bfde7c40-f27b-4747-bbd1-aab4b3b77bb9";
            var defaultsConfig = new DefaultsConfiguration { HostInstitutionCodeId = null };
            configurationRepositoryMock.Setup(conf => conf.GetDefaultsConfiguration()).Returns(defaultsConfig);
            studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgramEntity);
            //Act
            var result = await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
        }

        //bad guid
        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task StudentAcademicProgramService_GetStudentAcademicProgramSubmissionsByGuidAsync_RepositoryException_NullEntity()
        {
            //Arrange
            viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
            studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
            var result = await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);

        }

        //bad guid
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAcademicProgramService_GetStudentAcademicProgramByGuidAsync_KeyNotFoundException_NullEntity()
        {
            //Arrange
            viewStudentProgramRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentAcademicProgramConsent));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentProgramRole });
            studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            var result = await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);

        }



        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_Catalog_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
            catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_Program_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_location_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_Dept_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            referenceDataRepositoryMock.Setup(dept => dept.GetDepartments2GuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_term_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_acadLevel_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_AdmissionPopulations_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAdmissionPopulationsGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_GetStudentAcademicProgramSubmissionsByGuidAsync_StudentId_RepositoryException()
        {
            var exception = new RepositoryException("Repository Exception") { };

            personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(new Dictionary<string, string>());
            try
            {
                await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Person guid not found for Person Id: '1'");
                throw ex;
            }
        }

        [TestMethod]
        public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_CreatePermissions()
        {

            getStudentAcademicPrograms.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentAcademicProgramConsent));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms });

            var result = await studentAcademicProgramService.GetStudentAcademicProgramSubmissionByGuidAsync(guid);
            Assert.IsNotNull(result);
            Assert.AreEqual(guid, result.Id);
        }

        /* [TestMethod]
         [ExpectedException(typeof(IntegrationApiException))]
         public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_RepositoryException()
         {
             var exception = new RepositoryException("Repository Exception") { };

             exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception"));

             catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
             catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
             try
             {
                 await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
             }
             catch (IntegrationApiException ex)
             {
                 Assert.IsNotNull(ex);
                 Assert.IsTrue(ex.Errors.Count == 1);
                 Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                 throw ex;
             }
         }

         [TestMethod]
         [ExpectedException(typeof(IntegrationApiException))]
         public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_RepositoryException2()
         {
             var exception = new RepositoryException("Repository Exception Message") { };

             exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception 1"));

             catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
             catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
             try
             {
                 await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
             }
             catch (IntegrationApiException ex)
             {
                 Assert.IsNotNull(ex);
                 Assert.IsTrue(ex.Errors.Count == 2);
                 Assert.AreEqual(ex.Errors[0].Message, "Repository Exception 1", "Repository Exception 1");
                 Assert.AreEqual(ex.Errors[1].Message, "Repository Exception Message", "Repository Exception Message");
                 throw ex;
             }
         }

         [TestMethod]
         [ExpectedException(typeof(IntegrationApiException))]
         public async Task StuAcadPrgm_GetStudentAcademicProgramByGuid4Async_RepositoryException3()
         {
             var exception = new RepositoryException("Repository Exception") { };

             catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(exception);
             catalogRepositoryMock.Setup(c => c.GetCatalogGuidAsync(It.IsAny<string>())).ThrowsAsync(exception);
             try
             {
                 await studentAcademicProgramService.GetStudentAcademicProgramByGuid4Async(guid);
             }
             catch (IntegrationApiException ex)
             {
                 Assert.IsNotNull(ex);
                 Assert.IsTrue(ex.Errors.Count == 1);
                 Assert.IsTrue(ex.Errors[0].Message == "Repository Exception");
                 throw ex;
             }
         }*/


        #endregion
        #region PUT/POST
        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_PermissionException()
        //{
        //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
        //    await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
        //}

        

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_null_Dto()
        {
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(null);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Must provide a StudentAcademicProgramsSubmissions object for update");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_null_Program()
        {
            studentAcademicProgramSubmissions.AcademicProgram = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Program.id is a required property");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_student_Empty()
        {
            studentAcademicProgramSubmissions.Student.Id = string.Empty;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is a required property");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_CurriculumObjective_Null()
        {
            studentAcademicProgramSubmissions.CurriculumObjective = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "CurriculumObjective is a required property");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_CurriculumObjective_Not_Matriculated()
        {
            studentAcademicProgramSubmissions.CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "CurriculumObjective must be set to 'matriculated' for any new or updated student programs.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_StartOn_Null()
        {
            studentAcademicProgramSubmissions.StartOn = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "StartOn is required to create or update a matriculated student program.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_EnrollmentStatus()
        {
            studentAcademicProgramSubmissions.EnrollmentStatus = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EnrollmentStatus is a required property");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_StartOn_Before_EndOn()
        {
            studentAcademicProgramSubmissions.StartOn = DateTime.Parse("01/06/2018");
            studentAcademicProgramSubmissions.EndOn = DateTime.Parse("01/04/2018");
            studentAcademicProgramSubmissions.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EndOn cannot be before startOn.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_StartOn_Before_ExpectedGraduationDate()
        {
            studentAcademicProgramSubmissions.StartOn = DateTime.Parse("01/06/2018");
            studentAcademicProgramSubmissions.ExpectedGraduationDate = DateTime.Parse("01/04/2018");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ExpectedGraduationDate cannot be before startOn.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_EnrollmentStatus_InActive_No_EndOn()
        {
            studentAcademicProgramSubmissions.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
            studentAcademicProgramSubmissions.EndOn = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EndOn is required for the enrollment status of inactive.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_EnrollmentStatus_Complete()
        {
            studentAcademicProgramSubmissions.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Enrollment status of complete is not supported. Graduation processing can only be invoked directly in Colleague.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Preference()
        {
            studentAcademicProgramSubmissions.Preference = Dtos.EnumProperties.StudentAcademicProgramsPreference.Primary;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Preference may not be set for a student's program.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_EnrollmentStatus_Active_EndOn()
        {
            studentAcademicProgramSubmissions.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
            studentAcademicProgramSubmissions.EndOn = DateTime.Parse("01/04/2018");
            studentAcademicProgramSubmissions.StartOn = DateTime.Parse("01/01/2018");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EndOn is not valid for the enrollment status of active.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Credentials_Id_Null()
        {
            studentAcademicProgramSubmissions.Credentials[0].Id = string.Empty;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credential id is a required field when credentials are in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Disciplines_Discipline_Null()
        {
            studentAcademicProgramSubmissions.Disciplines[0].Discipline = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Discipline is a required property when disciplines are in the message body.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_DisciplineId_As_Null()
        {
            studentAcademicProgramSubmissions.Disciplines.FirstOrDefault().Discipline.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Discipline id is a required property when discipline is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_StudentId()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is not a valid GUID for persons.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_StudentId_Exception()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is not a valid GUID for persons.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_StudentId_IsCorp()
        {
            personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is not a valid GUID for persons.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_AcademicProgramId()
        {
            studentAcademicProgramSubmissions.AcademicProgram.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Program.Id is not a valid GUID for academic-programs.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync__AcademicProgramId_Exception()
        {
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Program.Id is not a valid GUID for academic-programs.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgram_AcademicCatalogId_As_Null()
        {
            studentAcademicProgramSubmissions.AcademicCatalog.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Catalog.id is a required property when catalog is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_AcademicCatalogId()
        {
            catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Catalog.Id is not a valid GUID for academic-catalogs.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_EnrollmentStatusId_Null()
        {
            studentAcademicProgramSubmissions.EnrollmentStatus.Detail = new GuidObject2(null);
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EnrollmentStatus.Detail.Id is a required field when detail is in the message body");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_EnrollmentStatus_And_Detail_NotMatch()
        {
            studentAcademicProgramSubmissions.EnrollmentStatus.Detail = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e875");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == " The enrollment Status of 'complete' referred by the detail ID '1a59eed8-5fe7-4120-b1cf-f23266b9e875' is different from that in the payload.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_EnrollmentStatusDetailId()
        {
            studentAcademicProgramSubmissions.EnrollmentStatus.Detail = new GuidObject2(Guid.NewGuid().ToString());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EnrollmentStatus.Detail.Id not a valid GUID for enrollment-statuses.");
                throw ex;
            }
        }



        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_EnrollmentStatusDetailId_Exception()
        {
            studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Unable to retrieve enrollment-statuses.");
                throw ex;
            }
        }
        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Null_ProgramOwnerId()
        {
            studentAcademicProgramSubmissions.ProgramOwner.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ProgramOwner.id is a required property when programOwner is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_ProgramOwnerId()
        {
            studentAcademicProgramSubmissions.ProgramOwner.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ProgramOwner.id is not a valid GUID for educational-institution-units.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_SiteId_Null()
        {
            studentAcademicProgramSubmissions.Site.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Site.id is a required property when site is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_SiteId()
        {
            studentAcademicProgramSubmissions.Site.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Site.id is not a valid GUID for sites.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_AcademicLevelId_Null()
        {
            studentAcademicProgramSubmissions.AcademicLevel.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicLevel.id is a required property when site is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_AcademicLevelId()
        {
            studentAcademicProgramSubmissions.AcademicLevel.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicLevel.id is not a valid GUID for academic-levels.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_AcademicPeriodsStartingId_Null()
        {
            studentAcademicProgramSubmissions.AcademicPeriods.Starting.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.starting.id is a required property when academicPeriods.starting is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_AcademicPeriodsStartingId()
        {
            studentAcademicProgramSubmissions.AcademicPeriods.Starting.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.starting.id is not a valid GUID for academic-periods.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_AcademicPeriodsExpectedGraduationId_Null()
        {
            studentAcademicProgramSubmissions.AcademicPeriods.ExpectedGraduation.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.expectedGraduation.id is a required property when academicPeriods.expectedGraduation is in the message body.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_AcademicPeriodsExpectedGraduationId()
        {
            studentAcademicProgramSubmissions.AcademicPeriods.ExpectedGraduation.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.expectedGraduation.id is not a valid GUID for academic-periods.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_CredentialId()
        {
            studentAcademicProgramSubmissions.Credentials.FirstOrDefault().Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials.id is not a valid GUID for academic-credentials.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Credentials_As_Honorary()
        {
            studentAcademicProgramSubmissions.Credentials.FirstOrDefault().Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e876";
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials.id of type honor is not supported.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Credentials_As_Diploma()
        {
            studentAcademicProgramSubmissions.Credentials.FirstOrDefault().Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e877";
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials.id of type diploma is not supported.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_CredentialsDegree_MoreThanOne()
        {
            studentAcademicProgramSubmissions.Credentials.Add(new GuidObject2(guid));
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials array cannot have more than one degree.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Invalid_DisciplineId()
        {
            studentAcademicProgramSubmissions.Disciplines.FirstOrDefault().Discipline.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Disciplines.discipline.id is not a valid GUID for academic-disciplines.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Discipline_StartOn_Endon()
        {
            studentAcademicProgramSubmissions.Disciplines.FirstOrDefault().StartOn = DateTime.Parse("01/04/2018");
            studentAcademicProgramSubmissions.Disciplines.FirstOrDefault().EndOn = DateTime.Parse("01/01/2018");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "The requested discipline 1a59eed8-5fe7-4120-b1cf-f23266b9e874 endOn must be on or after the discipline startOn.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Null_AdmissionClassification_AdmissionCategory()
        {
            studentAcademicProgramSubmissions.AdmissionClassification.AdmissionCategory = null; ;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AdmissionClassification.admissionCategory is a required property when admissionClassification is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Null_AdmissionClassification_AdmissionCategory_Id()
        {
            studentAcademicProgramSubmissions.AdmissionClassification.AdmissionCategory.Id = string.Empty ;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AdmissionClassification.admissionCategory.id is a required property when admissionClassification.admissionCategory is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync_Null_AdmissionClassification_AdmissionCategory_Invalid_Id()
        {
            studentAcademicProgramSubmissions.AdmissionClassification.AdmissionCategory.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AdmissionClassification.admissionCategory.id is not a valid GUID for admission-populations.");
                throw ex;
            }
        }



        [TestMethod]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync()
        {
            var result = await studentAcademicProgramService.CreateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions, true);

            Assert.IsNotNull(result);
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task StuAcadPrgm_UpdateStudentAcademicProgram2Async_PermissionException()
        //{
        //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
        //    await studentAcademicProgramService.UpdateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions);
        //}

        [TestMethod]
        public async Task StuAcadPrgm_UpdateStudentAcademicProgram2Async()
        {
            studentAcademicProgramSubmissions.Disciplines.FirstOrDefault().Discipline.Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e876";
            studentAcademicProgramSubmissions.EnrollmentStatus.Detail = new GuidObject2(guid);
            var result = await studentAcademicProgramService.UpdateStudentAcademicProgramSubmissionAsync(studentAcademicProgramSubmissions, true);

            Assert.IsNotNull(result);
        }

        #endregion
    }

    [TestClass]
    public class StudentAcademicProgramReplacementsTests : StudentUserSetup
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
        private List<Domain.Student.Entities.AdmissionPopulation> admissionPopulations;
        private List<Domain.Base.Entities.OtherDegree> otherDegrees;
        private List<Domain.Base.Entities.OtherMajor> otherMajors;
        private List<Domain.Base.Entities.OtherHonor> otherHonors;
        private List<Domain.Student.Entities.EnrollmentStatus> enrollmentStatuses;
        private List<Domain.Base.Entities.AcadCredential> academicCredentials;
        private List<Domain.Base.Entities.AcademicDiscipline> disciplines;

        private StudentAcademicProgramReplacements studentAcademicProgramReplacements;

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

            academicPrograms = new List<Domain.Student.Entities.AcademicProgram>() { new Domain.Student.Entities.AcademicProgram(guid, "1", "desc"), new Domain.Student.Entities.AcademicProgram(guid, "2", "2desc") };

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

            admissionPopulations = new List<Domain.Student.Entities.AdmissionPopulation>() { new Domain.Student.Entities.AdmissionPopulation(guid, "1", "desc") };
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

            studentAcademicProgramEntity = new StudentAcademicProgram("1", "2", "1", guid, DateTime.Today, "1")
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
                CreditsEarned = 0,
                AdmitStatus = "1"
            };

            studentAcademicProgramEntity.AddMajors("1");
            studentAcademicProgramEntity.AddHonors("1");

            studentAcademicPrograms = new Tuple<IEnumerable<StudentAcademicProgram>, int>(new List<StudentAcademicProgram>() { studentAcademicProgramEntity }, 1);

            studentAcademicProgramReplacements = new StudentAcademicProgramReplacements()
            {
                Id = guid,
                Student = new GuidObject2(guid),
                ProgramToReplace  = new GuidObject2(Guid.NewGuid().ToString()),
                NewProgram = new StudentAcademicProgramsReplacementsNewProgram()
                {
                    CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Matriculated,
                    Detail = new GuidObject2(guid),
                    AcademicCatalog = new GuidObject2(guid),
                    EnrollmentStatus = new EnrollmentStatusDetail() { EnrollStatus = Dtos.EnrollmentStatusType.Active },
                    ProgramOwner = new GuidObject2(guid),
                    Site = new GuidObject2(guid),
                    AcademicLevel = new GuidObject2(guid),
                    ExpectedGraduationDate = DateTime.Today.AddDays(100),
                    StartOn = DateTime.Today,
                    AcademicPeriods = new StudentAcademicProgramsAcademicPeriods2()
                    {
                        Starting = new GuidObject2(guid),
                        ExpectedGraduation = new GuidObject2(guid)
                    },
                    Credentials = new List<GuidObject2>() { new GuidObject2(guid), new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e875") },
                    Disciplines = new List<StudentAcademicProgramDisciplines2>()
                    {
                        new StudentAcademicProgramDisciplines2()
                        {
                            Discipline = new GuidObject2(guid),
                            AdministeringInstitutionUnit = new GuidObject2(guid),
                            StartOn = DateTime.Today,
                        }
                    },
                    AdmissionClassification = new Dtos.DtoProperties.AdmissionClassificationDtoProperty()
                    {
                        AdmissionCategory = new GuidObject2(guid)
                    }
                }
            };
        }

        private void InitializeMock(bool bypassCache = true)
        {
            createStudentAcademicPrograms.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ReplaceStudentAcademicProgram));

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getStudentAcademicPrograms, createStudentAcademicPrograms });
            configurationRepositoryMock.Setup(c => c.GetDefaultsConfiguration()).Returns(defaultConfiguration);
            studentAcademicProgramRepositoryMock.Setup(s => s.GetStudentAcademicProgramByGuid2Async(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentAcademicProgramEntity);
            studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicPrograms);
            catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ReturnsAsync(catalogs);
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);

            referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);
            referenceDataRepositoryMock.Setup(r => r.GetDepartments2Async(It.IsAny<bool>())).ReturnsAsync(departments);
            referenceDataRepositoryMock.Setup(r => r.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(academicCredentials);
            termRepositoryMock.Setup(t => t.GetAsync(It.IsAny<bool>())).ReturnsAsync(terms);
            termRepositoryMock.Setup(t => t.GetAcademicPeriods(It.IsAny<List<Term>>())).Returns(academicPeriods);
            studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);
            studentReferenceDataRepositoryMock.Setup(s => s.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulations);
            referenceDataRepositoryMock.Setup(r => r.GetOtherDegreesAsync(It.IsAny<bool>())).ReturnsAsync(otherDegrees);
            referenceDataRepositoryMock.Setup(r => r.GetOtherMajorsAsync(It.IsAny<bool>())).ReturnsAsync(otherMajors);
            referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(otherHonors);
            referenceDataRepositoryMock.Setup(r => r.GetOtherHonorsAsync(false)).ReturnsAsync(otherHonors);
            studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatuses);
            studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatuses);
            studentAcademicProgramRepositoryMock.Setup(s => s.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync(DateTime.Today.ToString());
            referenceDataRepositoryMock.Setup(r => r.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(disciplines);

            studentAcademicProgramRepositoryMock.Setup(s => s.CreateStudentAcademicProgram2Async(It.IsAny<StudentAcademicProgram>(), It.IsAny<string>())).ReturnsAsync(studentAcademicProgramEntity);
            foreach (var acadProg in academicPrograms)
            {
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(acadProg.Code)).ReturnsAsync(acadProg.Guid);
            }
            foreach (var catalog in catalogs)
            {
                catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(catalog.Code)).ReturnsAsync(catalog.Guid);
            }
            foreach (var location in locations)
            {
                referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(location.Code)).ReturnsAsync(location.Guid);
            }
            foreach (var dept in departments)
            {
                referenceDataRepositoryMock.Setup(d => d.GetDepartments2GuidAsync(dept.Code)).ReturnsAsync(dept.Guid);
            }
            foreach (var academicPeriod in academicPeriods)
            {
                termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(academicPeriod.Code)).ReturnsAsync(academicPeriod.Guid);
            }
            foreach (var acadLevel in academicLevels)
            {
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(acadLevel.Code)).ReturnsAsync(acadLevel.Guid);
            }
            foreach (var admissionPopulation in admissionPopulations)
            {
                studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAdmissionPopulationsGuidAsync(admissionPopulation.Code)).ReturnsAsync(admissionPopulation.Guid);
            }
            foreach (var honor in otherHonors)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(honor.Code)).ReturnsAsync(honor.Guid);
            }
            foreach (var degree in otherDegrees)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(degree.Code)).ReturnsAsync(degree.Guid);
            }
            foreach (var major in otherMajors)
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(major.Code)).ReturnsAsync(major.Guid);
            }
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicPrograms.FirstOrDefault().Guid);
            catalogRepositoryMock.Setup(cat => cat.GetCatalogGuidAsync(It.IsAny<string>())).ReturnsAsync(catalogs.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(loc => loc.GetLocationsGuidAsync(It.IsAny<string>())).ReturnsAsync(locations.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(dept => dept.GetDepartments2GuidAsync(It.IsAny<string>())).ReturnsAsync(departments.FirstOrDefault().Guid);
            termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicPeriods.FirstOrDefault().Guid);
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicLevelsGuidAsync(It.IsAny<string>())).ReturnsAsync(academicLevels.FirstOrDefault().Guid);
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAdmissionPopulationsGuidAsync(It.IsAny<string>())).ReturnsAsync(admissionPopulations.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsGuidAsync(It.IsAny<string>())).ReturnsAsync(otherHonors.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(repo => repo.GetOtherDegreeGuidAsync(It.IsAny<string>())).ReturnsAsync(otherDegrees.FirstOrDefault().Guid);
            referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsGuidAsync(It.IsAny<string>())).ReturnsAsync(otherMajors.FirstOrDefault().Guid);
        }

        #endregion
       
        #region PUT/POST
        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_PermissionException()
        //{
        //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
        //    await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
        //}



        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_null_Dto()
        {
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(null, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Must provide a StudentAcademicProgramsReplacements object for update");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_null_ProgramToReplace()
        {
            studentAcademicProgramReplacements.ProgramToReplace = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ProgramToReplace.id is a required property");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_student_Empty()
        {
            studentAcademicProgramReplacements.Student.Id = string.Empty;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is a required property");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_NewProgram_Null()
        {
            studentAcademicProgramReplacements.NewProgram = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "NewProgram.id is a required property");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_CurriculumObjective_Not_Matriculated()
        {
            studentAcademicProgramReplacements.NewProgram.CurriculumObjective = Dtos.EnumProperties.StudentAcademicProgramsCurriculumObjective2.Outcome;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "CurriculumObjective must be set to 'matriculated' for any new or updated student programs.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_StartOn_Null()
        {
            studentAcademicProgramReplacements.NewProgram.StartOn = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "StartOn is required to create or update a matriculated student program.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsyncAsync_EnrollmentStatus()
        {
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EnrollmentStatus is a required property");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_StartOn_Before_EndOn()
        {
            studentAcademicProgramReplacements.NewProgram.StartOn = DateTime.Parse("01/06/2018");
            studentAcademicProgramReplacements.NewProgram.EndOn = DateTime.Parse("01/04/2018");
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EndOn cannot be before startOn.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_StartOn_Before_ExpectedGraduationDate()
        {
            studentAcademicProgramReplacements.NewProgram.StartOn = DateTime.Parse("01/06/2018");
            studentAcademicProgramReplacements.NewProgram.ExpectedGraduationDate = DateTime.Parse("01/04/2018");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ExpectedGraduationDate cannot be before startOn.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_EnrollmentStatus_InActive_No_EndOn()
        {
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Inactive;
            studentAcademicProgramReplacements.NewProgram.EndOn = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EndOn is required for the enrollment status of inactive.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_EnrollmentStatus_Complete()
        {
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Complete;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Enrollment status of complete is not supported. Graduation processing can only be invoked directly in Colleague.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_EnrollmentStatus_Active_EndOn()
        {
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus.EnrollStatus = Dtos.EnrollmentStatusType.Active;
            studentAcademicProgramReplacements.NewProgram.EndOn = DateTime.Parse("01/04/2018");
            studentAcademicProgramReplacements.NewProgram.StartOn = DateTime.Parse("01/01/2018");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EndOn is not valid for the enrollment status of active.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Credentials_Id_Null()
        {
            studentAcademicProgramReplacements.NewProgram.Credentials[0].Id = string.Empty;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credential id is a required field when credentials are in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Disciplines_Discipline_Null()
        {
            studentAcademicProgramReplacements.NewProgram.Disciplines[0].Discipline = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Discipline is a required property when disciplines are in the message body.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_DisciplineId_As_Null()
        {
            studentAcademicProgramReplacements.NewProgram.Disciplines.FirstOrDefault().Discipline.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Discipline id is a required property when discipline is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_StudentId()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is not a valid GUID for persons.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_StudentId_Exception()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is not a valid GUID for persons.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_StudentId_IsCorp()
        {
            personRepositoryMock.Setup(pr => pr.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(true);
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Student.id is not a valid GUID for persons.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_AcademicProgramId()
        {
            studentAcademicProgramReplacements.NewProgram.Detail.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Program.Id is not a valid GUID for academic-programs.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync__AcademicProgramId_Exception()
        {
            studentReferenceDataRepositoryMock.Setup(srdr => srdr.GetAcademicProgramsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }            
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Program.Id is not a valid GUID for academic-programs.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgram_AcademicCatalogId_As_Null()
        {
            studentAcademicProgramReplacements.NewProgram.AcademicCatalog.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Catalog.id is a required property when catalog is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_AcademicCatalogId()
        {
            catalogRepositoryMock.Setup(c => c.GetAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Catalog.Id is not a valid GUID for academic-catalogs.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_EnrollmentStatusId_Null()
        {
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus.Detail = new GuidObject2(null);
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EnrollmentStatus.Detail.Id is a required field when detail is in the message body");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_EnrollmentStatus_And_Detail_NotMatch()
        {
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus.Detail = new GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e875");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == " The enrollment Status of 'complete' referred by the detail ID '1a59eed8-5fe7-4120-b1cf-f23266b9e875' is different from that in the payload.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_EnrollmentStatusDetailId()
        {
            studentAcademicProgramReplacements.NewProgram.EnrollmentStatus.Detail = new GuidObject2(Guid.NewGuid().ToString());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "EnrollmentStatus.Detail.Id not a valid GUID for enrollment-statuses.");
                throw ex;
            }
        }



        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_EnrollmentStatusDetailId_Exception()
        {
            studentReferenceDataRepositoryMock.Setup(s => s.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ProgramToReplace.Id does not match the program ID for the programToReplace.");
                throw ex;
            }
        }
        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Null_ProgramOwnerId()
        {
            studentAcademicProgramReplacements.NewProgram.ProgramOwner.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ProgramOwner.id is a required property when programOwner is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_ProgramOwnerId()
        {
            studentAcademicProgramReplacements.NewProgram.ProgramOwner.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "ProgramOwner.id is not a valid GUID for educational-institution-units.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_SiteId_Null()
        {
            studentAcademicProgramReplacements.NewProgram.Site.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Site.id is a required property when site is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_SiteId()
        {
            studentAcademicProgramReplacements.NewProgram.Site.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Site.id is not a valid GUID for sites.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_AcademicLevelId_Null()
        {
            studentAcademicProgramReplacements.NewProgram.AcademicLevel.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicLevel.id is a required property when site is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_AcademicLevelId()
        {
            studentAcademicProgramReplacements.NewProgram.AcademicLevel.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicLevel.id is not a valid GUID for academic-levels.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_AcademicPeriodsStartingId_Null()
        {
            studentAcademicProgramReplacements.NewProgram.AcademicPeriods.Starting.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.starting.id is a required property when academicPeriods.starting is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_AcademicPeriodsStartingId()
        {
            studentAcademicProgramReplacements.NewProgram.AcademicPeriods.Starting.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.starting.id is not a valid GUID for academic-periods.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_AcademicPeriodsExpectedGraduationId_Null()
        {
            studentAcademicProgramReplacements.NewProgram.AcademicPeriods.ExpectedGraduation.Id = null;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.expectedGraduation.id is a required property when academicPeriods.expectedGraduation is in the message body.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_AcademicPeriodsExpectedGraduationId()
        {
            studentAcademicProgramReplacements.NewProgram.AcademicPeriods.ExpectedGraduation.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AcademicPeriods.expectedGraduation.id is not a valid GUID for academic-periods.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_CredentialId()
        {
            studentAcademicProgramReplacements.NewProgram.Credentials.FirstOrDefault().Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials.id is not a valid GUID for academic-credentials.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Credentials_As_Honorary()
        {
            studentAcademicProgramReplacements.NewProgram.Credentials.FirstOrDefault().Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e876";
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials.id of type honor is not supported.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Credentials_As_Diploma()
        {
            studentAcademicProgramReplacements.NewProgram.Credentials.FirstOrDefault().Id = "1a59eed8-5fe7-4120-b1cf-f23266b9e877";
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials.id of type diploma is not supported.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_CredentialsDegree_MoreThanOne()
        {
            studentAcademicProgramReplacements.NewProgram.Credentials.Add(new GuidObject2(guid));
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Credentials array cannot have more than one degree.");
                throw ex;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Invalid_DisciplineId()
        {
            studentAcademicProgramReplacements.NewProgram.Disciplines.FirstOrDefault().Discipline.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Disciplines.discipline.id is not a valid GUID for academic-disciplines.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Discipline_StartOn_Endon()
        {
            studentAcademicProgramReplacements.NewProgram.Disciplines.FirstOrDefault().StartOn = DateTime.Parse("01/04/2018");
            studentAcademicProgramReplacements.NewProgram.Disciplines.FirstOrDefault().EndOn = DateTime.Parse("01/01/2018");
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "The requested discipline 1a59eed8-5fe7-4120-b1cf-f23266b9e874 endOn must be on or after the discipline startOn.");
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Null_AdmissionClassification_AdmissionCategory()
        {
            studentAcademicProgramReplacements.NewProgram.AdmissionClassification.AdmissionCategory = null; ;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AdmissionClassification.admissionCategory is a required property when admissionClassification is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Null_AdmissionClassification_AdmissionCategory_Id()
        {
            studentAcademicProgramReplacements.NewProgram.AdmissionClassification.AdmissionCategory.Id = string.Empty;
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AdmissionClassification.admissionCategory.id is a required property when admissionClassification.admissionCategory is in the message body.");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramReplacementsAsync_Null_AdmissionClassification_AdmissionCategory_Invalid_Id()
        {
            studentAcademicProgramReplacements.NewProgram.AdmissionClassification.AdmissionCategory.Id = Guid.NewGuid().ToString();
            try
            {
                await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "AdmissionClassification.admissionCategory.id is not a valid GUID for admission-populations.");
                throw ex;
            }
        }

        [TestMethod]
        public async Task StuAcadPrgm_CreateStudentAcademicProgramSubmissionAsync()
        {
            var result = await studentAcademicProgramService.CreateStudentAcademicProgramReplacementsAsync(studentAcademicProgramReplacements, true);

            Assert.IsNotNull(result);
        }

        #endregion
    }
}