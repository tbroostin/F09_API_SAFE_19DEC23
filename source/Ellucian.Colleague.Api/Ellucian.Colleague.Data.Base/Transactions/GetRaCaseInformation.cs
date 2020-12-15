//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/25/2020 9:15:00 AM by user spirest
//
//     Type: CTX
//     Transaction ID: GET.RA.CASE.INFORMATION
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class CaseInformation
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.IDS", OutBoundData = true)]
		public string AlCaseIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENTS", OutBoundData = true)]
		public string AlStudents { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CATEGORIES", OutBoundData = true)]
		public string AlCategories { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CATEGORY.DESCRIPTIONS", OutBoundData = true)]
		public string AlCategoryDescriptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STATUSES", OutBoundData = true)]
		public string AlStatuses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PRIORITIES", OutBoundData = true)]
		public string AlPriorities { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.OWNERS", OutBoundData = true)]
		public string AlCaseOwners { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.OWNER.IDS", OutBoundData = true)]
		public string AlCaseOwnerIds { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.REMINDER.DATES", OutBoundData = true)]
		public Nullable<DateTime> AlReminderDates { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.DATES.CREATED", OutBoundData = true)]
		public Nullable<DateTime> AlDatesCreated { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.TYPES", OutBoundData = true)]
		public string AlCaseTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.METHOD.OF.CONTACTS", OutBoundData = true)]
		public string AlCaseMethodOfContacts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.DAYS.OPEN", OutBoundData = true)]
		public string AlCaseDaysOpen { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.CASE.LAST.ACTION.DATES", OutBoundData = true)]
		public Nullable<DateTime> AlCaseLastActionDates { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.ACTION.COUNTS", OutBoundData = true)]
		public string AlCaseActionCounts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.CLOSED.BY", OutBoundData = true)]
		public string AlCaseClosedBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.CASE.CLOSED.DATE", OutBoundData = true)]
		public Nullable<DateTime> AlCaseClosedDate { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.RA.CASE.INFORMATION", GeneratedDateTime = "3/25/2020 9:15:00 AM", User = "spirest")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetRaCaseInformationRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ADVISOR.ID", InBoundData = true)]        
		public string AAdvisorId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STUDENT.IDS", InBoundData = true)]        
		public List<string> AlStudentIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.IDS.IN", InBoundData = true)]        
		public List<string> AlCaseIdsIn { get; set; }

		public GetRaCaseInformationRequest()
		{	
			AlStudentIds = new List<string>();
			AlCaseIdsIn = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.RA.CASE.INFORMATION", GeneratedDateTime = "3/25/2020 9:15:00 AM", User = "spirest")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetRaCaseInformationResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", OutBoundData = true)]        
		public string AError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> AlErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.CASE.IDS", OutBoundData = true)]
		public List<CaseInformation> CaseInformation { get; set; }

		public GetRaCaseInformationResponse()
		{	
			AlErrorMessages = new List<string>();
			CaseInformation = new List<CaseInformation>();
		}
	}
}