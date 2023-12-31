//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/16/2020 10:24:52 AM by user beaton
//
//     Type: CTX
//     Transaction ID: INST.ENROLL.GET.PMT.TRANS
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
	public class OutConvenienceFees
	{
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
	}

	[DataContract]
	public class OutPaymentMethods
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.PAY.METHODS", OutBoundData = true)]
		public string OutRcptPayMethods { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.PAY.METHOD.DESCS", OutBoundData = true)]
		public string OutRcptPayMethodDescs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.CONTROL.NOS", OutBoundData = true)]
		public string OutRcptControlNos { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.CONFIRM.NOS", OutBoundData = true)]
		public string OutRcptConfirmNos { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.TRANS.NOS", OutBoundData = true)]
		public string OutRcptTransNos { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.TRANS.DESCS", OutBoundData = true)]
		public string OutRcptTransDescs { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "OUT.RCPT.TRANS.AMTS", OutBoundData = true)]
		public Nullable<Decimal> OutRcptTransAmts { get; set; }
	}

	[DataContract]
	public class RegisteredSections
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.REGISTERED.SECTION.IDS", OutBoundData = true)]
		public string OutRegisteredSectionIds { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "OUT.REGISTERED.SECTION.COSTS", OutBoundData = true)]
		public Nullable<Decimal> OutRegisteredSectionCosts { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "OUT.REGISTERED.SECTION.CREDITS", OutBoundData = true)]
		public Nullable<Decimal> OutRegisteredSectionCredits { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "OUT.REGISTERED.SECTION.CEUS", OutBoundData = true)]
		public Nullable<Decimal> OutRegisteredSectionCeus { get; set; }
	}

	[DataContract]
	public class FailedSections
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.FAILED.SECTION.IDS", OutBoundData = true)]
		public string OutFailedSectionIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.FAILED.SECTION.MESSAGES", OutBoundData = true)]
		public string OutFailedSectionMessages { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.GET.PMT.TRANS", GeneratedDateTime = "3/16/2020 10:24:52 AM", User = "beaton")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstEnrollGetCashReceiptAckInfoRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.EC.PAY.TRANS.ID", InBoundData = true)]        
		public string InEcPayTransId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IO.CASH.RCPTS.ID", InBoundData = true)]        
		public string IoCashRcptsId { get; set; }

		public InstEnrollGetCashReceiptAckInfoRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.GET.PMT.TRANS", GeneratedDateTime = "3/16/2020 10:24:52 AM", User = "beaton")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstEnrollGetCashReceiptAckInfoResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IO.CASH.RCPTS.ID", OutBoundData = true)]        
		public string IoCashRcptsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.NO", OutBoundData = true)]        
		public string OutRcptNo { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "OUT.RCPT.DATE", OutBoundData = true)]        
		public Nullable<DateTime> OutRcptDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "OUT.RCPT.TIME", OutBoundData = true)]        
		public Nullable<DateTime> OutRcptTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.MERCHANT.NAME.ADDR", OutBoundData = true)]        
		public List<string> OutMerchantNameAddr { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.MERCHANT.PHONE", OutBoundData = true)]        
		public string OutMerchantPhone { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.MERCHANT.EMAIL", OutBoundData = true)]        
		public string OutMerchantEmail { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.PAYER.ID", OutBoundData = true)]        
		public string OutRcptPayerId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.RCPT.PAYER.NAME", OutBoundData = true)]        
		public string OutRcptPayerName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.NEW.LOGIN.ID", OutBoundData = true)]        
		public string OutNewLoginId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.EXCEPTION.MESSAGE", OutBoundData = true)]        
		public string OutExceptionMessage { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.EC.PROC.STATUS", OutBoundData = true)]        
		public string OutEcProcStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.LOGIN.CREATION.ERRORS", OutBoundData = true)]        
		public List<string> OutLoginCreationErrors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:OUT.CONV.FEE.CODE", OutBoundData = true)]
		public List<OutConvenienceFees> OutConvenienceFees { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:OUT.RCPT.PAY.METHODS", OutBoundData = true)]
		public List<OutPaymentMethods> OutPaymentMethods { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:OUT.REGISTERED.SECTION.IDS", OutBoundData = true)]
		public List<RegisteredSections> RegisteredSections { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:OUT.FAILED.SECTION.IDS", OutBoundData = true)]
		public List<FailedSections> FailedSections { get; set; }

		public InstEnrollGetCashReceiptAckInfoResponse()
		{	
			OutMerchantNameAddr = new List<string>();
			OutLoginCreationErrors = new List<string>();
			OutConvenienceFees = new List<OutConvenienceFees>();
			OutPaymentMethods = new List<OutPaymentMethods>();
			RegisteredSections = new List<RegisteredSections>();
			FailedSections = new List<FailedSections>();
		}
	}
}
