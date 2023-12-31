//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/14/2022 11:33:17 AM by user rebecca.rowland
//
//     Type: ENTITY
//     Entity: COURSE.SECTIONS
//     Application: ST
//     Environment: dvcoll
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Ellucian.Dmi.Runtime;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CourseSections")]
	[ColleagueDataContract(GeneratedDateTime = "4/14/2022 11:33:17 AM", User = "rebecca.rowland")]
	[EntityDataContract(EntityName = "COURSE.SECTIONS", EntityType = "PHYS")]
	public class CourseSections : IColleagueGuidEntity
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		/// <summary>
		/// Record Key
		/// </summary>
		[DataMember]
		public string Recordkey { get; set; }
		
		public void setKey(string key)
		{
			Recordkey = key;
		}
	
		/// <summary>
		/// Record GUID
		/// </summary>
		[DataMember(Name = "RecordGuid")]
		public string RecordGuid { get; set; }

		/// <summary>
		/// Record Model Name
		/// </summary>
		[DataMember(Name = "RecordModelName")]
		public string RecordModelName { get; set; }	
		
		/// <summary>
		/// CDD Name: SEC.SHORT.TITLE
		/// </summary>
		[DataMember(Order = 0, Name = "SEC.SHORT.TITLE")]
		public string SecShortTitle { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.LOCATION
		/// </summary>
		[DataMember(Order = 2, Name = "SEC.LOCATION")]
		public string SecLocation { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CAPACITY
		/// </summary>
		[DataMember(Order = 4, Name = "SEC.CAPACITY")]
		public int? SecCapacity { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.SUBJECT
		/// </summary>
		[DataMember(Order = 5, Name = "SEC.SUBJECT")]
		public string SecSubject { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CEUS
		/// </summary>
		[DataMember(Order = 6, Name = "SEC.CEUS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? SecCeus { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.MIN.CRED
		/// </summary>
		[DataMember(Order = 7, Name = "SEC.MIN.CRED")]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? SecMinCred { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.MAX.CRED
		/// </summary>
		[DataMember(Order = 8, Name = "SEC.MAX.CRED")]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? SecMaxCred { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ONLY.PASS.NOPASS.FLAG
		/// </summary>
		[DataMember(Order = 10, Name = "SEC.ONLY.PASS.NOPASS.FLAG")]
		public string SecOnlyPassNopassFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.MEETING
		/// </summary>
		[DataMember(Order = 11, Name = "SEC.MEETING")]
		public List<string> SecMeeting { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OTHER.REG.BILLING.RATES
		/// </summary>
		[DataMember(Order = 13, Name = "SEC.OTHER.REG.BILLING.RATES")]
		public List<string> SecOtherRegBillingRates { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.TERM
		/// </summary>
		[DataMember(Order = 14, Name = "SEC.TERM")]
		public string SecTerm { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.INSTR.METHODS
		/// </summary>
		[DataMember(Order = 15, Name = "SEC.INSTR.METHODS")]
		public List<string> SecInstrMethods { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COURSE.NO
		/// </summary>
		[DataMember(Order = 16, Name = "SEC.COURSE.NO")]
		public string SecCourseNo { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.NO
		/// </summary>
		[DataMember(Order = 17, Name = "SEC.NO")]
		public string SecNo { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.DEPTS
		/// </summary>
		[DataMember(Order = 18, Name = "SEC.DEPTS")]
		public List<string> SecDepts { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.BILLING.CRED
		/// </summary>
		[DataMember(Order = 23, Name = "SEC.BILLING.CRED")]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? SecBillingCred { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COURSE
		/// </summary>
		[DataMember(Order = 24, Name = "SEC.COURSE")]
		public string SecCourse { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COREQ.SECTIONS
		/// </summary>
		[DataMember(Order = 26, Name = "SEC.COREQ.SECTIONS")]
		public List<string> SecCoreqSections { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COURSE.TYPES
		/// </summary>
		[DataMember(Order = 27, Name = "SEC.COURSE.TYPES")]
		public List<string> SecCourseTypes { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ACAD.LEVEL
		/// </summary>
		[DataMember(Order = 29, Name = "SEC.ACAD.LEVEL")]
		public string SecAcadLevel { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COURSE.LEVELS
		/// </summary>
		[DataMember(Order = 30, Name = "SEC.COURSE.LEVELS")]
		public List<string> SecCourseLevels { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.XLIST
		/// </summary>
		[DataMember(Order = 31, Name = "SEC.XLIST")]
		public string SecXlist { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.LOAD
		/// </summary>
		[DataMember(Order = 32, Name = "SEC.LOAD")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> SecLoad { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CRED.TYPE
		/// </summary>
		[DataMember(Order = 35, Name = "SEC.CRED.TYPE")]
		public string SecCredType { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CONTACT.HOURS
		/// </summary>
		[DataMember(Order = 36, Name = "SEC.CONTACT.HOURS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> SecContactHours { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ALLOW.PASS.NOPASS.FLAG
		/// </summary>
		[DataMember(Order = 37, Name = "SEC.ALLOW.PASS.NOPASS.FLAG")]
		public string SecAllowPassNopassFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ALLOW.AUDIT.FLAG
		/// </summary>
		[DataMember(Order = 38, Name = "SEC.ALLOW.AUDIT.FLAG")]
		public string SecAllowAuditFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.FACULTY
		/// </summary>
		[DataMember(Order = 39, Name = "SEC.FACULTY")]
		public List<string> SecFaculty { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.GRADE.SCHEME
		/// </summary>
		[DataMember(Order = 40, Name = "SEC.GRADE.SCHEME")]
		public string SecGradeScheme { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COREQ.SEC.REQD.FLAG
		/// </summary>
		[DataMember(Order = 41, Name = "SEC.COREQ.SEC.REQD.FLAG")]
		public List<string> SecCoreqSecReqdFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COREQ.COURSES
		/// </summary>
		[DataMember(Order = 43, Name = "SEC.COREQ.COURSES")]
		public List<string> SecCoreqCourses { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COREQ.COURSES.REQD.FLAG
		/// </summary>
		[DataMember(Order = 44, Name = "SEC.COREQ.COURSES.REQD.FLAG")]
		public List<string> SecCoreqCoursesReqdFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.STATUS.DATE
		/// </summary>
		[DataMember(Order = 45, Name = "SEC.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SecStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.START.DATE
		/// </summary>
		[DataMember(Order = 46, Name = "SEC.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.END.DATE
		/// </summary>
		[DataMember(Order = 47, Name = "SEC.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.TOPIC.CODE
		/// </summary>
		[DataMember(Order = 55, Name = "SEC.TOPIC.CODE")]
		public string SecTopicCode { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.BILLING.PERIOD.TYPE
		/// </summary>
		[DataMember(Order = 56, Name = "SEC.BILLING.PERIOD.TYPE")]
		public string SecBillingPeriodType { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.SYNONYM
		/// </summary>
		[DataMember(Order = 60, Name = "SEC.SYNONYM")]
		public string SecSynonym { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CONTACT.MEASURES
		/// </summary>
		[DataMember(Order = 61, Name = "SEC.CONTACT.MEASURES")]
		public List<string> SecContactMeasures { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ACTIVE.STUDENTS
		/// </summary>
		[DataMember(Order = 65, Name = "SEC.ACTIVE.STUDENTS")]
		public List<string> SecActiveStudents { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.VAR.CRED.INCREMENT
		/// </summary>
		[DataMember(Order = 66, Name = "SEC.VAR.CRED.INCREMENT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? SecVarCredIncrement { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.CENSUS.DATES
		/// </summary>
		[DataMember(Order = 71, Name = "SEC.OVR.CENSUS.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SecOvrCensusDates { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ALLOW.WAITLIST.FLAG
		/// </summary>
		[DataMember(Order = 72, Name = "SEC.ALLOW.WAITLIST.FLAG")]
		public string SecAllowWaitlistFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.WAITLIST.MAX
		/// </summary>
		[DataMember(Order = 73, Name = "SEC.WAITLIST.MAX")]
		public int? SecWaitlistMax { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.WAITLIST.RATING
		/// </summary>
		[DataMember(Order = 74, Name = "SEC.WAITLIST.RATING")]
		public string SecWaitlistRating { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.BILLING.METHOD
		/// </summary>
		[DataMember(Order = 76, Name = "SEC.BILLING.METHOD")]
		public string SecBillingMethod { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.REG.START.DATE
		/// </summary>
		[DataMember(Order = 80, Name = "SEC.OVR.REG.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrRegStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.REG.END.DATE
		/// </summary>
		[DataMember(Order = 81, Name = "SEC.OVR.REG.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrRegEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.ADD.START.DATE
		/// </summary>
		[DataMember(Order = 82, Name = "SEC.OVR.ADD.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrAddStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.ADD.END.DATE
		/// </summary>
		[DataMember(Order = 83, Name = "SEC.OVR.ADD.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrAddEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.PREREG.START.DATE
		/// </summary>
		[DataMember(Order = 84, Name = "SEC.OVR.PREREG.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrPreregStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.PREREG.END.DATE
		/// </summary>
		[DataMember(Order = 85, Name = "SEC.OVR.PREREG.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrPreregEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.DROP.END.DATE
		/// </summary>
		[DataMember(Order = 86, Name = "SEC.OVR.DROP.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrDropEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.STATUS
		/// </summary>
		[DataMember(Order = 87, Name = "SEC.STATUS")]
		public List<string> SecStatus { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.DEPT.PCTS
		/// </summary>
		[DataMember(Order = 88, Name = "SEC.DEPT.PCTS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> SecDeptPcts { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.TRANSFER.STATUS
		/// </summary>
		[DataMember(Order = 90, Name = "SEC.TRANSFER.STATUS")]
		public string SecTransferStatus { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.FACULTY.CONSENT.FLAG
		/// </summary>
		[DataMember(Order = 93, Name = "SEC.FACULTY.CONSENT.FLAG")]
		public string SecFacultyConsentFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.PRINTED.COMMENTS
		/// </summary>
		[DataMember(Order = 94, Name = "SEC.PRINTED.COMMENTS")]
		public string SecPrintedComments { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CALENDAR.SCHEDULES
		/// </summary>
		[DataMember(Order = 95, Name = "SEC.CALENDAR.SCHEDULES")]
		public List<string> SecCalendarSchedules { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CLOCK.HOURS
		/// </summary>
		[DataMember(Order = 96, Name = "SEC.CLOCK.HOURS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> SecClockHours { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.DROP.GR.REQD.DATE
		/// </summary>
		[DataMember(Order = 97, Name = "SEC.OVR.DROP.GR.REQD.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrDropGrReqdDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.NAME
		/// </summary>
		[DataMember(Order = 98, Name = "SEC.NAME")]
		public string SecName { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVR.DROP.START.DATE
		/// </summary>
		[DataMember(Order = 100, Name = "SEC.OVR.DROP.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecOvrDropStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.NO.WEEKS
		/// </summary>
		[DataMember(Order = 102, Name = "SEC.NO.WEEKS")]
		public int? SecNoWeeks { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.WAITLIST.NO.DAYS
		/// </summary>
		[DataMember(Order = 145, Name = "SEC.WAITLIST.NO.DAYS")]
		public int? SecWaitlistNoDays { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.FIRST.MEETING.DATE
		/// </summary>
		[DataMember(Order = 146, Name = "SEC.FIRST.MEETING.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecFirstMeetingDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.LAST.MEETING.DATE
		/// </summary>
		[DataMember(Order = 147, Name = "SEC.LAST.MEETING.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SecLastMeetingDate { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.PORTAL.SITE
		/// </summary>
		[DataMember(Order = 150, Name = "SEC.PORTAL.SITE")]
		public string SecPortalSite { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.BOOKS
		/// </summary>
		[DataMember(Order = 155, Name = "SEC.BOOKS")]
		public List<string> SecBooks { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.BOOK.OPTIONS
		/// </summary>
		[DataMember(Order = 156, Name = "SEC.BOOK.OPTIONS")]
		public List<string> SecBookOptions { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.LEARNING.PROVIDER
		/// </summary>
		[DataMember(Order = 157, Name = "SEC.LEARNING.PROVIDER")]
		public string SecLearningProvider { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CLOSE.WAITLIST.FLAG
		/// </summary>
		[DataMember(Order = 161, Name = "SEC.CLOSE.WAITLIST.FLAG")]
		public string SecCloseWaitlistFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.MIN.NO.COREQ.SECS
		/// </summary>
		[DataMember(Order = 162, Name = "SEC.MIN.NO.COREQ.SECS")]
		public int? SecMinNoCoreqSecs { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.COREQ.SECS
		/// </summary>
		[DataMember(Order = 163, Name = "SEC.COREQ.SECS")]
		public List<string> SecCoreqSecs { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.RECOMMENDED.SECS
		/// </summary>
		[DataMember(Order = 164, Name = "SEC.RECOMMENDED.SECS")]
		public List<string> SecRecommendedSecs { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.OVERRIDE.CRS.REQS.FLAG
		/// </summary>
		[DataMember(Order = 165, Name = "SEC.OVERRIDE.CRS.REQS.FLAG")]
		public string SecOverrideCrsReqsFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.REQS
		/// </summary>
		[DataMember(Order = 169, Name = "SEC.REQS")]
		public List<string> SecReqs { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.HIDE.IN.CATALOG
		/// </summary>
		[DataMember(Order = 171, Name = "SEC.HIDE.IN.CATALOG")]
		public string SecHideInCatalog { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ADD.AUTH.EXCLUSION.FLAG
		/// </summary>
		[DataMember(Order = 172, Name = "SEC.ADD.AUTH.EXCLUSION.FLAG")]
		public string SecAddAuthExclusionFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.ATTEND.TRACKING.TYPE
		/// </summary>
		[DataMember(Order = 173, Name = "SEC.ATTEND.TRACKING.TYPE")]
		public string SecAttendTrackingType { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.GRADE.SUBSCHEMES.ID
		/// </summary>
		[DataMember(Order = 174, Name = "SEC.GRADE.SUBSCHEMES.ID")]
		public string SecGradeSubschemesId { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CERT.CENSUS.DATES
		/// </summary>
		[DataMember(Order = 175, Name = "SEC.CERT.CENSUS.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SecCertCensusDates { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CERT.RECORDED.DATES
		/// </summary>
		[DataMember(Order = 176, Name = "SEC.CERT.RECORDED.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SecCertRecordedDates { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CERT.RECORDED.TIMES
		/// </summary>
		[DataMember(Order = 177, Name = "SEC.CERT.RECORDED.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SecCertRecordedTimes { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CERT.PERSON.IDS
		/// </summary>
		[DataMember(Order = 178, Name = "SEC.CERT.PERSON.IDS")]
		public List<string> SecCertPersonIds { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CERT.POSITIONS
		/// </summary>
		[DataMember(Order = 179, Name = "SEC.CERT.POSITIONS")]
		public List<string> SecCertPositions { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.CERT.POSITION.LABELS
		/// </summary>
		[DataMember(Order = 180, Name = "SEC.CERT.POSITION.LABELS")]
		public List<string> SecCertPositionLabels { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.SHOW.DROP.ROSTER.FLAG
		/// </summary>
		[DataMember(Order = 181, Name = "SEC.SHOW.DROP.ROSTER.FLAG")]
		public string SecShowDropRosterFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.GRADE.BY.RANDOM.ID.FLAG
		/// </summary>
		[DataMember(Order = 182, Name = "SEC.GRADE.BY.RANDOM.ID.FLAG")]
		public string SecGradeByRandomIdFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SEC.REOPEN.ATTENDANCE.FLAG
		/// </summary>
		[DataMember(Order = 183, Name = "SEC.REOPEN.ATTENDANCE.FLAG")]
		public string SecReopenAttendanceFlag { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CourseSectionsSecContact> SecContactEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CourseSectionsSecDepartments> SecDepartmentsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CourseSectionsSecCoreqs> SecCoreqsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CourseSectionsSecCourseCoreqs> SecCourseCoreqsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CourseSectionsSecStatuses> SecStatusesEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CourseSectionsSecCertCensus> SecCertCensusEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: SEC.CONTACT
			
			SecContactEntityAssociation = new List<CourseSectionsSecContact>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SecInstrMethods != null)
			{
				int numSecContact = SecInstrMethods.Count;
				if (SecLoad !=null && SecLoad.Count > numSecContact) numSecContact = SecLoad.Count;
				if (SecContactHours !=null && SecContactHours.Count > numSecContact) numSecContact = SecContactHours.Count;
				if (SecContactMeasures !=null && SecContactMeasures.Count > numSecContact) numSecContact = SecContactMeasures.Count;
				if (SecClockHours !=null && SecClockHours.Count > numSecContact) numSecContact = SecClockHours.Count;

				for (int i = 0; i < numSecContact; i++)
				{

					string value0 = "";
					if (SecInstrMethods != null && i < SecInstrMethods.Count)
					{
						value0 = SecInstrMethods[i];
					}


					Decimal? value1 = null;
					if (SecLoad != null && i < SecLoad.Count)
					{
						value1 = SecLoad[i];
					}


					Decimal? value2 = null;
					if (SecContactHours != null && i < SecContactHours.Count)
					{
						value2 = SecContactHours[i];
					}


					string value3 = "";
					if (SecContactMeasures != null && i < SecContactMeasures.Count)
					{
						value3 = SecContactMeasures[i];
					}


					Decimal? value4 = null;
					if (SecClockHours != null && i < SecClockHours.Count)
					{
						value4 = SecClockHours[i];
					}

					SecContactEntityAssociation.Add(new CourseSectionsSecContact( value0, value1, value2, value3, value4));
				}
			}
			// EntityAssociation Name: SEC.DEPARTMENTS
			
			SecDepartmentsEntityAssociation = new List<CourseSectionsSecDepartments>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SecDepts != null)
			{
				int numSecDepartments = SecDepts.Count;
				if (SecDeptPcts !=null && SecDeptPcts.Count > numSecDepartments) numSecDepartments = SecDeptPcts.Count;

				for (int i = 0; i < numSecDepartments; i++)
				{

					string value0 = "";
					if (SecDepts != null && i < SecDepts.Count)
					{
						value0 = SecDepts[i];
					}


					Decimal? value1 = null;
					if (SecDeptPcts != null && i < SecDeptPcts.Count)
					{
						value1 = SecDeptPcts[i];
					}

					SecDepartmentsEntityAssociation.Add(new CourseSectionsSecDepartments( value0, value1));
				}
			}
			// EntityAssociation Name: SEC.COREQS
			
			SecCoreqsEntityAssociation = new List<CourseSectionsSecCoreqs>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SecCoreqSections != null)
			{
				int numSecCoreqs = SecCoreqSections.Count;
				if (SecCoreqSecReqdFlag !=null && SecCoreqSecReqdFlag.Count > numSecCoreqs) numSecCoreqs = SecCoreqSecReqdFlag.Count;

				for (int i = 0; i < numSecCoreqs; i++)
				{

					string value0 = "";
					if (SecCoreqSections != null && i < SecCoreqSections.Count)
					{
						value0 = SecCoreqSections[i];
					}


					string value1 = "";
					if (SecCoreqSecReqdFlag != null && i < SecCoreqSecReqdFlag.Count)
					{
						value1 = SecCoreqSecReqdFlag[i];
					}

					SecCoreqsEntityAssociation.Add(new CourseSectionsSecCoreqs( value0, value1));
				}
			}
			// EntityAssociation Name: SEC.COURSE.COREQS
			
			SecCourseCoreqsEntityAssociation = new List<CourseSectionsSecCourseCoreqs>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SecCoreqCourses != null)
			{
				int numSecCourseCoreqs = SecCoreqCourses.Count;
				if (SecCoreqCoursesReqdFlag !=null && SecCoreqCoursesReqdFlag.Count > numSecCourseCoreqs) numSecCourseCoreqs = SecCoreqCoursesReqdFlag.Count;

				for (int i = 0; i < numSecCourseCoreqs; i++)
				{

					string value0 = "";
					if (SecCoreqCourses != null && i < SecCoreqCourses.Count)
					{
						value0 = SecCoreqCourses[i];
					}


					string value1 = "";
					if (SecCoreqCoursesReqdFlag != null && i < SecCoreqCoursesReqdFlag.Count)
					{
						value1 = SecCoreqCoursesReqdFlag[i];
					}

					SecCourseCoreqsEntityAssociation.Add(new CourseSectionsSecCourseCoreqs( value0, value1));
				}
			}
			// EntityAssociation Name: SEC.STATUSES
			
			SecStatusesEntityAssociation = new List<CourseSectionsSecStatuses>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SecStatus != null)
			{
				int numSecStatuses = SecStatus.Count;
				if (SecStatusDate !=null && SecStatusDate.Count > numSecStatuses) numSecStatuses = SecStatusDate.Count;

				for (int i = 0; i < numSecStatuses; i++)
				{

					DateTime? value0 = null;
					if (SecStatusDate != null && i < SecStatusDate.Count)
					{
						value0 = SecStatusDate[i];
					}


					string value1 = "";
					if (SecStatus != null && i < SecStatus.Count)
					{
						value1 = SecStatus[i];
					}

					SecStatusesEntityAssociation.Add(new CourseSectionsSecStatuses( value0, value1));
				}
			}
			// EntityAssociation Name: SEC.CERT.CENSUS
			
			SecCertCensusEntityAssociation = new List<CourseSectionsSecCertCensus>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SecCertCensusDates != null)
			{
				int numSecCertCensus = SecCertCensusDates.Count;
				if (SecCertRecordedDates !=null && SecCertRecordedDates.Count > numSecCertCensus) numSecCertCensus = SecCertRecordedDates.Count;
				if (SecCertRecordedTimes !=null && SecCertRecordedTimes.Count > numSecCertCensus) numSecCertCensus = SecCertRecordedTimes.Count;
				if (SecCertPersonIds !=null && SecCertPersonIds.Count > numSecCertCensus) numSecCertCensus = SecCertPersonIds.Count;
				if (SecCertPositions !=null && SecCertPositions.Count > numSecCertCensus) numSecCertCensus = SecCertPositions.Count;
				if (SecCertPositionLabels !=null && SecCertPositionLabels.Count > numSecCertCensus) numSecCertCensus = SecCertPositionLabels.Count;

				for (int i = 0; i < numSecCertCensus; i++)
				{

					DateTime? value0 = null;
					if (SecCertCensusDates != null && i < SecCertCensusDates.Count)
					{
						value0 = SecCertCensusDates[i];
					}


					DateTime? value1 = null;
					if (SecCertRecordedDates != null && i < SecCertRecordedDates.Count)
					{
						value1 = SecCertRecordedDates[i];
					}


					DateTime? value2 = null;
					if (SecCertRecordedTimes != null && i < SecCertRecordedTimes.Count)
					{
						value2 = SecCertRecordedTimes[i];
					}


					string value3 = "";
					if (SecCertPersonIds != null && i < SecCertPersonIds.Count)
					{
						value3 = SecCertPersonIds[i];
					}


					string value4 = "";
					if (SecCertPositions != null && i < SecCertPositions.Count)
					{
						value4 = SecCertPositions[i];
					}


					string value5 = "";
					if (SecCertPositionLabels != null && i < SecCertPositionLabels.Count)
					{
						value5 = SecCertPositionLabels[i];
					}

					SecCertCensusEntityAssociation.Add(new CourseSectionsSecCertCensus( value0, value1, value2, value3, value4, value5));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class CourseSectionsSecContact
	{
		public string SecInstrMethodsAssocMember;	
		public Decimal? SecLoadAssocMember;	
		public Decimal? SecContactHoursAssocMember;	
		public string SecContactMeasuresAssocMember;	
		public Decimal? SecClockHoursAssocMember;	
		public CourseSectionsSecContact() {}
		public CourseSectionsSecContact(
			string inSecInstrMethods,
			Decimal? inSecLoad,
			Decimal? inSecContactHours,
			string inSecContactMeasures,
			Decimal? inSecClockHours)
		{
			SecInstrMethodsAssocMember = inSecInstrMethods;
			SecLoadAssocMember = inSecLoad;
			SecContactHoursAssocMember = inSecContactHours;
			SecContactMeasuresAssocMember = inSecContactMeasures;
			SecClockHoursAssocMember = inSecClockHours;
		}
	}
	
	[Serializable]
	public class CourseSectionsSecDepartments
	{
		public string SecDeptsAssocMember;	
		public Decimal? SecDeptPctsAssocMember;	
		public CourseSectionsSecDepartments() {}
		public CourseSectionsSecDepartments(
			string inSecDepts,
			Decimal? inSecDeptPcts)
		{
			SecDeptsAssocMember = inSecDepts;
			SecDeptPctsAssocMember = inSecDeptPcts;
		}
	}
	
	[Serializable]
	public class CourseSectionsSecCoreqs
	{
		public string SecCoreqSectionsAssocMember;	
		public string SecCoreqSecReqdFlagAssocMember;	
		public CourseSectionsSecCoreqs() {}
		public CourseSectionsSecCoreqs(
			string inSecCoreqSections,
			string inSecCoreqSecReqdFlag)
		{
			SecCoreqSectionsAssocMember = inSecCoreqSections;
			SecCoreqSecReqdFlagAssocMember = inSecCoreqSecReqdFlag;
		}
	}
	
	[Serializable]
	public class CourseSectionsSecCourseCoreqs
	{
		public string SecCoreqCoursesAssocMember;	
		public string SecCoreqCoursesReqdFlagAssocMember;	
		public CourseSectionsSecCourseCoreqs() {}
		public CourseSectionsSecCourseCoreqs(
			string inSecCoreqCourses,
			string inSecCoreqCoursesReqdFlag)
		{
			SecCoreqCoursesAssocMember = inSecCoreqCourses;
			SecCoreqCoursesReqdFlagAssocMember = inSecCoreqCoursesReqdFlag;
		}
	}
	
	[Serializable]
	public class CourseSectionsSecStatuses
	{
		public DateTime? SecStatusDateAssocMember;	
		public string SecStatusAssocMember;	
		public CourseSectionsSecStatuses() {}
		public CourseSectionsSecStatuses(
			DateTime? inSecStatusDate,
			string inSecStatus)
		{
			SecStatusDateAssocMember = inSecStatusDate;
			SecStatusAssocMember = inSecStatus;
		}
	}
	
	[Serializable]
	public class CourseSectionsSecCertCensus
	{
		public DateTime? SecCertCensusDatesAssocMember;	
		public DateTime? SecCertRecordedDatesAssocMember;	
		public DateTime? SecCertRecordedTimesAssocMember;	
		public string SecCertPersonIdsAssocMember;	
		public string SecCertPositionsAssocMember;	
		public string SecCertPositionLabelsAssocMember;	
		public CourseSectionsSecCertCensus() {}
		public CourseSectionsSecCertCensus(
			DateTime? inSecCertCensusDates,
			DateTime? inSecCertRecordedDates,
			DateTime? inSecCertRecordedTimes,
			string inSecCertPersonIds,
			string inSecCertPositions,
			string inSecCertPositionLabels)
		{
			SecCertCensusDatesAssocMember = inSecCertCensusDates;
			SecCertRecordedDatesAssocMember = inSecCertRecordedDates;
			SecCertRecordedTimesAssocMember = inSecCertRecordedTimes;
			SecCertPersonIdsAssocMember = inSecCertPersonIds;
			SecCertPositionsAssocMember = inSecCertPositions;
			SecCertPositionLabelsAssocMember = inSecCertPositionLabels;
		}
	}
}