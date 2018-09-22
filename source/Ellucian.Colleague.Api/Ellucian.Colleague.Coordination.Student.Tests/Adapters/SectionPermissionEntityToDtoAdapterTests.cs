using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class SectionPermissionEntityToDtoAdapterTests
    {
        private Ellucian.Colleague.Domain.Student.Entities.SectionPermission sectionPermission;

        [TestInitialize]
        public void Initialize() 
        {
            sectionPermission = new Ellucian.Colleague.Domain.Student.Entities.SectionPermission("Section1");

            var studentPetition1 = new Ellucian.Colleague.Domain.Student.Entities.StudentPetition("1", null, "Section1", "Student1", StudentPetitionType.StudentPetition, "A") { Comment = "Comment1", ReasonCode = "Reason1" };
            studentPetition1.DateTimeChanged = DateTime.Today.AddDays(-1);
            studentPetition1.StartDate = DateTime.Today.AddDays(5);
            studentPetition1.EndDate = DateTime.Today.AddDays(10);
            studentPetition1.UpdatedBy = "Faculty1";
            var studentPetition2 = new Ellucian.Colleague.Domain.Student.Entities.StudentPetition("2", null, "Section1", "Student2", StudentPetitionType.StudentPetition, "B") { Comment = "Comment2", ReasonCode = "Reason2" }; ;
            var studentPetition3 = new Ellucian.Colleague.Domain.Student.Entities.StudentPetition("3", null, "Section1", "Student3", StudentPetitionType.StudentPetition, "C") { Comment = "Comment3", ReasonCode = "Reason3" };
            sectionPermission.AddStudentPetition(studentPetition1);
            sectionPermission.AddStudentPetition(studentPetition2);
            sectionPermission.AddStudentPetition(studentPetition3);

            var facultyConsent1 = new Ellucian.Colleague.Domain.Student.Entities.StudentPetition("1", null, "Section1", "Student4", StudentPetitionType.FacultyConsent, "D") { Comment = "Comment4", ReasonCode = "Reason4" };
            facultyConsent1.DateTimeChanged = DateTime.Today.AddDays(-10);
            facultyConsent1.StartDate = DateTime.Today.AddDays(50);
            facultyConsent1.EndDate = DateTime.Today.AddDays(100);
            facultyConsent1.UpdatedBy = "Faculty2";
            var facultyConsent2 = new Ellucian.Colleague.Domain.Student.Entities.StudentPetition("2", null, "Section1", "Student5", StudentPetitionType.FacultyConsent, "E") { Comment = "Comment5", ReasonCode = "Reason5" }; ;
            sectionPermission.AddFacultyConsent(facultyConsent1);
            sectionPermission.AddFacultyConsent(facultyConsent2);
        }

        [TestMethod]
        public void SectionPermissionEntityToDtoAdapter_MapToType()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var adapterRegistry = adapterRegistryMock.Object;
            var loggerMock = new Mock<ILogger>();            
            var sectionPermissionEntitytoDtoAdapter = new SectionPermissionEntityToDtoAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionPermission, Ellucian.Colleague.Dtos.Student.SectionPermission>()).Returns(sectionPermissionEntitytoDtoAdapter);
            var studentPetitionEntityToDtoAdapter = new StudentPetitionEntityToDtoAdapter(adapterRegistry, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>()).Returns(studentPetitionEntityToDtoAdapter);

            var sectionPermissionDto = sectionPermissionEntitytoDtoAdapter.MapToType(sectionPermission);
            Assert.AreEqual(sectionPermission.SectionId, sectionPermissionDto.SectionId);
            Assert.AreEqual(sectionPermission.FacultyConsents.Count(), sectionPermissionDto.FacultyConsents.Count());
            Assert.AreEqual(sectionPermission.StudentPetitions.Count(), sectionPermissionDto.StudentPetitions.Count());
            
            for (int i = 0; i < (sectionPermission.StudentPetitions.Count() - 1); i++)
            { 
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).Id, sectionPermissionDto.StudentPetitions.ElementAt(i).Id);
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).ReasonCode, sectionPermissionDto.StudentPetitions.ElementAt(i).ReasonCode);
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).SectionId, sectionPermissionDto.StudentPetitions.ElementAt(i).SectionId);
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).StatusCode, sectionPermissionDto.StudentPetitions.ElementAt(i).StatusCode);
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).StudentId, sectionPermissionDto.StudentPetitions.ElementAt(i).StudentId);
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).UpdatedBy, sectionPermissionDto.StudentPetitions.ElementAt(i).UpdatedBy);
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).Comment, sectionPermissionDto.StudentPetitions.ElementAt(i).Comment);
                Assert.AreEqual(sectionPermission.StudentPetitions.ElementAt(i).DateTimeChanged, sectionPermissionDto.StudentPetitions.ElementAt(i).DateTimeChanged);
                Assert.AreEqual(Ellucian.Colleague.Dtos.Student.StudentPetitionType.StudentPetition, sectionPermissionDto.StudentPetitions.ElementAt(i).Type);
            }

            for (int i = 0; i < (sectionPermission.FacultyConsents.Count() - 1); i++)
            {
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).Id, sectionPermissionDto.FacultyConsents.ElementAt(i).Id);
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).ReasonCode, sectionPermissionDto.FacultyConsents.ElementAt(i).ReasonCode);
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).SectionId, sectionPermissionDto.FacultyConsents.ElementAt(i).SectionId);
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).StatusCode, sectionPermissionDto.FacultyConsents.ElementAt(i).StatusCode);
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).StudentId, sectionPermissionDto.FacultyConsents.ElementAt(i).StudentId);
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).UpdatedBy, sectionPermissionDto.FacultyConsents.ElementAt(i).UpdatedBy);
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).Comment, sectionPermissionDto.FacultyConsents.ElementAt(i).Comment);
                Assert.AreEqual(sectionPermission.FacultyConsents.ElementAt(i).DateTimeChanged, sectionPermissionDto.FacultyConsents.ElementAt(i).DateTimeChanged);
                Assert.AreEqual(Ellucian.Colleague.Dtos.Student.StudentPetitionType.FacultyConsent, sectionPermissionDto.FacultyConsents.ElementAt(i).Type);
            }

        }

    }
}
