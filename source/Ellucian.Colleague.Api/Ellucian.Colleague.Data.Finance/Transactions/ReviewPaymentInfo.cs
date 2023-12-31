//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:43:05 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: SFX006
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

namespace Ellucian.Colleague.Data.Finance.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "SFX006", GeneratedDateTime = "10/4/2017 1:43:05 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class ReviewPaymentInfoRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.DISTRIBUTION", InBoundData = true)]        
		public string InDistribution { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.PAYMENT.METHOD", InBoundData = true)]        
		public string InPaymentMethod { get; set; }

		[DataMember(IsRequired = true)]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "IN.PAYMENT.AMT", InBoundData = true)]        
		public Nullable<Decimal> InPaymentAmt { get; set; }

		public ReviewPaymentInfoRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "SFX006", GeneratedDateTime = "10/4/2017 1:43:05 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class ReviewPaymentInfoResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "OUT.PROVIDER.ACCT", OutBoundData = true)]        
		public string OutProviderAcct { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.CONV.FEE.CODE", OutBoundData = true)]        
		public string OutConvFeeCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.CONV.FEE.DESC", OutBoundData = true)]        
		public string OutConvFeeDesc { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "OUT.CONV.FEE.AMT", OutBoundData = true)]        
		public Nullable<Decimal> OutConvFeeAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.CONV.FEE.GL.NO", OutBoundData = true)]        
		public string OutConvFeeGlNo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.CONFIRM.TEXT", OutBoundData = true)]        
		public List<string> OutConfirmText { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ERROR.MSG", OutBoundData = true)]        
		public string OutErrorMsg { get; set; }

		public ReviewPaymentInfoResponse()
		{	
			OutConfirmText = new List<string>();
		}
	}
}
