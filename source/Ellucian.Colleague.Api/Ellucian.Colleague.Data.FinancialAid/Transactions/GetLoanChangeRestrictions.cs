//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 2:21:01 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.LOAN.CHANGE.RESTRICTIONS
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

namespace Ellucian.Colleague.Data.FinancialAid.Transactions
{
	[DataContract]
	public class AwardData
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AWARDS", OutBoundData = true)]
		public string Awards { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AWARD.CATEGORIES", OutBoundData = true)]
		public string AwardCategories { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AWARD.CHANGEABLE", OutBoundData = true)]
		public string AwardChangeable { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FAILED.ELIG.RULE.MESSAGES", OutBoundData = true)]
		public string FailedRuleMessages { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.LOAN.CHANGE.RESTRICTIONS", GeneratedDateTime = "10/4/2017 2:21:01 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetLoanChangeRestrictionsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "YEAR", InBoundData = true)]        
		public string Year { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		public GetLoanChangeRestrictionsRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.LOAN.CHANGE.RESTRICTIONS", GeneratedDateTime = "10/4/2017 2:21:01 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetLoanChangeRestrictionsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FROZEN.AWARD.PERIODS", OutBoundData = true)]        
		public List<string> FrozenAwardPeriods { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AWARDS", OutBoundData = true)]
		public List<AwardData> AwardData { get; set; }

		public GetLoanChangeRestrictionsResponse()
		{	
			FrozenAwardPeriods = new List<string>();
			AwardData = new List<AwardData>();
		}
	}
}
