// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class StudentSectionsAttendancesEntityToDtoAdapterTests
    {
        StudentSectionsAttendances entity;
        StudentSectionsAttendances emptyEntity;
        StudentSectionsAttendances entityWithOnlyOneValue;
        StudentSectionsAttendancesEntityAdapter adapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapter = new StudentSectionsAttendancesEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            var AttendanceDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>()).Returns(AttendanceDtoAdapter);

            entity = new StudentSectionsAttendances("0001234");
            List<StudentAttendance> attendances = new List<StudentAttendance>();

            attendances.Add(new StudentAttendance("0001234", "1111", new DateTime(2018, 01, 01), "P", null, "good job")
            {
                EndTime = DateTime.Now.AddHours(-1),
                StartTime = DateTime.Now.AddHours(-3),
                InstructionalMethod = "LAB",
                StudentCourseSectionId = "67890"
            });

            attendances.Add(new StudentAttendance("0001234", "1111", new DateTime(2018, 01, 02), "A", null, "")
            {
                EndTime = DateTime.Now.AddHours(-1),
                StartTime = DateTime.Now.AddHours(-3),
                InstructionalMethod = "LEC",
                StudentCourseSectionId = "67890"
            });
            attendances.Add(new StudentAttendance("0001234", "1112", new DateTime(2018, 01, 01), null, 23, "good job")
            {
                EndTime = DateTime.Now.AddHours(-1),
                StartTime = DateTime.Now.AddHours(-3),
                InstructionalMethod = "LAB",
                StudentCourseSectionId = "67891"
            });
            attendances.Add(new StudentAttendance("0001234", "1112", new DateTime(2018, 01, 02), null, 12, "come regularly")
            {
                EndTime = DateTime.Now.AddHours(-1),
                StartTime = DateTime.Now.AddHours(-3),
                InstructionalMethod = "LEC",
                StudentCourseSectionId = "67891"
            });
            entity.AddStudentAttendances(attendances);

            //empty entity declaration

            emptyEntity = new StudentSectionsAttendances("0001234");
            List<StudentAttendance> attendances2 = new List<StudentAttendance>();

            //entity with only one value
            entityWithOnlyOneValue = new StudentSectionsAttendances("0001234");
            List<StudentAttendance> attendances3 = new List<StudentAttendance>();

            attendances3.Add(new StudentAttendance("0001234", "1111", new DateTime(2018, 01, 01), "P", null, "good job")
            {
                EndTime = DateTime.Now.AddHours(-1),
                StartTime = DateTime.Now.AddHours(-3),
                InstructionalMethod = "LAB",
                StudentCourseSectionId = "67890"
            });


            attendances3.Add(new StudentAttendance("0001234", "1112", new DateTime(2018, 01, 01), null, 23, "good job")
            {
                EndTime = DateTime.Now.AddHours(-1),
                StartTime = DateTime.Now.AddHours(-3),
                InstructionalMethod = "LAB",
                StudentCourseSectionId = "67891"
            });

            entityWithOnlyOneValue.AddStudentAttendances(attendances3);

           
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentAttendanceDtoToEntityAdapter_Null_source_throws_Exception()
        {
            var entity = adapter.MapToType(null);
        }

        [TestMethod]
        public void StudentAttendanceDtoToEntityAdapter_Validate_Mapping()
        {
            var dto = adapter.MapToType(entity);
            Assert.AreEqual(dto.StudentId, entity.StudentId);
            Assert.AreEqual(dto.SectionWiseAttendances.Count, entity.SectionWiseAttendances.Count);
            //validate each section in dictionary
            foreach (var sectionAttendance in dto.SectionWiseAttendances)
            {
                Assert.AreEqual(sectionAttendance.Value.Count, entity.SectionWiseAttendances[sectionAttendance.Key].Count);
                int i = 0;

                foreach (var sectionAttendanceDto in sectionAttendance.Value)
                {
                    List<Domain.Student.Entities.StudentAttendance> attendaceEntities = entity.SectionWiseAttendances[sectionAttendance.Key];
                    Assert.AreEqual(sectionAttendanceDto.EndTime, attendaceEntities[i].EndTime);
                    Assert.AreEqual(sectionAttendanceDto.StartTime, attendaceEntities[i].StartTime);
                    Assert.AreEqual(sectionAttendanceDto.InstructionalMethod, attendaceEntities[i].InstructionalMethod);
                    Assert.AreEqual(sectionAttendanceDto.MeetingDate, attendaceEntities[i].MeetingDate);
                    Assert.AreEqual(sectionAttendanceDto.SectionId, attendaceEntities[i].SectionId);
                    Assert.AreEqual(sectionAttendanceDto.StudentCourseSectionId, attendaceEntities[i].StudentCourseSectionId);
                    Assert.AreEqual(sectionAttendanceDto.StudentId, attendaceEntities[i].StudentId);
                    Assert.AreEqual(sectionAttendanceDto.MinutesAttended, attendaceEntities[i].MinutesAttended);
                    i = i + 1;
                }


            }
        }


        [TestMethod]
        public void StudentAttendanceDtoToEntityAdapter_Validate_Dictiona_Key_Values_AreEmpty()
        {
            var dto = adapter.MapToType(emptyEntity);
            Assert.AreEqual(dto.StudentId, emptyEntity.StudentId);
            Assert.AreEqual(dto.SectionWiseAttendances.Count, emptyEntity.SectionWiseAttendances.Count);
            Assert.AreEqual(dto.SectionWiseAttendances.Count, 0);
            foreach (var sectionAttendance in dto.SectionWiseAttendances)
            {
                Assert.AreEqual(sectionAttendance.Value.Count, emptyEntity.SectionWiseAttendances[sectionAttendance.Key].Count);
                Assert.AreEqual(sectionAttendance.Value.Count, 0);
                Assert.AreEqual(emptyEntity.SectionWiseAttendances[sectionAttendance.Key].Count, 0);
               
            }
        }

        [TestMethod]
        public void StudentAttendanceDtoToEntityAdapter_Validate_Dictiona_Key_WithSingleValues()
        {
            var dto = adapter.MapToType(entityWithOnlyOneValue);
            Assert.AreEqual(dto.StudentId, entityWithOnlyOneValue.StudentId);
            Assert.AreEqual(entityWithOnlyOneValue.SectionWiseAttendances.Count,dto.SectionWiseAttendances.Count );
            Assert.AreEqual(2,dto.SectionWiseAttendances.Count);
            foreach (var sectionAttendance in dto.SectionWiseAttendances)
            {
                Assert.AreEqual(sectionAttendance.Value.Count, entityWithOnlyOneValue.SectionWiseAttendances[sectionAttendance.Key].Count);
                Assert.AreEqual(sectionAttendance.Value.Count,1);
                Assert.AreEqual(entityWithOnlyOneValue.SectionWiseAttendances[sectionAttendance.Key].Count, 1);

            }
        }

    }
}
