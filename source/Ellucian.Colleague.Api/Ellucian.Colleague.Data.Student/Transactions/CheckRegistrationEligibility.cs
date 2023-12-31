//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/5/2017 2:25:56 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: CHECK.REGISTRATION.ELIGIBILITY
//     Application: ST
//     Environment: dvColl
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
	public class Terms
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.CODE", OutBoundData = true)]
		public string TermCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.ADD.ALLOWED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool TermAddAllowed { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.ADD.MESSAGES", OutBoundData = true)]
		public string TermAddMessages { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "TERM.ADD.CHECK.DATE", OutBoundData = true)]
		public Nullable<DateTime> TermAddCheckDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.CHECK.PRIORITY", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool TermCheckPriority { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.PRIORITY.OVRD", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool TermPriorityOverride { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.SKIP.WAITLIST.ALLOWED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool TermSkipWaitlistAllowed { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.REG.RULES.FAILED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool TermRegRulesFailed { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CHECK.REGISTRATION.ELIGIBILITY", GeneratedDateTime = "10/5/2017 2:25:56 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class CheckRegistrationEligibilityRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		public CheckRegistrationEligibilityRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CHECK.REGISTRATION.ELIGIBILITY", GeneratedDateTime = "10/5/2017 2:25:56 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class CheckRegistrationEligibilityResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ELIGIBLE", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool Eligible { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MESSAGES", OutBoundData = true)]        
		public List<string> Messages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HAS.OVERRIDE", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool HasOverride { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:TERM.CODE", OutBoundData = true)]
		public List<Terms> Terms { get; set; }

		public CheckRegistrationEligibilityResponse()
		{	
			Messages = new List<string>();
			Terms = new List<Terms>();
		}
	}
}
