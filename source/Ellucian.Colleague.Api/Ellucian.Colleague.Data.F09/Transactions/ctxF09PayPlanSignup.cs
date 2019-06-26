//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.1
//     Last generated on 6/18/2019 8:57:27 PM by user tshadley
//
//     Type: CTX
//     Transaction ID: XCTX.F09.SS.PAY.PLAN.SIGNUP
//     Application: ST
//     Environment: F09 Fielding Dev
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

namespace Ellucian.Colleague.Data.F09.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.1")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.PAY.PLAN.SIGNUP", GeneratedDateTime = "6/18/2019 8:57:27 PM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09PayPlanSignupRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.ID", InBoundData = true)]        
		public string Id { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.REQUEST.TYPE", InBoundData = true)]        
		public string RequestType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION.SELECTED", InBoundData = true)]        
		public string SuOptionSelected { get; set; }

		public ctxF09PayPlanSignupRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.1")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.PAY.PLAN.SIGNUP", GeneratedDateTime = "6/18/2019 8:57:27 PM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09PayPlanSignupResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.RESPOND.TYPE", OutBoundData = true)]        
		public string RespondType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.MSG", OutBoundData = true)]        
		public string ErrorMsg { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.INSTRUCTIONS", OutBoundData = true)]        
		public string SuInstructions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.TERMS.AND.COND", OutBoundData = true)]        
		public string SuTermsAndCond { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.FA.TERMS", OutBoundData = true)]        
		public string SuFaTerms { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION1.VALUE", OutBoundData = true)]        
		public string SuOption1Value { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION1.DESC", OutBoundData = true)]        
		public string SuOption1Desc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION1.INV.TER", OutBoundData = true)]        
		public string SuOption1InvTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION1.ERROR", OutBoundData = true)]        
		public string SuOption1Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION2.VALUE", OutBoundData = true)]        
		public string SuOption2Value { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION2.DESC", OutBoundData = true)]        
		public string SuOption2Desc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION2.INV.TER", OutBoundData = true)]        
		public string SuOption2InvTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.OPTION2.ERROR", OutBoundData = true)]        
		public string SuOption2Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.INV.AR.CODE", OutBoundData = true)]        
		public string SuInvArCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.INV.AMOUNT", OutBoundData = true)]        
		public string SuInvAmount { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SU.INV.DESC", OutBoundData = true)]        
		public string SuInvDesc { get; set; }

		public ctxF09PayPlanSignupResponse()
		{	
		}
	}
}
