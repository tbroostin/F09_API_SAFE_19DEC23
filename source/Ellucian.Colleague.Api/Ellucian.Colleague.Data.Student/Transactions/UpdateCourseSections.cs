//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/6/2021 12:44:04 PM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: UPDATE.COURSE.SECTIONS
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

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[DataContract]
	public class SecDepartments
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.DEPTS", InBoundData = true)]
		public string SecDepts { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "SEC.DEPT.PCTS", InBoundData = true)]
		public Nullable<Decimal> SecDeptPcts { get; set; }
	}

	[DataContract]
	public class SecStatuses
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.STATUS", InBoundData = true)]
		public string SecStatus { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "SEC.STATUS.DATE", InBoundData = true)]
		public Nullable<DateTime> SecStatusDate { get; set; }
	}

	[DataContract]
	public class UpdateCourseSectionErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[DataContract]
	public class UpdateCourseSectionWarnings
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.CODES", OutBoundData = true)]
		public string WarningCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.MESSAGES", OutBoundData = true)]
		public string WarningMessages { get; set; }
	}

	[DataContract]
	public class SecContact
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.INSTR.METHODS", InBoundData = true)]
		public string SecInstrMethods { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "SEC.CONTACT.HOURS", InBoundData = true)]
		public Nullable<Decimal> SecContactHours { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.CONTACT.MEASURES", InBoundData = true)]
		public string SecContactMeasures { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.COURSE.SECTIONS", GeneratedDateTime = "10/6/2021 12:44:04 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateCourseSectionsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "COURSE.SECTIONS.ID", InBoundData = true)]        
		public string CourseSectionsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.COURSE", InBoundData = true)]        
		public string SecCourse { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.NO", InBoundData = true)]        
		public string SecNo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.SHORT.TITLE", InBoundData = true)]        
		public string SecShortTitle { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "SEC.START.DATE", InBoundData = true)]        
		public Nullable<DateTime> SecStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "SEC.END.DATE", InBoundData = true)]        
		public Nullable<DateTime> SecEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.CRED.TYPE", InBoundData = true)]        
		public string SecCredType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.ACAD.LEVEL", InBoundData = true)]        
		public string SecAcadLevel { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.COURSE.LEVELS", InBoundData = true)]        
		public List<string> SecCourseLevels { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "SEC.CEUS", InBoundData = true)]        
		public Nullable<Decimal> SecCeus { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "SEC.MIN.CRED", InBoundData = true)]        
		public Nullable<Decimal> SecMinCred { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "SEC.MAX.CRED", InBoundData = true)]        
		public Nullable<Decimal> SecMaxCred { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "SEC.VAR.CRED.INCREMENT", InBoundData = true)]        
		public Nullable<Decimal> SecVarCredIncrement { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "SEC.BILLING.CRED", InBoundData = true)]        
		public Nullable<Decimal> SecBillingCred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.LOCATION", InBoundData = true)]        
		public string SecLocation { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.GRADE.SCHEME", InBoundData = true)]        
		public string SecGradeScheme { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.CAPACITY", InBoundData = true)]        
		public Nullable<int> SecCapacity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.ALLOW.PASS.NOPASS.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> SecAllowPassNopassFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.ALLOW.AUDIT.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> SecAllowAuditFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.ONLY.PASS.NOPASS.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> SecOnlyPassNopassFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.ALLOW.WAITLIST.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> SecAllowWaitlistFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.CLOSE.WAITLIST.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> SecCloseWaitlistFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.FACULTY.CONSENT.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> SecFacultyConsentFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.COURSE.TYPES", InBoundData = true)]        
		public List<string> SecCourseTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.NO.WEEKS", InBoundData = true)]        
		public Nullable<int> SecNoWeeks { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.TOPIC.CODE", InBoundData = true)]        
		public string SecTopicCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.TERM", InBoundData = true)]        
		public string SecTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.LEARNING.PROVIDER", InBoundData = true)]        
		public string SecLearningProvider { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "SEC.OVR.CENSUS.DATES", InBoundData = true)]        
		public List<Nullable<DateTime>> SecOvrCensusDates { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.PRINTED.COMMENTS", InBoundData = true)]        
		public string SecPrintedComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.BILLING.METHOD", InBoundData = true)]        
		public string SecBillingMethod { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.WAITLIST.MAX", InBoundData = true)]        
		public Nullable<int> SecWaitlistMax { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.WAITLIST.NO.DAYS", InBoundData = true)]        
		public Nullable<int> SecWaitlistNoDays { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.HIDE.IN.CATALOG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> SecHideInCatalog { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.GUID", InBoundData = true)]        
		public string SecGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.XLIST.FLAG", InBoundData = true)]        
		public string SecXlistFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VERSION", InBoundData = true)]        
		public string Version { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:SEC.DEPTS", InBoundData = true)]
		public List<SecDepartments> SecDepartments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:SEC.STATUS", InBoundData = true)]
		public List<SecStatuses> SecStatuses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:SEC.INSTR.METHODS", InBoundData = true)]
		public List<SecContact> SecContact { get; set; }

		public UpdateCourseSectionsRequest()
		{	
			SecCourseLevels = new List<string>();
			SecCourseTypes = new List<string>();
			SecOvrCensusDates = new List<Nullable<DateTime>>();
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
			SecDepartments = new List<SecDepartments>();
			SecStatuses = new List<SecStatuses>();
			SecContact = new List<SecContact>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.COURSE.SECTIONS", GeneratedDateTime = "10/6/2021 12:44:04 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateCourseSectionsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "COURSE.SECTIONS.ID", OutBoundData = true)]        
		public string CourseSectionsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEC.GUID", OutBoundData = true)]        
		public string SecGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<UpdateCourseSectionErrors> UpdateCourseSectionErrors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:WARNING.CODES", OutBoundData = true)]
		public List<UpdateCourseSectionWarnings> UpdateCourseSectionWarnings { get; set; }

		public UpdateCourseSectionsResponse()
		{	
			UpdateCourseSectionErrors = new List<UpdateCourseSectionErrors>();
			UpdateCourseSectionWarnings = new List<UpdateCourseSectionWarnings>();
		}
	}
}
