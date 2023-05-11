// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using slf4net;
using Moq;

namespace Ellucian.Colleague.Domain.Student.Tests.Services
{
    [TestClass]
    public class SectionProcessorTests
    {
        [TestClass]
        public class SectionProcessor_CalculateMeetingLoadFactor
        {
            private Section sec;
            private string dept;
            private string acadLevel;
            private List<string> courseLevels = new List<string>();
            private string course1;
            private string number;
            private string title;
            private DateTime startDate;
            private DateTime endDate;
            private List<OfferingDepartment> depts;
            private List<SectionStatusItem> statuses;
            private Dictionary<string, decimal> instrMethodLoads;

            [TestInitialize]
            public void Initialize()
            {
                dept = "CS";
                acadLevel = "UG";
                courseLevels = new List<string>() { "100" };
                depts = new List<OfferingDepartment>() { new OfferingDepartment(dept, 100m) };
                statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-4)) };
                course1 = "1";
                number = "01";
                startDate = DateTime.Today.AddDays(-30);
                endDate = DateTime.Today.AddDays(30);
                title = "Title";

                // SectionCapacity but no students and no reserved seats.
                sec = new Section("1", course1, number, startDate, 4m, 1m, title, "IN", depts, courseLevels, acadLevel, statuses);

                // Instr method load dictionary
                instrMethodLoads = new Dictionary<string, decimal>();
                instrMethodLoads["LEC"] = 20m;
                instrMethodLoads["LAB"] = 10m;

            }

            [TestMethod]
            public void SectionProcessor_CalculateMeetingLoadFactor_SingleMeetingSingleFaculty()
            {
                // Arrange - Set up response data
                // Single section meeting
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W") { TotalMeetingMinutes = 1350 };
                sec.AddSectionMeeting(secMeeting1);
                // Single faculty
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 100m);
                secMeeting1.AddSectionFaculty(secFaculty1);

                // Act - get the section
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert - verify load properly calculated and updated
                Assert.AreEqual(1, sec.Meetings.Count());
                Assert.AreEqual("LEC", sec.Meetings[0].FacultyRoster[0].InstructionalMethodCode);
                Assert.AreEqual(20.00m, sec.Meetings[0].FacultyRoster[0].MeetingLoadFactor);
            }

            [TestMethod]
            public void SectionProcessor_CalculateMeetingLoadFactor_MultiMeeting_OneInstrMethodPerFaculty_ByPercent()
            {
                // Arrange
                // Add section meeting for LEC 
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W") { TotalMeetingMinutes = 1350 };
                sec.AddSectionMeeting(secMeeting1);
                // Add faculty to section meeting
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 100m);
                secMeeting1.AddSectionFaculty(secFaculty1);

                // Add section meeting for LAB
                var secMeeting2 = new SectionMeeting("M2", "1", "LAB", startDate, endDate, "W") { TotalMeetingMinutes = 1620 };
                sec.AddSectionMeeting(secMeeting2);
                // Add faculty to section meeting
                var secFaculty2 = new SectionFaculty("SF2", "1", "FAC2", "LAB", startDate, endDate, 100m);
                secMeeting2.AddSectionFaculty(secFaculty2);

                // Act - get the section
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert - verify load properly calculated and updated
                Assert.AreEqual(2, sec.Meetings.Count);
                // Meeting with Instr Method LEC, Faculty FAC1
                var sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC").SelectMany(m => m.FacultyRoster.Where(fr => fr.FacultyId == "FAC1"));
                Assert.AreEqual(1, sectionFaculty.Count());
                Assert.AreEqual(20.00m, sectionFaculty.ElementAt(0).MeetingLoadFactor);
                // Meeting with instr method LAB, Faculty FAC2
                sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LAB").SelectMany(m => m.FacultyRoster.Where(fr => fr.FacultyId == "FAC2"));
                Assert.AreEqual(10.00m, sectionFaculty.ElementAt(0).MeetingLoadFactor);
            }

            [TestMethod]
            public void SectionProcessor_CalculateMeetingLoadFactor_MultiMeeting_OneFacultyPerInstrMethod_ByLoad()
            {
                // Arrange
                // Add section meeting for LEC 
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W");
                sec.AddSectionMeeting(secMeeting1);
                // Add faculty to section meeting
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 100m) { LoadFactor = 25m };
                secMeeting1.AddSectionFaculty(secFaculty1);

                // Add section meeting for LAB
                var secMeeting2 = new SectionMeeting("M2", "1", "LAB", startDate, endDate, "W");
                sec.AddSectionMeeting(secMeeting2);
                // Add faculty to section meeting
                var secFaculty2 = new SectionFaculty("SF2", "1", "FAC2", "LAB", startDate, endDate, 100m) { LoadFactor = 5m };
                secMeeting2.AddSectionFaculty(secFaculty2);

                // Act - get the section
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert - verify load properly calculated and updated
                Assert.IsTrue(sec.Meetings.Count() == 2);
                // Verify one faculty entry for instr method LEC
                // Meeting with Instr Method LEC, Faculty FAC1
                var sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC").SelectMany(m => m.FacultyRoster);
                Assert.AreEqual(1, sectionFaculty.Count());
                Assert.AreEqual(25.00m, sectionFaculty.ElementAt(0).LoadFactor);
                // Meeting with instr method LAB, Faculty FAC2
                sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LAB").SelectMany(m => m.FacultyRoster.Where(fr => fr.FacultyId == "FAC2"));
                Assert.AreEqual(5.00m, sectionFaculty.ElementAt(0).LoadFactor);
            }

            [TestMethod]
            public void SectionProcessor_CalculateMeetingLoadFactor_OneMeetingPerInstrMethod_MultiFacultyPerInstrMethod()
            {
                // Arrange 
                // Add section meeting for LEC 
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W") { TotalMeetingMinutes = 1350 };
                sec.AddSectionMeeting(secMeeting1);
                // Add faculty FAC1 to section meeting
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 40m);
                secMeeting1.AddSectionFaculty(secFaculty1);
                // Add faculty FAC2 to section meeting
                var secFaculty3 = new SectionFaculty("SF1", "1", "FAC2", "LEC", startDate, endDate, 60m);
                secMeeting1.AddSectionFaculty(secFaculty3);

                // Add section meeting for LAB
                var secMeeting2 = new SectionMeeting("M2", "1", "LAB", startDate, endDate, "W") { TotalMeetingMinutes = 1600 };
                sec.AddSectionMeeting(secMeeting2);
                // Add faculty to section meeting
                var secFaculty2 = new SectionFaculty("SF2", "1", "FAC2", "LAB", startDate, endDate, 90m);
                secMeeting2.AddSectionFaculty(secFaculty2);

                // Act - get the section
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert - verify load properly calculated and updated
                Assert.IsTrue(sec.Meetings.Count() == 2);
                // Two faculty associated with LEC meeting
                var sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC").SelectMany(m => m.FacultyRoster);
                Assert.AreEqual(2, sectionFaculty.Count());
                // FAC1 Load = faculty pct (40%) * total LEC load (20) = 8
                var secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(8m, secFac1.ElementAt(0).MeetingLoadFactor);
                // FAC2 load = faculty pct (60%) * total LEC load (20) = 12
                var secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(12m, secFac2.ElementAt(0).MeetingLoadFactor);
                // Meeting with instr method LAB, Faculty FAC2, load = faculty pct (90%) * 10 = 9
                sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LAB").SelectMany(m => m.FacultyRoster.Where(fr => fr.FacultyId == "FAC2"));
                Assert.AreEqual(9m, sectionFaculty.ElementAt(0).MeetingLoadFactor);
            }

            [TestMethod]
            public void SectionRepository_CalculateMeetingLoadFactor_MultiMeetingPerInstrMethod_MultiFacultyPerInstrMethod()
            {
                // Arrange 
                // set "LEC" instr method total load to 30
                instrMethodLoads["LEC"] = 30m;

                // Add first section meeting for LEC 
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W") { TotalMeetingMinutes = 675 };
                sec.AddSectionMeeting(secMeeting1);
                // Add faculty FAC1 and FAC2 to section meeting1
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 60m);
                secMeeting1.AddSectionFaculty(secFaculty1);
                var secFaculty2 = new SectionFaculty("SF2", "1", "FAC2", "LEC", startDate, endDate, 40m);
                secMeeting1.AddSectionFaculty(secFaculty2);

                // Add second section meeting for LEC 
                var secMeeting2 = new SectionMeeting("M2", "1", "LEC", startDate, endDate, "W") { TotalMeetingMinutes = 675 };
                sec.AddSectionMeeting(secMeeting2);
                // Add faculty FAC1 and FAC2 to section meeting2
                secMeeting2.AddSectionFaculty(secFaculty1);
                secMeeting2.AddSectionFaculty(secFaculty2);

                // Act
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert
                // Section meetings
                Assert.IsTrue(sec.Meetings.Count() == 2);
                // Both section meetings are LEC
                var sectionMeetings = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC");
                Assert.AreEqual(2, sectionMeetings.Count());
                // Sec Meeting 1: two faculty
                var sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M1").SelectMany(m => m.FacultyRoster);
                Assert.AreEqual(2, sectionFaculty.Count());
                // FAC1 Load = faculty pct (60%) * total LEC load (30) / 2 meetings = 9
                var secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(9m, secFac1.ElementAt(0).MeetingLoadFactor);
                // FAC2 load = faculty pct (40%) * total LEC load (30) / 2 meetings = 6
                var secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(6m, secFac2.ElementAt(0).MeetingLoadFactor);
                // Sec Meeting 2: two faculty
                // FAC1 Load = faculty pct (60%) * total LEC load (30) / 2 meetings = 9
                sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M2").SelectMany(m => m.FacultyRoster);
                secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(9m, secFac1.ElementAt(0).MeetingLoadFactor);
                // FAC2 load = faculty pct (40%) * total LEC load (30) / 2 meetings = 6
                secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(6m, secFac2.ElementAt(0).MeetingLoadFactor);
            }

            [TestMethod]
            public void SectionProcessor_CalculateMeetingLoadFactor_MultiMeetingPerInstrMethod_MultiFacultyPerInstrMethod_ByLoad()
            {
                // Arrange 
                // Update overall instr method load
                instrMethodLoads["LEC"] = 30m;
                instrMethodLoads["LAB"] = 20m;

                // Add first section meeting for LEC 
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W") { TotalMeetingMinutes = 675 };
                sec.AddSectionMeeting(secMeeting1);
                // Add faculty FAC1 and FAC2 to section meeting1
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 60m) { LoadFactor = 1m };
                secMeeting1.AddSectionFaculty(secFaculty1);
                var secFaculty2 = new SectionFaculty("SF2", "1", "FAC2", "LEC", startDate, endDate, 40m) { LoadFactor = 0m };
                secMeeting1.AddSectionFaculty(secFaculty2);

                // Add second section meeting for LEC 
                var secMeeting2 = new SectionMeeting("M2", "1", "LEC", startDate, endDate, "W") { TotalMeetingMinutes = 675 };
                sec.AddSectionMeeting(secMeeting2);
                // Add faculty FAC1 and FAC2 to section meeting2
                secMeeting2.AddSectionFaculty(secFaculty1);
                secMeeting2.AddSectionFaculty(secFaculty2);

                // Act
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert
                // Section meetings
                Assert.IsTrue(sec.Meetings.Count() == 2);
                // Both section meetings are LEC
                var sectionMeetings = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC");
                Assert.AreEqual(2, sectionMeetings.Count());
                // Sec Meeting 1: two faculty
                var sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M1").SelectMany(m => m.FacultyRoster);
                Assert.AreEqual(2, sectionFaculty.Count());
                // FAC1 Load = faculty load (1m) / 2 meetings = .5
                var secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(.5m, secFac1.ElementAt(0).MeetingLoadFactor);
                // FAC2 load = faculty load (0m) / 2 meetings = 0
                var secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(0m, secFac2.ElementAt(0).MeetingLoadFactor);
                // Sec Meeting 2: two faculty
                // FAC1 Load = faculty load (1m) / 2 meetings = .5
                sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M2").SelectMany(m => m.FacultyRoster);
                secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(.5m, secFac1.ElementAt(0).MeetingLoadFactor);
                // FAC2 load = faculty load (0m) / 2 meetings = 0
                secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(0, secFac2.ElementAt(0).MeetingLoadFactor);
            }

            [TestMethod]
            public void SectionProcessor_CalculateMeetingLoadFactor_MultiMeetingPerInstrMethod_MultiFacultyPerInstrMethod_ByRelativeHours()
            {
                // Arrange 
                // Update overall instr method load
                instrMethodLoads["LEC"] = 30m;
                instrMethodLoads["LAB"] = 20m;

                // Add first section meeting for LEC 
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                    Frequency = "W",
                    StartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 10, 00, 00),
                    EndTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 11, 00, 00),
                    TotalMeetingMinutes = 200
                };
                sec.AddSectionMeeting(secMeeting1);
                // Add faculty FAC1 and FAC2 to section meeting1
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 50m);
                secMeeting1.AddSectionFaculty(secFaculty1);
                var secFaculty2 = new SectionFaculty("SF2", "1", "FAC2", "LEC", startDate, endDate, 50m);
                secMeeting1.AddSectionFaculty(secFaculty2);

                // Add second section meeting for LEC 
                var secMeeting2 = new SectionMeeting("M2", "1", "LEC", startDate, endDate, "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    Frequency = "W",
                    StartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 10, 00, 00),
                    EndTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 11, 00, 00),
                    TotalMeetingMinutes = 100
                };
                sec.AddSectionMeeting(secMeeting2);
                // Add faculty FAC1 and FAC2 to section meeting2
                var secFaculty3 = new SectionFaculty("SF3", "1", "FAC1", "LEC", startDate, endDate, 50m);
                secMeeting2.AddSectionFaculty(secFaculty3);
                var secFaculty4 = new SectionFaculty("SF4", "1", "FAC2", "LEC", startDate, endDate, 50m);
                secMeeting2.AddSectionFaculty(secFaculty4);

                // Act - get the section
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert
                // Section meetings
                Assert.IsTrue(sec.Meetings.Count() == 2);
                // Both section meetings are LEC
                var sectionMeetings = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC");
                Assert.AreEqual(2, sectionMeetings.Count());
                // Sec Meeting 1: two faculty
                var sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M1").SelectMany(m => m.FacultyRoster);
                Assert.AreEqual(2, sectionFaculty.Count());
                // FAC1 Load = Instr Method load (30) * responsibility 50% * percentage by meeting time (.667) = 10
                var secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(10m, secFac1.ElementAt(0).MeetingLoadFactor);
                // FAC2 load = Instr Method load (30) * responsibility 50% * percentage by meeting time (.667) = 10
                var secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(10m, secFac2.ElementAt(0).MeetingLoadFactor);
                // Sec Meeting 2: two faculty
                // FAC1 Load = Instr Method load (30) * responsibility 50% * percentage by meeting time (.333) = 5 (after rounding)
                sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M2").SelectMany(m => m.FacultyRoster);
                secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(5, secFac1.ElementAt(0).MeetingLoadFactor);
                // FAC2 load = Instr Method load (30) * responsibility 50% * percentage by meeting time (.333) = 5 (after rounding)
                secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(5, secFac2.ElementAt(0).MeetingLoadFactor);
            }

            [TestMethod]
            public void SectionProcessor_CalculateMeetingLoadFactor_MultiMeetingPerInstrMethod_MultiFacultyPerInstrMethod_ByFacultyLoad()
            {
                // Arrange 
                // Update overall instr method load
                instrMethodLoads["LEC"] = 30m;
                instrMethodLoads["LAB"] = 20m;

                // Add first section meeting for LEC 
                var secMeeting1 = new SectionMeeting("M1", "1", "LEC", startDate, endDate, "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
                    Frequency = "W",
                    StartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 10, 00, 00),
                    EndTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 11, 00, 00),
                    TotalMeetingMinutes = 200
                };
                sec.AddSectionMeeting(secMeeting1);
                // Add faculty FAC1 and FAC2 to section meeting1
                var secFaculty1 = new SectionFaculty("SF1", "1", "FAC1", "LEC", startDate, endDate, 80m);
                secMeeting1.AddSectionFaculty(secFaculty1);
                var secFaculty2 = new SectionFaculty("SF2", "1", "FAC2", "LEC", startDate, endDate, 20m) { LoadFactor = 13m };
                secMeeting1.AddSectionFaculty(secFaculty2);

                // Add second section meeting for LEC 
                var secMeeting2 = new SectionMeeting("M2", "1", "LEC", startDate, endDate, "W")
                {
                    Days = new List<DayOfWeek>() { DayOfWeek.Tuesday },
                    Frequency = "W",
                    StartTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, 10, 00, 00),
                    EndTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, 11, 00, 00),
                    TotalMeetingMinutes = 100
                };
                sec.AddSectionMeeting(secMeeting2);
                // Add faculty FAC1 and FAC2 to section meeting2
                var secFaculty3 = new SectionFaculty("SF3", "1", "FAC1", "LEC", startDate, endDate, 80m);
                secMeeting2.AddSectionFaculty(secFaculty3);
                var secFaculty4 = new SectionFaculty("SF4", "1", "FAC2", "LEC", startDate, endDate, 20m) { LoadFactor = 13m };
                secMeeting2.AddSectionFaculty(secFaculty4);

                // Act - get the section
                SectionProcessor.CalculateMeetingLoadFactor(sec.Meetings, instrMethodLoads);

                // Assert
                // Section meetings
                Assert.IsTrue(sec.Meetings.Count == 2);
                // Both section meetings are LEC
                var sectionMeetings = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC");
                Assert.AreEqual(2, sectionMeetings.Count());
                // Sec Meeting 1: two faculty
                var sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M1").SelectMany(m => m.FacultyRoster);
                Assert.AreEqual(2, sectionFaculty.Count());
                // MTG 1 FAC1 Load = Instr Method load (30) * responsibility 80% * percentage by meeting time (.667) = 16
                var secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(16m, secFac1.ElementAt(0).MeetingLoadFactor);
                // MTG 1 FAC2 Load = Faculty load (13) * percentage by meeting time (.333) = 8.67
                var secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(8.67m, secFac2.ElementAt(0).MeetingLoadFactor);
                // Sec Meeting 2: two faculty
                sectionFaculty = sec.Meetings.Where(m => m.InstructionalMethodCode == "LEC" && m.Id == "M2").SelectMany(m => m.FacultyRoster);
                // MTG 2 FAC1 load = Instr Method load (30) * responsibility 80% * percentage by meeting time (.333) = 8
                secFac1 = sectionFaculty.Where(f => f.FacultyId == "FAC1");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(8m, secFac1.ElementAt(0).MeetingLoadFactor);
                // MTG 2 FAC2 load = Faculty load (13) * percentage by meeting time (.333) = 4.33
                secFac2 = sectionFaculty.Where(f => f.FacultyId == "FAC2");
                Assert.AreEqual(1, secFac1.Count());
                Assert.AreEqual(4.33m, secFac2.ElementAt(0).MeetingLoadFactor);
            }
        }

        //[TestClass]
        //public class SectionProcessor_UpdateSectionFromNewSectionMeeting
        //{
        //    [TestInitialize]
        //    public void Initialize()
        //    {

        //    }

        //    [TestMethod]
        //    public void FacultyNotOnSection()
        //    {

        //    }

        //    [TestMethod]
        //    public void MeetingNotOnSection()
        //    {

        //    }
        //}

        [TestClass]
        public class DetermineEffectiveSectionRequisites
        {
            Course course;
            Section section;

            [TestInitialize]
            public void Initialize()
            {
                var depts = new List<OfferingDepartment>() { new OfferingDepartment("HIST") };
                course = new Course("1", "Course title", "Long course title", depts, "HIST", "100", "UG", new List<string>() { "100" }, 3, 0, new List<CourseApproval>());
                course.Requisites = new List<Requisite>()
                    {
                        new Requisite("R1", true, RequisiteCompletionOrder.Previous, true),
                        new Requisite("R2", false, RequisiteCompletionOrder.PreviousOrConcurrent, false),
                        new Requisite("R3", true, RequisiteCompletionOrder.Concurrent, false),
                        new Requisite("R4", true, RequisiteCompletionOrder.PreviousOrConcurrent, true),
                    };

                section = new Section("s1", "c1", "01", DateTime.Today, 3, 0, "section 1", "IN", depts, new List<string> { "100" }, "ug", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "X", DateTime.Today) });
                section.OverridesCourseRequisites = true;
                section.Requisites = new List<Requisite>()
                {
                        new Requisite("R5", true, RequisiteCompletionOrder.Previous, false),
                        new Requisite("R6", false, RequisiteCompletionOrder.PreviousOrConcurrent, false),
                        new Requisite("R7", true, RequisiteCompletionOrder.Concurrent, false),
                        new Requisite("R8", true, RequisiteCompletionOrder.PreviousOrConcurrent, false)
                };
            }

            [TestMethod]
            public void ProtectedCourseRequisitesCarryIntoSectionEvenIfSectionOverride()
            {
                var requisites = SectionProcessor.DetermineEffectiveSectionRequisites(section, course);
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R1").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R4").FirstOrDefault());
            }

            [TestMethod]
            public void UnprotectedCourseRequistesDoNotCarryIntoSectionIfSectionOverride()
            {
                var requisites = SectionProcessor.DetermineEffectiveSectionRequisites(section, course);
                Assert.IsNull(requisites.Where(r => r.RequirementCode == "R2").FirstOrDefault());
                Assert.IsNull(requisites.Where(r => r.RequirementCode == "R3").FirstOrDefault());
            }

            [TestMethod]
            public void SectionRequisitesPrevailIfSectionOverride()
            {
                var requisites = SectionProcessor.DetermineEffectiveSectionRequisites(section, course);
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R5").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R6").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R7").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R8").FirstOrDefault());
            }

            [TestMethod]
            public void CourseRequisitesPrevailIfNotSectionOverride()
            {
                section.OverridesCourseRequisites = false;
                var requisites = SectionProcessor.DetermineEffectiveSectionRequisites(section, course);
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R1").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R2").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R3").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R4").FirstOrDefault());
            }
        }

        [TestClass]
        public class DetermineWaiverableRequisites
        {
            Course course;
            Section section;

            [TestInitialize]
            public void Initialize()
            {
                var depts = new List<OfferingDepartment>() { new OfferingDepartment("HIST") };
                course = new Course("1", "Course title", "Long course title", depts, "HIST", "100", "UG", new List<string>() { "100" }, 3, 0, new List<CourseApproval>());
                course.Requisites = new List<Requisite>()
                    {
                        new Requisite("R1", true, RequisiteCompletionOrder.Previous, true),
                        new Requisite("R2", false, RequisiteCompletionOrder.PreviousOrConcurrent, false),
                        new Requisite("R3", true, RequisiteCompletionOrder.Concurrent, false),
                        new Requisite("R4", true, RequisiteCompletionOrder.PreviousOrConcurrent, true),
                    };

                section = new Section("s1", "c1", "01", DateTime.Today, 3, 0, "section 1", "IN", depts, new List<string> { "100" }, "ug", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "X", DateTime.Today) });
                section.OverridesCourseRequisites = true;
                section.Requisites = new List<Requisite>()
                {
                        new Requisite("R5", true, RequisiteCompletionOrder.Previous, false),
                        new Requisite("R6", false, RequisiteCompletionOrder.PreviousOrConcurrent, false),
                        new Requisite("R7", true, RequisiteCompletionOrder.Concurrent, false),
                        new Requisite("R8", true, RequisiteCompletionOrder.PreviousOrConcurrent, false)
                };
            }

            [TestMethod]
            public void OnlyPreviousOrPreviousConcurrentRequiredRequisitesAreWaiverable()
            {
                var requisites = SectionProcessor.DetermineWaiverableRequisites(section, course);
                // Protected course requisites
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R1").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R4").FirstOrDefault());
                // Section requisites
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R5").FirstOrDefault());
                Assert.IsNull(requisites.Where(r => r.RequirementCode == "R6").FirstOrDefault());
                Assert.IsNull(requisites.Where(r => r.RequirementCode == "R7").FirstOrDefault());
                Assert.IsNotNull(requisites.Where(r => r.RequirementCode == "R8").FirstOrDefault());
            }
        }

        [TestClass]
        public class GetSectionRegistrationDatesTests
        {
            private IEnumerable<Section> requestedSections;
            private IEnumerable<Term> regTerms;
            private RegistrationGroup registrationGroup;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private DateTime date1;
            private DateTime date2;
            private DateTime date3;
            private DateTime date4;
            private DateTime date5;
            private DateTime date6;
            private DateTime date7;
            private DateTime date8;
            private DateTime date9;
            private SectionRegistrationDate section15overrides;
            private RegistrationDate termdates;
            private RegistrationDate otherDates;
            private RegistrationDate otherDatesMain;
            private Mock<ILogger> loggerMock;
            private ILogger logger;

            [TestInitialize]
            public async void Initialize()
            {
                date1 = DateTime.Today;
                date2 = DateTime.Today.AddDays(1);
                date3 = DateTime.Today.AddDays(2);
                date4 = DateTime.Today.AddDays(3);
                date5 = DateTime.Today.AddDays(4);
                date6 = DateTime.Today.AddDays(5);
                date7 = DateTime.Today.AddDays(6);
                date8 = DateTime.Today.AddDays(7);
                date9 = DateTime.Today.AddDays(8);

                allSections = new TestSectionRepository().GetAsync().Result;
                regTerms = await new TestTermRepository().GetRegistrationTermsAsync();

                var term = regTerms.Where(t => t.Code == "2012/FA").FirstOrDefault();
                termdates = term.RegistrationDates.Where(r => r.Location == null).FirstOrDefault();

                registrationGroup = new RegistrationGroup("REGISTRAR");
                // Add a section specific override for section 15 in term 2012/FA.
                section15overrides = new SectionRegistrationDate("15", null, date1, date2, date3, date4, date5, date6, date7, date8, date9, null);
                registrationGroup.AddSectionRegistrationDate(section15overrides);
                // Add a section specific override for section 165 with no term.
                var section165overrides = new SectionRegistrationDate("165", null, date1, date2, date3, date4, date5, date6, date7, date8, date9, null);

                otherDates = new RegistrationDate(null, date1.AddYears(1), date2.AddYears(1), date3.AddYears(1), date4.AddYears(1), date5.AddYears(1), date6.AddYears(1), date7.AddYears(1), date8.AddYears(1), date9.AddYears(1), null);
                otherDatesMain = new RegistrationDate("MAIN", date1.AddYears(2), date2.AddYears(2), date3.AddYears(2), date4.AddYears(2), date5.AddYears(2), date6.AddYears(2), date7.AddYears(2), date8.AddYears(2), date9.AddYears(2), null);

                registrationGroup.AddSectionRegistrationDate(section165overrides);

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                // Initialize the logger for the section processor service
                SectionProcessor.InitializeLogger(logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                allSections = null;
                termdates = null;
                otherDates = null;
                registrationGroup = null;
                regTerms = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSectionRegistrationDates_Null_RegistrationGroup_throws_Exception()
            {
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(null, requestedSections, regTerms);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSectionRegistrationDates_Null_Sections_throws_Exception()
            {
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, null, regTerms);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSectionRegistrationDates_Empty_Sections_throws_Exception()
            {
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, new List<Section>(), regTerms);
            }

            [TestMethod]
            public void GetSectionRegistrationDates_SectionsInNonRegTermReturnsNone()
            {
                // Sections in terms outside of the terms open for registration will not return any DTOs with date information.
                // Terms open for registration are "2012/FA" and "2013/SP".
                requestedSections = allSections.Where(s => s.TermId == "2014/FA");
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, regTerms);
                Assert.AreEqual(0, sectionRegistrationDates.Count());
            }

            [TestMethod]
            public void GetSectionRegistrationaDates_NonTermSectionsWithNoOverridesReturnsNone()
            {
                // Sections with no terms and no overrides will not return any DTOs with date information.
                requestedSections = allSections.Where(s => string.IsNullOrEmpty(s.TermId));
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, regTerms);
                Assert.AreEqual(0, sectionRegistrationDates.Count());
            }

            public void GetSectionRegistrationaDates_MixOfTermAndNonTermReturnsBoth()
            {
                // Section 15 has a term and section 165 has no term. Both have the same overrides so both should have an item returned.
                requestedSections = allSections.Where(s => s.Id == "15" || s.Id == "165");
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, regTerms);
                Assert.AreEqual(2, sectionRegistrationDates.Count());
                var section15dates = sectionRegistrationDates.Where(s => s.SectionId == "15").FirstOrDefault();
                Assert.AreEqual(section15overrides.PreRegistrationStartDate, section15dates.PreRegistrationStartDate);
                Assert.AreEqual(section15overrides.RegistrationStartDate, section15dates.RegistrationStartDate);
                Assert.AreEqual(section15overrides.AddStartDate, section15dates.AddStartDate);
                Assert.AreEqual(section15overrides.DropStartDate, section15dates.DropStartDate);
                Assert.AreEqual(section15overrides.PreRegistrationEndDate, section15dates.PreRegistrationEndDate);
                Assert.AreEqual(section15overrides.RegistrationEndDate, section15dates.RegistrationEndDate);
                Assert.AreEqual(section15overrides.AddEndDate, section15dates.AddEndDate);
                Assert.AreEqual(section15overrides.DropEndDate, section15dates.DropEndDate);
                Assert.AreEqual(section15overrides.DropGradeRequiredDate, section15dates.DropGradeRequiredDate);
                var section165dates = sectionRegistrationDates.Where(s => s.SectionId == "165").FirstOrDefault();
                Assert.AreEqual(section15overrides.PreRegistrationStartDate, section165dates.PreRegistrationStartDate);
                Assert.AreEqual(section15overrides.RegistrationStartDate, section165dates.RegistrationStartDate);
                Assert.AreEqual(section15overrides.AddStartDate, section165dates.AddStartDate);
                Assert.AreEqual(section15overrides.DropStartDate, section165dates.DropStartDate);
                Assert.AreEqual(section15overrides.PreRegistrationEndDate, section165dates.PreRegistrationEndDate);
                Assert.AreEqual(section15overrides.RegistrationEndDate, section165dates.RegistrationEndDate);
                Assert.AreEqual(section15overrides.AddEndDate, section165dates.AddEndDate);
                Assert.AreEqual(section15overrides.DropEndDate, section165dates.DropEndDate);
                Assert.AreEqual(section15overrides.DropGradeRequiredDate, section165dates.DropGradeRequiredDate);
            }

            [TestMethod]
            public void GetSectionRegistrationDates_SectionWithRegUserSectionOverrideReturnsOverrides()
            {
                // Prepare

                // section 15 has registration User Section override
                requestedSections = allSections.Where(s => s.Id == "15" || s.Id == "16");

                // Act
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, regTerms);

                // Verify
                Assert.AreEqual(2, sectionRegistrationDates.Count());
                var section15dates = sectionRegistrationDates.Where(s => s.SectionId == "15").FirstOrDefault();
                Assert.AreEqual(section15overrides.PreRegistrationStartDate, section15dates.PreRegistrationStartDate);
                Assert.AreEqual(section15overrides.RegistrationStartDate, section15dates.RegistrationStartDate);
                Assert.AreEqual(section15overrides.AddStartDate, section15dates.AddStartDate);
                Assert.AreEqual(section15overrides.DropStartDate, section15dates.DropStartDate);
                Assert.AreEqual(section15overrides.PreRegistrationEndDate, section15dates.PreRegistrationEndDate);
                Assert.AreEqual(section15overrides.RegistrationEndDate, section15dates.RegistrationEndDate);
                Assert.AreEqual(section15overrides.AddEndDate, section15dates.AddEndDate);
                Assert.AreEqual(section15overrides.DropEndDate, section15dates.DropEndDate);
                Assert.AreEqual(section15overrides.DropGradeRequiredDate, section15dates.DropGradeRequiredDate);

                // And the one with no override should show the term dates
                var section16dates = sectionRegistrationDates.Where(s => s.SectionId == "16").FirstOrDefault();
                Assert.AreEqual(termdates.PreRegistrationStartDate, section16dates.PreRegistrationStartDate);
                Assert.AreEqual(termdates.RegistrationStartDate, section16dates.RegistrationStartDate);
                Assert.AreEqual(termdates.AddStartDate, section16dates.AddStartDate);
                Assert.AreEqual(termdates.DropStartDate, section16dates.DropStartDate);
                Assert.AreEqual(termdates.PreRegistrationEndDate, section16dates.PreRegistrationEndDate);
                Assert.AreEqual(termdates.RegistrationEndDate, section16dates.RegistrationEndDate);
                Assert.AreEqual(termdates.AddEndDate, section16dates.AddEndDate);
                Assert.AreEqual(termdates.DropEndDate, section16dates.DropEndDate);
                Assert.AreEqual(termdates.DropGradeRequiredDate, section16dates.DropGradeRequiredDate);
            }

            [TestMethod]
            public void GetSectionRegistrationDates_SectionWithSectionOverrideReturnsOverrides()
            {
                // Stage: Give section 17 specific Section overrides - using otherDates 
                var sec17 = allSections.Where(s => s.Id == "17").FirstOrDefault();
                sec17.RegistrationDateOverrides = otherDates;
                requestedSections = new List<Section>() { sec17 };

                // Act
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, regTerms);

                Assert.AreEqual(1, sectionRegistrationDates.Count());
                var section17dates = sectionRegistrationDates.Where(s => s.SectionId == "17").FirstOrDefault();
                Assert.AreEqual(otherDates.PreRegistrationStartDate, section17dates.PreRegistrationStartDate);
                Assert.AreEqual(otherDates.RegistrationStartDate, section17dates.RegistrationStartDate);
                Assert.AreEqual(otherDates.AddStartDate, section17dates.AddStartDate);
                Assert.AreEqual(otherDates.DropStartDate, section17dates.DropStartDate);
                Assert.AreEqual(otherDates.PreRegistrationEndDate, section17dates.PreRegistrationEndDate);
                Assert.AreEqual(otherDates.RegistrationEndDate, section17dates.RegistrationEndDate);
                Assert.AreEqual(otherDates.AddEndDate, section17dates.AddEndDate);
                Assert.AreEqual(otherDates.DropEndDate, section17dates.DropEndDate);
                Assert.AreEqual(otherDates.DropGradeRequiredDate, section17dates.DropGradeRequiredDate);
            }

            [TestMethod]
            public void GetSectionRegistrationDates_SectionWithRegUserTermLocationOverrides()
            {
                // Stage: Give HIST-200 Intermediate History Section 01 Term 2013/SP a TermLocation override specific to the reg User for term 2013/SP and location MAIN.
                registrationGroup.AddTermRegistrationDate(new TermRegistrationDate("2013/SP", "MAIN", otherDatesMain.RegistrationStartDate, otherDatesMain.RegistrationEndDate, otherDatesMain.PreRegistrationStartDate, otherDatesMain.PreRegistrationEndDate, otherDatesMain.AddStartDate, otherDatesMain.AddEndDate, otherDatesMain.DropStartDate, otherDatesMain.DropEndDate, otherDatesMain.DropGradeRequiredDate, null));

                var section = allSections.Where(s => s.CourseId == "42" && s.TermId == "2013/SP" && s.Number == "01").FirstOrDefault();
                requestedSections = new List<Section>() { section };

                // Act
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, regTerms);

                // Assert
                Assert.AreEqual(1, sectionRegistrationDates.Count());
                var sectionDates = sectionRegistrationDates.Where(s => s.SectionId == section.Id).FirstOrDefault();
                Assert.AreEqual(otherDatesMain.PreRegistrationStartDate, sectionDates.PreRegistrationStartDate);
                Assert.AreEqual(otherDatesMain.RegistrationStartDate, sectionDates.RegistrationStartDate);
                Assert.AreEqual(otherDatesMain.AddStartDate, sectionDates.AddStartDate);
                Assert.AreEqual(otherDatesMain.DropStartDate, sectionDates.DropStartDate);
                Assert.AreEqual(otherDatesMain.PreRegistrationEndDate, sectionDates.PreRegistrationEndDate);
                Assert.AreEqual(otherDatesMain.RegistrationEndDate, sectionDates.RegistrationEndDate);
                Assert.AreEqual(otherDatesMain.AddEndDate, sectionDates.AddEndDate);
                Assert.AreEqual(otherDatesMain.DropEndDate, sectionDates.DropEndDate);
                Assert.AreEqual(otherDatesMain.DropGradeRequiredDate, sectionDates.DropGradeRequiredDate);
            }

            [TestMethod]
            public void GetSectionRegistrationDates_SectionWithTermLocationOverrides()
            {
                // Stage: Give registration term 2013/SP as special term/location override for location MAIN.
                var term2013SP = regTerms.Where(t => t.Code == "2013/SP").FirstOrDefault();
                term2013SP.RegistrationDates.Add(otherDatesMain);
                var liteRegTerms = new List<Term>() { term2013SP };

                var section = allSections.Where(s => s.CourseId == "42" && s.TermId == "2013/SP" && s.Number == "01").FirstOrDefault();
                requestedSections = new List<Section>() { section };

                // Act
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, liteRegTerms);

                Assert.AreEqual(1, sectionRegistrationDates.Count());
                var sectionDates = sectionRegistrationDates.Where(s => s.SectionId == section.Id).FirstOrDefault();
                Assert.AreEqual(otherDatesMain.PreRegistrationStartDate, sectionDates.PreRegistrationStartDate);
                Assert.AreEqual(otherDatesMain.RegistrationStartDate, sectionDates.RegistrationStartDate);
                Assert.AreEqual(otherDatesMain.AddStartDate, sectionDates.AddStartDate);
                Assert.AreEqual(otherDatesMain.DropStartDate, sectionDates.DropStartDate);
                Assert.AreEqual(otherDatesMain.PreRegistrationEndDate, sectionDates.PreRegistrationEndDate);
                Assert.AreEqual(otherDatesMain.RegistrationEndDate, sectionDates.RegistrationEndDate);
                Assert.AreEqual(otherDatesMain.AddEndDate, sectionDates.AddEndDate);
                Assert.AreEqual(otherDatesMain.DropEndDate, sectionDates.DropEndDate);
                Assert.AreEqual(otherDatesMain.DropGradeRequiredDate, sectionDates.DropGradeRequiredDate);
            }

            [TestMethod]
            public void GetSectionRegistrationDates_SectionWithRegUserTermOverrides()
            {
                // Stage: Give reg user a registration term override for 2013/SP - NO LOCATION.
                registrationGroup.AddTermRegistrationDate(new TermRegistrationDate("2013/SP", null, otherDates.RegistrationStartDate, otherDates.RegistrationEndDate, otherDates.PreRegistrationStartDate, otherDates.PreRegistrationEndDate, otherDates.AddStartDate, otherDates.AddEndDate, otherDates.DropStartDate, otherDates.DropEndDate, otherDates.DropGradeRequiredDate, otherDates.CensusDates));

                var section = allSections.Where(s => s.CourseId == "42" && s.TermId == "2013/SP" && s.Number == "01").FirstOrDefault();
                requestedSections = new List<Section>() { section };

                // Act
                var sectionRegistrationDates = SectionProcessor.GetSectionRegistrationDates(registrationGroup, requestedSections, regTerms);

                Assert.AreEqual(1, sectionRegistrationDates.Count());
                var sectionDates = sectionRegistrationDates.Where(s => s.SectionId == section.Id).FirstOrDefault();
                Assert.AreEqual(otherDates.PreRegistrationStartDate, sectionDates.PreRegistrationStartDate);
                Assert.AreEqual(otherDates.RegistrationStartDate, sectionDates.RegistrationStartDate);
                Assert.AreEqual(otherDates.AddStartDate, sectionDates.AddStartDate);
                Assert.AreEqual(otherDates.DropStartDate, sectionDates.DropStartDate);
                Assert.AreEqual(otherDates.PreRegistrationEndDate, sectionDates.PreRegistrationEndDate);
                Assert.AreEqual(otherDates.RegistrationEndDate, sectionDates.RegistrationEndDate);
                Assert.AreEqual(otherDates.AddEndDate, sectionDates.AddEndDate);
                Assert.AreEqual(otherDates.DropEndDate, sectionDates.DropEndDate);
                Assert.AreEqual(otherDates.DropGradeRequiredDate, sectionDates.DropGradeRequiredDate);
            }
        }

        [TestClass]
        public class GetSectionCensusDatesTests
        {
            private IEnumerable<Term> allTterms;
            private IEnumerable<Section> allSections;
            private IEnumerable<Section> requestedSections;
            private DateTime date1;
            private DateTime date2;
            private DateTime date3;
            private DateTime date4;
            private DateTime date5;
            private DateTime date6;
            private DateTime date7;
            private DateTime date8;
            private DateTime date9;
            private RegistrationDate termdates;
            private RegistrationDate otherDates;
            private RegistrationDate otherDatesMain;
            private Mock<ILogger> loggerMock;
            private ILogger logger;

            [TestInitialize]
            public async void Initialize()
            {
                date1 = DateTime.Today;
                date2 = DateTime.Today.AddDays(1);
                date3 = DateTime.Today.AddDays(2);
                date4 = DateTime.Today.AddDays(3);
                date5 = DateTime.Today.AddDays(4);
                date6 = DateTime.Today.AddDays(5);
                date7 = DateTime.Today.AddDays(6);
                date8 = DateTime.Today.AddDays(7);
                date9 = DateTime.Today.AddDays(8);

                allTterms = await new TestTermRepository().GetAsync();
                allSections = new TestSectionRepository().GetAsync().Result;

                var term = allTterms.Where(t => t.Code == "2012/FA").FirstOrDefault();
                termdates = term.RegistrationDates.Where(r => r.Location == null).FirstOrDefault();

                otherDates = new RegistrationDate(null, date1.AddYears(1), date2.AddYears(1), date3.AddYears(1), date4.AddYears(1), date5.AddYears(1), date6.AddYears(1), date7.AddYears(1), date8.AddYears(1), date9.AddYears(1), null);
                otherDatesMain = new RegistrationDate("MAIN", date1.AddYears(2), date2.AddYears(2), date3.AddYears(2), date4.AddYears(2), date5.AddYears(2), date6.AddYears(2), date7.AddYears(2), date8.AddYears(2), date9.AddYears(2), null);

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                SectionProcessor.InitializeLogger(logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                allTterms = null;
                allSections = null;
                termdates = null;
                otherDates = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSectionCensusDates_Null_Sections_ThrowsException()
            {
                SectionProcessor.GetSectionCensusDates(null, allTterms);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetSectionCensusDates_Empty_Sections_ThrowsException()
            {
                SectionProcessor.GetSectionCensusDates(new List<Section>(), allTterms);
            }

            [TestMethod]
            public void GetSectionCensusDates_Null_Terms_DoesNotThrowsException()
            {
                requestedSections = allSections.Where(s => s != null && !string.IsNullOrEmpty(s.TermId) && s.TermId == "2012/FA");
                var result = SectionProcessor.GetSectionCensusDates(requestedSections, null);
            }

            [TestMethod]
            public void GetSectionCensusDates_Empty_Terms_DoesNotThrowsException()
            {
                requestedSections = allSections.Where(s => s != null && !string.IsNullOrEmpty(s.TermId) && s.TermId == "2012/FA");
                var result = SectionProcessor.GetSectionCensusDates(requestedSections, new List<Term>());
            }

            [TestMethod]
            public void GetSectionCensusDates_SectionsInNonRegTermReturnsDates()
            {
                // Sections in terms outside of the terms open for registration will not return any DTOs with date information.
                // Terms open for registration are "2012/FA" and "2013/SP".
                var term = allTterms.Where(t => t.Code == "2014/FA").FirstOrDefault();

                // Give it some registration dates and census dates
                term.RegistrationDates.Add(new RegistrationDate(null, new DateTime(2012, 10, 1), new DateTime(2012, 10, 5), new DateTime(2013, 1, 1), new DateTime(2013, 1, 15), new DateTime(2013, 2, 1), new DateTime(2013, 2, 15), null, null, null, new List<DateTime?>() { new DateTime(2021, 01, 01), new DateTime(2021, 03, 02) }));
                term.RegistrationDates.Add(new RegistrationDate("NW", new DateTime(2012, 10, 1), new DateTime(2012, 10, 5), new DateTime(2013, 1, 1), new DateTime(2013, 1, 15), new DateTime(2013, 2, 1), new DateTime(2013, 2, 15), null, null, null, new List<DateTime?>() { new DateTime(2021, 02, 01), new DateTime(2021, 05, 02), new DateTime(2021, 06, 02) }));

                requestedSections = allSections.Where(s => s.TermId == "2014/FA");
                var sectionCensusDates = SectionProcessor.GetSectionCensusDates(requestedSections, new List<Term>() { term });
                Assert.AreEqual(expected: requestedSections.Count(), actual: sectionCensusDates.Count);

                //get section with no location
                var censusDates = sectionCensusDates["669"];
                Assert.AreEqual(expected: 2, actual: censusDates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 01, 01), actual: censusDates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 03, 02), actual: censusDates[1]);

                //get section with location
                censusDates = sectionCensusDates["678"];
                Assert.AreEqual(expected: 3, actual: censusDates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 02, 01), actual: censusDates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 05, 02), actual: censusDates[1]);
                Assert.AreEqual(expected: new DateTime(2021, 06, 02), actual: censusDates[2]);
            }

            [TestMethod]
            public void GetSectionCensusDates_NonTermSectionsWithNoOverridesReturnsNone()
            {
                // Sections with no terms and no overrides will not return any DTOs with date information.
                requestedSections = allSections.Where(s => string.IsNullOrEmpty(s.TermId));
                var censusDates = SectionProcessor.GetSectionCensusDates(requestedSections, null);
                Assert.AreEqual(expected: 0, actual: censusDates.Count);
            }

            [TestMethod]
            public void GetSectionCensusDates_MixOfTermAndNonTermHaveOverrideWithNullCensusDatesReturnsSectionNullCensusDates()
            {
                var terms = allTterms.Where(t => t.Code == "2012/FA");
                var section15 = allSections.Where(s => s.Id == "15").FirstOrDefault();
                var section165 = allSections.Where(s => s.Id == "165").FirstOrDefault();

                // Add a section specific override for section 15 in term 2012/FA.
                var section15overrides = new SectionRegistrationDate("15", null, date1, date2, date3, date4, date5, date6, date7, date8, date9, null);
                section15.RegistrationDateOverrides = section15overrides;

                // Add a section specific override for section 165 with no term.
                var section165overrides = new SectionRegistrationDate("165", null, date1, date2, date3, date4, date5, date6, date7, date8, date9, null);
                section165.RegistrationDateOverrides = section165overrides;

                var sectionCensusDates = SectionProcessor.GetSectionCensusDates(new List<Section>() { section15, section165 }, terms);
                //check for both sections
                Assert.AreEqual(2, sectionCensusDates.Count());

                var section15dates = sectionCensusDates["15"];
                //check that census dates list is null
                Assert.IsNull(section15dates);

                var section165dates = sectionCensusDates["165"];
                //check that census dates list is null
                Assert.IsNull(section165dates);
            }

            [TestMethod]
            public void GetSectionCensusDates_MixOfTermAndNonTermHaveOverrideWithEmptyCensusDatesReturnsSectionEmptyCensusDates()
            {
                var terms = allTterms.Where(t => t.Code == "2012/FA");
                var section15 = allSections.Where(s => s.Id == "15").FirstOrDefault();
                var section165 = allSections.Where(s => s.Id == "165").FirstOrDefault();

                // Add a section specific override for section 15 in term 2012/FA.
                var section15overrides = new SectionRegistrationDate("15", null, date1, date2, date3, date4, date5, date6, date7, date8, date9, new List<DateTime?>());
                section15.RegistrationDateOverrides = section15overrides;

                // Add a section specific override for section 165 with no term.
                var section165overrides = new SectionRegistrationDate("165", null, date1, date2, date3, date4, date5, date6, date7, date8, date9, new List<DateTime?>());
                section165.RegistrationDateOverrides = section165overrides;

                var sectionCensusDates = SectionProcessor.GetSectionCensusDates(new List<Section>() { section15, section165 }, terms);
                Assert.AreEqual(2, sectionCensusDates.Count());

                var section15dates = sectionCensusDates["15"];
                Assert.AreEqual(expected: 0, actual: section15dates.Count);

                var section165dates = sectionCensusDates["165"];
                Assert.AreEqual(expected: 0, actual: section165dates.Count);
            }

            [TestMethod]
            public void GetSectionCensusDates_MixOfTermAndNonTermHaveOverrideWithCensusDatesReturnsBoth()
            {
                var terms = allTterms.Where(t => t.Code == "2012/FA");
                var section15 = allSections.Where(s => s.Id == "15").FirstOrDefault();
                var section165 = allSections.Where(s => s.Id == "165").FirstOrDefault();

                // Add a section specific override for section 165 with no term.
                var section15overrides = new SectionRegistrationDate("15", null, date1, date2, date3, date4, date5, date6, date7, date8, date9,
                    new List<DateTime?>() { new DateTime(2021, 02, 02), new DateTime(2021, 05, 03), new DateTime(2021, 06, 04) });
                section15.RegistrationDateOverrides = section15overrides;

                // Add a section specific override for section 165 with no term.
                var section165overrides = new SectionRegistrationDate("165", null, date1, date2, date3, date4, date5, date6, date7, date8, date9,
                    new List<DateTime?>() { new DateTime(2021, 04, 02), new DateTime(2021, 06, 04) });
                section165.RegistrationDateOverrides = section165overrides;

                var sectionCensusDates = SectionProcessor.GetSectionCensusDates(new List<Section>() { section15, section165 }, terms);
                Assert.AreEqual(2, sectionCensusDates.Count());

                var section15dates = sectionCensusDates["15"];
                Assert.AreEqual(expected: 3, actual: section15dates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 02, 02), actual: section15dates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 05, 03), actual: section15dates[1]);
                Assert.AreEqual(expected: new DateTime(2021, 06, 04), actual: section15dates[2]);

                var section165dates = sectionCensusDates["165"];
                Assert.AreEqual(expected: 2, actual: section165dates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 04, 02), actual: section165dates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 06, 04), actual: section165dates[1]);
            }

            [TestMethod]
            public void GetSectionCensusDates_TermSectionWithTermLocationeOverrideCensusDatesReturnsTermLocationCensusDates()
            {
                var term2012 = allTterms.Where(t => t.Code == "2012/FA").FirstOrDefault(); //reg term
                var term2014 = allTterms.Where(t => t.Code == "2014/FA").FirstOrDefault(); // non-reg term

                // Give it some registration dates and census dates
                term2014.RegistrationDates.Add(new RegistrationDate(null, new DateTime(2012, 10, 1), new DateTime(2012, 10, 5), new DateTime(2013, 1, 1), new DateTime(2013, 1, 15), new DateTime(2013, 2, 1), new DateTime(2013, 2, 15), null, null, null, new List<DateTime?>() { new DateTime(2021, 01, 01), new DateTime(2021, 03, 02) }));
                term2014.RegistrationDates.Add(new RegistrationDate("NW", new DateTime(2012, 10, 1), new DateTime(2012, 10, 5), new DateTime(2013, 1, 1), new DateTime(2013, 1, 15), new DateTime(2013, 2, 1), new DateTime(2013, 2, 15), null, null, null, new List<DateTime?>() { new DateTime(2021, 02, 01), new DateTime(2021, 05, 02), new DateTime(2021, 06, 02) }));

                requestedSections = allSections.Where(s => (s.Id == "270" && s.TermId == "2012/FA") || (s.Id == "678" && s.TermId == "2014/FA"));

                var sectionCensusDates = SectionProcessor.GetSectionCensusDates(requestedSections, new List<Term>() { term2012, term2014 });
                Assert.AreEqual(expected: requestedSections.Count(), actual: sectionCensusDates.Count);

                //get reg section with location
                var censusDates = sectionCensusDates["270"];
                Assert.AreEqual(expected: 3, actual: censusDates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 02, 01), actual: censusDates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 05, 02), actual: censusDates[1]);
                Assert.AreEqual(expected: new DateTime(2021, 06, 02), actual: censusDates[2]);

                //get non-reg section with location
                censusDates = sectionCensusDates["678"];
                Assert.AreEqual(expected: 3, actual: censusDates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 02, 01), actual: censusDates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 05, 02), actual: censusDates[1]);
                Assert.AreEqual(expected: new DateTime(2021, 06, 02), actual: censusDates[2]);
            }

            [TestMethod]
            public void GetSectionCensusDates_TermSectionWithTermOverrideCensusDatesReturnsTermLocationCensusDates()
            {
                var term2012 = allTterms.Where(t => t.Code == "2012/FA").FirstOrDefault(); //reg term
                var term2014 = allTterms.Where(t => t.Code == "2014/FA").FirstOrDefault(); // non-reg term

                // Give it some registration dates and census dates
                term2014.RegistrationDates.Add(new RegistrationDate(null, new DateTime(2012, 10, 1), new DateTime(2012, 10, 5), new DateTime(2013, 1, 1), new DateTime(2013, 1, 15), new DateTime(2013, 2, 1), new DateTime(2013, 2, 15), null, null, null, new List<DateTime?>() { new DateTime(2021, 01, 01), new DateTime(2021, 03, 02) }));
                term2014.RegistrationDates.Add(new RegistrationDate("NW", new DateTime(2012, 10, 1), new DateTime(2012, 10, 5), new DateTime(2013, 1, 1), new DateTime(2013, 1, 15), new DateTime(2013, 2, 1), new DateTime(2013, 2, 15), null, null, null, new List<DateTime?>() { new DateTime(2021, 02, 01), new DateTime(2021, 05, 02), new DateTime(2021, 06, 02) }));

                requestedSections = allSections.Where(s => (s.Id == "91" && s.TermId == "2012/FA") || (s.Id == "669" && s.TermId == "2014/FA"));

                var sectionCensusDates = SectionProcessor.GetSectionCensusDates(requestedSections, new List<Term>() { term2012, term2014 });
                Assert.AreEqual(expected: requestedSections.Count(), actual: sectionCensusDates.Count);

                //get reg section with no location (not in location NW)
                var censusDates = sectionCensusDates["91"];
                Assert.AreEqual(expected: 2, actual: censusDates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 01, 01), actual: censusDates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 03, 02), actual: censusDates[1]);

                //get non-reg section with no location (not in location NW)
                censusDates = sectionCensusDates["669"];
                Assert.AreEqual(expected: 2, actual: censusDates.Count);
                Assert.AreEqual(expected: new DateTime(2021, 01, 01), actual: censusDates[0]);
                Assert.AreEqual(expected: new DateTime(2021, 03, 02), actual: censusDates[1]);
            }
        }
    }
}
