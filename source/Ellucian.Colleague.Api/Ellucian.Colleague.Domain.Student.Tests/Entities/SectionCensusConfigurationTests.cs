// Copyright 2021 Ellucian Company L.P. and its affiliates.using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionCensusConfigurationTests
    {
        private LastDateAttendedNeverAttendedFieldDisplayType lastDateAttendedNeverAttendedCensusRoster;
        private List<CensusDatePositionSubmission> censusDatePositionSubmissions;

        [TestInitialize]
        public void SectionCensusConfigurationTests_Initialize()
        {
            lastDateAttendedNeverAttendedCensusRoster = LastDateAttendedNeverAttendedFieldDisplayType.Editable;
            censusDatePositionSubmissions = new List<CensusDatePositionSubmission>()
            {
                null, // Nulls should be handled gracefully
                new CensusDatePositionSubmission(1, string.Empty, null),
                new CensusDatePositionSubmission(2, "2nd Census", null),
                new CensusDatePositionSubmission(4, "4th Census", 10),
                new CensusDatePositionSubmission(5, "5th Census", 0),
            };
        }

        [TestMethod]
        public void SectionCensusConfigurationTests_Null_CensusDatePositionSubmission_DoesNotThrow()
        {
            var sectionCensusConfiguration = new SectionCensusConfiguration(lastDateAttendedNeverAttendedCensusRoster, null, "D");
            Assert.AreEqual(lastDateAttendedNeverAttendedCensusRoster, sectionCensusConfiguration.LastDateAttendedNeverAttendedCensusRoster);
            Assert.AreEqual(0, sectionCensusConfiguration.CensusDatePositionSubmissionRange.Count);
            Assert.AreEqual("D", sectionCensusConfiguration.FacultyDropReasonCode);
        }

        [TestMethod]
        public void SectionCensusConfigurationTests_Empty_CensusDatePositionSubmission_DoesNotThrow()
        {
            var sectionCensusConfiguration = new SectionCensusConfiguration(lastDateAttendedNeverAttendedCensusRoster, new List<CensusDatePositionSubmission>(), "D");
            Assert.AreEqual(lastDateAttendedNeverAttendedCensusRoster, sectionCensusConfiguration.LastDateAttendedNeverAttendedCensusRoster);
            Assert.AreEqual(0, sectionCensusConfiguration.CensusDatePositionSubmissionRange.Count);
            Assert.AreEqual("D", sectionCensusConfiguration.FacultyDropReasonCode);
        }

        [TestMethod]
        public void SectionCensusConfigurationTests_Null_FacultyDropReasonCode_DoesNotThrow()
        {
            var sectionCensusConfiguration = new SectionCensusConfiguration(lastDateAttendedNeverAttendedCensusRoster, censusDatePositionSubmissions, null);
            Assert.AreEqual(lastDateAttendedNeverAttendedCensusRoster, sectionCensusConfiguration.LastDateAttendedNeverAttendedCensusRoster);
            Assert.AreEqual(4, sectionCensusConfiguration.CensusDatePositionSubmissionRange.Count);
            Assert.AreEqual(null, sectionCensusConfiguration.FacultyDropReasonCode);
        }

        [TestMethod]
        public void SectionCensusConfigurationTests_Empty_FacultyDropReasonCode_DoesNotThrow()
        {
            var sectionCensusConfiguration = new SectionCensusConfiguration(lastDateAttendedNeverAttendedCensusRoster, censusDatePositionSubmissions, string.Empty);
            Assert.AreEqual(lastDateAttendedNeverAttendedCensusRoster, sectionCensusConfiguration.LastDateAttendedNeverAttendedCensusRoster);
            Assert.AreEqual(4, sectionCensusConfiguration.CensusDatePositionSubmissionRange.Count);
            Assert.AreEqual(string.Empty, sectionCensusConfiguration.FacultyDropReasonCode);
        }

        [TestMethod]
        public void SectionCensusConfigurationTests_CensusDatePositionSubmission_Valid()
        {
            var sectionCensusConfiguration = new SectionCensusConfiguration(lastDateAttendedNeverAttendedCensusRoster, censusDatePositionSubmissions, "D");
            Assert.AreEqual(lastDateAttendedNeverAttendedCensusRoster, sectionCensusConfiguration.LastDateAttendedNeverAttendedCensusRoster);
            Assert.AreEqual(4, sectionCensusConfiguration.CensusDatePositionSubmissionRange.Count);

            for (int i = 1; i < censusDatePositionSubmissions.Count; i++)
            {
                var expectedSubmission = censusDatePositionSubmissions[i];
                var actualSubmission = sectionCensusConfiguration.CensusDatePositionSubmissionRange.ElementAt(i-1);

                Assert.AreEqual(expectedSubmission.Position, actualSubmission.Position);
                Assert.AreEqual(expectedSubmission.Label, actualSubmission.Label);
                Assert.AreEqual(expectedSubmission.CertifyDaysBeforeOffset, actualSubmission.CertifyDaysBeforeOffset);
            }

            Assert.AreEqual("D", sectionCensusConfiguration.FacultyDropReasonCode);
        }
    }
}
