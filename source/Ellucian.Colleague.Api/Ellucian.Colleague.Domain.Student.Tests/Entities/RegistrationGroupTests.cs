using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationGroupTests
    {

        [TestClass]
        public class RegistrationGroup_Constructor
        {
            private string id;
            private RegistrationGroup group;

            [TestInitialize]
            public void Initialize()
            {
                id = "REGISTRAR";
            }

            [TestCleanup]
            public void CleanUp()
            {
                group = null;
            }

            [TestMethod]
            public void RegistrationGroup_Success_Id()
            {
                group = new RegistrationGroup(id);
                Assert.AreEqual(id, group.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationGroup_ThrowsException_NullId()
            {
                group = new RegistrationGroup(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RegistrationGroup_ThrowsException_EmptyId()
            {
                group = new RegistrationGroup(string.Empty);
            }

        }

        [TestClass]
        public class RegistrationGroup_AddStaffAssignment
        {
            private string id;
            private string staffId;
            private string staffId2;
            private DateTime? startDate;
            private DateTime? endDate;
            private DateTime? endDate2;
            private RegistrationGroup group;

            [TestInitialize]
            public void Initialize()
            {
                id = "5";
                staffId = "R000001";
                staffId2 = "R000002";
                startDate = DateTime.Today.AddDays(-2);
                endDate = DateTime.Today.AddDays(10);
                endDate2 = DateTime.Today.AddDays(12);
                group = new RegistrationGroup(id);
            }

            [TestCleanup]
            public void CleanUp()
            {
                group = null;
            }

            [TestMethod]
            public void AddStaffAssignment_AddsUniqueAssignment()
            {
                StaffAssignment staff1 = new StaffAssignment(staffId);
                group.AddStaffAssignment(staff1);
                Assert.AreEqual(1, group.StaffAssignments.Count());
                Assert.AreEqual(staffId, group.StaffAssignments.ElementAt(0).StaffId);

                StaffAssignment staff2 = new StaffAssignment(staffId) { StartDate = startDate, EndDate = endDate };
                group.AddStaffAssignment(staff2);
                Assert.AreEqual(2, group.StaffAssignments.Count());
                Assert.AreEqual(staffId, group.StaffAssignments.ElementAt(1).StaffId);
                Assert.AreEqual(startDate, group.StaffAssignments.ElementAt(1).StartDate);
                Assert.AreEqual(endDate, group.StaffAssignments.ElementAt(1).EndDate);

                // Now add one where just the end date is different.
                StaffAssignment staff3 = new StaffAssignment(staffId) { StartDate = startDate, EndDate = endDate2 };
                group.AddStaffAssignment(staff3);
                Assert.AreEqual(3, group.StaffAssignments.Count());
                Assert.AreEqual(staffId, group.StaffAssignments.ElementAt(2).StaffId);
                Assert.AreEqual(startDate, group.StaffAssignments.ElementAt(2).StartDate);
                Assert.AreEqual(endDate2, group.StaffAssignments.ElementAt(2).EndDate);

                // Now add one where just the start date is different.
                StaffAssignment staff4 = new StaffAssignment(staffId) { EndDate = endDate };
                group.AddStaffAssignment(staff4);
                Assert.AreEqual(4, group.StaffAssignments.Count());
                Assert.AreEqual(staffId, group.StaffAssignments.ElementAt(3).StaffId);
                Assert.IsNull(group.StaffAssignments.ElementAt(3).StartDate);
                Assert.AreEqual(endDate, group.StaffAssignments.ElementAt(3).EndDate);
            }

            [TestMethod]
            public void AddStaffAssignment_AddExistingAssignment_DoesNotAdd()
            {
                StaffAssignment staff1 = new StaffAssignment(staffId) { StartDate = startDate, EndDate = endDate };
                group.AddStaffAssignment(staff1);
                group.AddStaffAssignment(staff1);
                Assert.AreEqual(1, group.StaffAssignments.Count());
                StaffAssignment staff2 = new StaffAssignment(staffId2);
                group.AddStaffAssignment(staff2);
                group.AddStaffAssignment(staff2);
                Assert.AreEqual(2, group.StaffAssignments.Count());
            }
        }

        [TestClass]
        public class RegistrationGroup_AddTermRegistrationDate
        {
            // In this class we really don't care what the dates are, only that they are being updated.
            private string id;
            private string termCode;
            private string locationCode;
            private DateTime? regStart;
            private DateTime? regEnd;
            private RegistrationGroup group;
            private TermRegistrationDate termLocationDate;
            private TermRegistrationDate termDate;
            private List<DateTime?> censusDates;

            [TestInitialize]
            public void Initialize()
            {
                id = "5";
                termCode = "2014/FA";
                locationCode = "Location1";
                regStart = DateTime.Today.AddDays(1);
                regEnd = DateTime.Today.AddDays(2);
                group = new RegistrationGroup(id);
                termLocationDate = new TermRegistrationDate(termCode, locationCode, regStart, regEnd, null, null, null, null, null, null, null, null);
                termDate = new TermRegistrationDate(termCode, null, regStart, regEnd, null, null, null, null, null, null, null, null);

            }

            [TestCleanup]
            public void CleanUp()
            {
                group = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddTermRegistration_NullTermRegistration_ThrowsException()
            {
                group.AddTermRegistrationDate(null);
            }

            [TestMethod]
            public void AddTermRegistrationDate_WithLocation_AddsToCorrectList()
            {
                group.AddTermRegistrationDate(termLocationDate);
                Assert.AreEqual(1, group.TermLocationRegistrationDates.Count());
                Assert.AreEqual(0, group.TermRegistrationDates.Count());
                var addedTerm = group.TermLocationRegistrationDates.ElementAt(0);
                Assert.AreEqual(termCode, addedTerm.TermId);
                Assert.AreEqual(locationCode, addedTerm.Location);
                Assert.AreEqual(regStart, addedTerm.RegistrationStartDate);
                Assert.AreEqual(regEnd, addedTerm.RegistrationEndDate);
            }

            [TestMethod]
            public void AddTermRegistrationDate_WithNoLocation_AddsToCorrectList()
            {
                group.AddTermRegistrationDate(termDate);
                Assert.AreEqual(0, group.TermLocationRegistrationDates.Count());
                Assert.AreEqual(1, group.TermRegistrationDates.Count());
                var addedTerm = group.TermRegistrationDates.ElementAt(0);
                Assert.AreEqual(termCode, addedTerm.TermId);
                Assert.AreEqual(null, addedTerm.Location);
                Assert.AreEqual(regStart, addedTerm.RegistrationStartDate);
                Assert.AreEqual(regEnd, addedTerm.RegistrationEndDate);
            }

            [TestMethod]
            public void AddTermRegistrationDate_WithLocation_DoesNotAddDuplicate()
            {
                group.AddTermRegistrationDate(termLocationDate);
                group.AddTermRegistrationDate(termLocationDate);
                Assert.AreEqual(1, group.TermLocationRegistrationDates.Count());
                Assert.AreEqual(0, group.TermRegistrationDates.Count());
            }

            [TestMethod]
            public void AddTermRegistrationDate_WithNoLocation_DoesNotAddDuplicate()
            {
                group.AddTermRegistrationDate(termDate);
                group.AddTermRegistrationDate(termDate);
                Assert.AreEqual(0, group.TermLocationRegistrationDates.Count());
                Assert.AreEqual(1, group.TermRegistrationDates.Count());
            }
        }

        [TestClass]
        public class RegistrationGroup_AddSectionRegistrationDate
        {
            // In this class we really don't care what the dates are, only that they are being updated.
            private string id;
            private string sectionId;
            private string locationCode;
            private DateTime? regStart;
            private DateTime? regEnd;
            private RegistrationGroup group;
            private SectionRegistrationDate sectionRegDate;

            [TestInitialize]
            public void Initialize()
            {
                id = "5";
                sectionId = "Section1";
                locationCode = "Location1";
                regStart = DateTime.Today.AddDays(1);
                regEnd = DateTime.Today.AddDays(2);
                group = new RegistrationGroup(id);
                sectionRegDate = new SectionRegistrationDate(sectionId, locationCode, regStart, regEnd, null, null, null, null, null, null, null, null, RegistrationDateSource.RegistrationUserSection);
            }

            [TestCleanup]
            public void CleanUp()
            {
                group = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddSectionRegistration_NullSectionRegistration_ThrowsException()
            {
                group.AddSectionRegistrationDate(null);
            }

            [TestMethod]
            public void AddSectionRegistrationDate_WithSectionRegistrationDates()
            {
                group.AddSectionRegistrationDate(sectionRegDate);
                Assert.AreEqual(1, group.SectionRegistrationDates.Count());
                var addedSection = group.SectionRegistrationDates.ElementAt(0);
                Assert.AreEqual(sectionId, addedSection.SectionId);
                Assert.AreEqual(locationCode, addedSection.Location);
                Assert.AreEqual(regStart, addedSection.RegistrationStartDate);
                Assert.AreEqual(regEnd, addedSection.RegistrationEndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AddSectionRegistrationDate_DoesNotAddDuplicate()
            {
                group.AddSectionRegistrationDate(sectionRegDate);
                group.AddSectionRegistrationDate(sectionRegDate);
            }
        }
    }
}
