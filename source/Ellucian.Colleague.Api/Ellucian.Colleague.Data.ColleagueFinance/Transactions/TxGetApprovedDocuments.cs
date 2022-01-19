//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/23/2021 5:23:10 PM by user tglsql
//
//     Type: CTX
//     Transaction ID: TX.GET.APPROVED.DOCUMENTS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.Transactions
{
	[DataContract]
	public class AlApprovedDocuments
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DOC.ID", OutBoundData = true)]
		public string AlDocId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DOC.NUMBER", OutBoundData = true)]
		public string AlDocNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DOC.TYPE", OutBoundData = true)]
		public string AlDocType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DOC.VENDOR.NAME", OutBoundData = true)]
		public string AlDocVendorName { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.DOC.DATE", OutBoundData = true)]
		public Nullable<DateTime> AlDocDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DOC.STATUS", OutBoundData = true)]
		public string AlDocStatus { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "AL.DOC.NET.AMT", OutBoundData = true)]
		public Nullable<Decimal> AlDocNetAmt { get; set; }
	}

	[DataContract]
	public class AlDocumentApprovers
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.APPR.DOC.TYPE", OutBoundData = true)]
		public string AlApprDocType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.APPR.DOC.ID", OutBoundData = true)]
		public string AlApprDocId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.APPR.ID", OutBoundData = true)]
		public string AlApprId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.APPR.NAME", OutBoundData = true)]
		public string AlApprName { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.APPR.DATE", OutBoundData = true)]
		public Nullable<DateTime> AlApprDate { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.GET.APPROVED.DOCUMENTS", GeneratedDateTime = "4/23/2021 5:23:10 PM", User = "tglsql")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxGetApprovedDocumentsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.APPROVAL.ID", InBoundData = true)]        
		public string AApprovalId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DOCUMENT.TYPES", InBoundData = true)]        
		public List<string> AlDocumentTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.IDS", InBoundData = true)]        
		public List<string> AlVendorIds { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.DOCUMENT.DATE.FROM", InBoundData = true)]        
		public Nullable<DateTime> ADocumentDateFrom { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.DOCUMENT.DATE.TO", InBoundData = true)]        
		public Nullable<DateTime> ADocumentDateTo { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.APPROVAL.DATE.FROM", InBoundData = true)]        
		public Nullable<DateTime> AApprovalDateFrom { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.APPROVAL.DATE.TO", InBoundData = true)]        
		public Nullable<DateTime> AApprovalDateTo { get; set; }

		public TxGetApprovedDocumentsRequest()
		{	
			AlDocumentTypes = new List<string>();
			AlVendorIds = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.GET.APPROVED.DOCUMENTS", GeneratedDateTime = "4/23/2021 5:23:10 PM", User = "tglsql")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxGetApprovedDocumentsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.DOC.DATE", OutBoundData = true)]
		public List<AlApprovedDocuments> AlApprovedDocuments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.APPR.DOC.TYPE", OutBoundData = true)]
		public List<AlDocumentApprovers> AlDocumentApprovers { get; set; }

		public TxGetApprovedDocumentsResponse()
		{	
			AlApprovedDocuments = new List<AlApprovedDocuments>();
			AlDocumentApprovers = new List<AlDocumentApprovers>();
		}
	}
}