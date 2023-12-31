//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/9/2020 2:24:49 PM by user asainju1
//
//     Type: CTX
//     Transaction ID: CREATE.PROCUREMENT.RECEIPT
//     Application: CF
//     Environment: coldev
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
	public class CreateProcurementReceiptErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[DataContract]
	public class BpvItems
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "BPV.ITEMS.ID", InBoundData = true)]
		public string PriiItemsId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N3}")]
		[SctrqDataMember(AppServerName = "BPV.RECEIVED.QTY", InBoundData = true)]
		public Nullable<Decimal> PriiReceivedQty { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "BPV.RECEIVED.AMT", InBoundData = true)]
		public Nullable<Decimal> PriiReceivedAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BPV.RECEIVED.AMT.CURRENCY", InBoundData = true)]
		public string PriiReceivedAmtCurrency { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N3}")]
		[SctrqDataMember(AppServerName = "BPV.REJECTED.QTY", InBoundData = true)]
		public Nullable<Decimal> PriiRejectedQty { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "BPV.REJECTED.AMT", InBoundData = true)]
		public Nullable<Decimal> PriiRejectedAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BPV.REJECTED.AMT.CURRENCY", InBoundData = true)]
		public string PriiRejectedAmtCurrency { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BPV.RECEIVING.COMMENTS", InBoundData = true)]
		public string PriiReceivingComments { get; set; }
	}

	[DataContract]
	public class CreateProcurementReceiptWarnings
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.CODES", OutBoundData = true)]
		public string WarningCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.MESSAGES", OutBoundData = true)]
		public string WarningMessages { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.PROCUREMENT.RECEIPT", GeneratedDateTime = "1/9/2020 2:24:49 PM", User = "asainju1")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class CreateProcurementReceiptRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PO.RECEIPT.INTG.ID", InBoundData = true)]        
		public string PoReceiptIntgId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PRI.PO.ID", InBoundData = true)]        
		public string PriPoId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PRI.ARRIVED.VIA", InBoundData = true)]        
		public string PriArrivedVia { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PRI.PACKING.SLIP", InBoundData = true)]        
		public string PriPackingSlip { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PRI.RECEIVED.BY", InBoundData = true)]        
		public string PriReceivedBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PRI.RECEIVED.DATE", InBoundData = true)]        
		public Nullable<DateTime> PriReceivedDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PRI.RECEIVING.COMMENTS", InBoundData = true)]        
		public string PriReceivingComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PRI.GUID", InBoundData = true)]        
		public string PriGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:BPV.ITEMS.ID", InBoundData = true)]
		public List<BpvItems> BpvItems { get; set; }

		public CreateProcurementReceiptRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
			BpvItems = new List<BpvItems>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.PROCUREMENT.RECEIPT", GeneratedDateTime = "1/9/2020 2:24:49 PM", User = "asainju1")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class CreateProcurementReceiptResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PO.RECEIPT.INTG.ID", OutBoundData = true)]        
		public string PoReceiptIntgId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PRI.GUID", OutBoundData = true)]        
		public string PriGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<CreateProcurementReceiptErrors> CreateProcurementReceiptErrors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:WARNING.CODES", OutBoundData = true)]
		public List<CreateProcurementReceiptWarnings> CreateProcurementReceiptWarnings { get; set; }

		public CreateProcurementReceiptResponse()
		{	
			CreateProcurementReceiptErrors = new List<CreateProcurementReceiptErrors>();
			CreateProcurementReceiptWarnings = new List<CreateProcurementReceiptWarnings>();
		}
	}
}
