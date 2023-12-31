//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/6/2019 3:46:40 PM by user sobel
//
//     Type: CTX
//     Transaction ID: GET.POTENTIAL.D7.FINANCIAL.AID
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

namespace Ellucian.Colleague.Data.Finance.Transactions
{
	[DataContract]
	public class PotentialD7FinancialAid
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "POTENTIAL.AWD.PRD.AWARDS", OutBoundData = true)]
		public string PotentialAwdPrdAwards { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "POTENTIAL.AWD.PRD.AWARD.DESCS", OutBoundData = true)]
		public string PotentialAwdPrdAwardDescriptions { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "POTENTIAL.AWD.PRD.AWARD.AMOUNTS", OutBoundData = true)]
		public Nullable<Decimal> PotentialAwdPrdAwardAmounts { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.POTENTIAL.D7.FINANCIAL.AID", GeneratedDateTime = "5/6/2019 3:46:40 PM", User = "sobel")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetPotentialD7FinancialAidRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.ID", InBoundData = true)]        
		public string TermId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "AWD.PRD.AWARDS.TO.EVALUATE", InBoundData = true)]        
		public List<string> AwdPrdAwardsToEvaluate { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "AWD.PRD.AWARDS.XMIT.EXCESS.IND", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public List<bool> AwdPrdAwardsXmitExcessInd { get; set; }

		public GetPotentialD7FinancialAidRequest()
		{	
			AwdPrdAwardsToEvaluate = new List<string>();
			AwdPrdAwardsXmitExcessInd = new List<bool>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.POTENTIAL.D7.FINANCIAL.AID", GeneratedDateTime = "5/6/2019 3:46:40 PM", User = "sobel")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetPotentialD7FinancialAidResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ABORT.MESSAGE", OutBoundData = true)]        
		public string AbortMessage { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:POTENTIAL.AWD.PRD.AWARDS", OutBoundData = true)]
		public List<PotentialD7FinancialAid> PotentialD7FinancialAid { get; set; }

		public GetPotentialD7FinancialAidResponse()
		{	
			PotentialD7FinancialAid = new List<PotentialD7FinancialAid>();
		}
	}
}
