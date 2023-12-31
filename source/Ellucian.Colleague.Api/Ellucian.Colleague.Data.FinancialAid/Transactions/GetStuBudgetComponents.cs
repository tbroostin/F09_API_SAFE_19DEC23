//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/14/2018 12:44:22 PM by user otorres
//
//     Type: CTX
//     Transaction ID: GET.STU.BUDGET.COMPONENTS
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

namespace Ellucian.Colleague.Data.FinancialAid.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.STU.BUDGET.COMPONENTS", GeneratedDateTime = "12/14/2018 12:44:22 PM", User = "otorres")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetStuBudgetComponentsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "YEAR", InBoundData = true)]        
		public string Year { get; set; }

		public GetStuBudgetComponentsRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.STU.BUDGET.COMPONENTS", GeneratedDateTime = "12/14/2018 12:44:22 PM", User = "otorres")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetStuBudgetComponentsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STU.BGT.COMPONENT.CODES", OutBoundData = true)]        
		public List<string> StudentBudgetComponents { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STU.BGT.COMPONENT.ORIG.AMTS", OutBoundData = true)]        
		public List<string> StuBgtComponentOrigAmts { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STU.BGT.COMPONENT.OVR.AMTS", OutBoundData = true)]        
		public List<string> StuBgtComponentOvrAmts { get; set; }

		public GetStuBudgetComponentsResponse()
		{	
			StudentBudgetComponents = new List<string>();
			StuBgtComponentOrigAmts = new List<string>();
			StuBgtComponentOvrAmts = new List<string>();
		}
	}
}
