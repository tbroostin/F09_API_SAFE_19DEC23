//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/27/2020 3:23:13 PM by user spirest
//
//     Type: CTX
//     Transaction ID: UPDT.RA.CASE
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
	public class EmailAddresses
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.ADDRESSES", InBoundData = true)]
		public string AlEmailAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.NAMES", InBoundData = true)]
		public string AlEmailNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.TYPES", InBoundData = true)]
		public string AlEmailTypes { get; set; }
	}

	[DataContract]
	public class ReassignCase
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.REASSIGN.TO", InBoundData = true)]
		public string AlReassignTo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.REASSIGN.TO.IS.ROLE", InBoundData = true)]
		public string AlReassignToIsRole { get; set; }
	}

	[DataContract]
	public class ManageReminderDates
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CASE.ITEMS.ID", InBoundData = true)]
		public string AlCaseItemsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CLEAR.REMINDER.DATES", InBoundData = true)]
		public string AlClearReminderDates { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDT.RA.CASE", GeneratedDateTime = "3/27/2020 3:23:13 PM", User = "spirest")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class UpdtRaCaseRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.CASES.ID", InBoundData = true)]        
		public string ACasesId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.UPDATED.BY", InBoundData = true)]        
		public string AUpdatedBy { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ACTION", InBoundData = true)]        
		public string AAction { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.COMM.CODE", InBoundData = true)]        
		public string ACommCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PRIORITY", InBoundData = true)]        
		public string APriority { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CASE.TYPE", InBoundData = true)]        
		public string ACaseType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CLOSURE.REASON", InBoundData = true)]        
		public string AClosureReason { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.REMINDER.DATE", InBoundData = true)]        
		public Nullable<DateTime> AReminderDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SUMMARY", InBoundData = true)]        
		public string ASummary { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.NOTES", InBoundData = true)]        
		public List<string> AlNotes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.EMAIL.SUBJECT", InBoundData = true)]        
		public string AEmailSubject { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.EMAIL.BODY", InBoundData = true)]        
		public string AEmailBody { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.EMAIL.ADDRESSES", InBoundData = true)]
		public List<EmailAddresses> EmailAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.REASSIGN.TO", InBoundData = true)]
		public List<ReassignCase> ReassignCase { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.CASE.ITEMS.ID", InBoundData = true)]
		public List<ManageReminderDates> ManageReminderDates { get; set; }

		public UpdtRaCaseRequest()
		{	
			AlNotes = new List<string>();
			EmailAddresses = new List<EmailAddresses>();
			ReassignCase = new List<ReassignCase>();
			ManageReminderDates = new List<ManageReminderDates>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDT.RA.CASE", GeneratedDateTime = "3/27/2020 3:23:13 PM", User = "spirest")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class UpdtRaCaseResponse
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

		public UpdtRaCaseResponse()
		{	
			AlErrorMessages = new List<string>();
		}
	}
}
