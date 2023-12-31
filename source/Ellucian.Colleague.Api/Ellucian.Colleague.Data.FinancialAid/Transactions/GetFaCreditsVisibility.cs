//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/3/2022 4:57:22 PM by user nravioli
//
//     Type: CTX
//     Transaction ID: GET.FA.CREDITS.VISIBILITY
//     Application: ST
//     Environment: DVColl
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
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.FA.CREDITS.VISIBILITY", GeneratedDateTime = "8/3/2022 4:57:22 PM", User = "nravioli")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetFaCreditsVisibilityRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "FA.YEAR", InBoundData = true)]        
		public string FaYear { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "RULE.TABLE.ID", InBoundData = true)]        
		public string RuleTableId { get; set; }

		public GetFaCreditsVisibilityRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.FA.CREDITS.VISIBILITY", GeneratedDateTime = "8/3/2022 4:57:22 PM", User = "nravioli")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetFaCreditsVisibilityResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUTPUT.VISIBILITY", OutBoundData = true)]        
		public string OutputVisibility { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MSGS", OutBoundData = true)]        
		public List<string> ErrorMsgs { get; set; }

		public GetFaCreditsVisibilityResponse()
		{	
			ErrorMsgs = new List<string>();
		}
	}
}
