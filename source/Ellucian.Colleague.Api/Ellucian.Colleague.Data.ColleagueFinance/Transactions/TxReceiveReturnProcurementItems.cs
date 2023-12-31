//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/16/2019 2:24:03 PM by user vaidyasubrahmanyadv
//
//     Type: CTX
//     Transaction ID: TX.RECEIVE.RETURN.PROCUREMENT.ITEMS
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
	public class ItemInformation
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PO.ID", InBoundData = true, OutBoundData = true)]
		public string PoId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDORS", InBoundData = true)]
		public string Vendor { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PO.NUMBER", InBoundData = true, OutBoundData = true)]
		public string PoNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ITEM.ID", InBoundData = true, OutBoundData = true)]
		public string ItemId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ITEM.DESC", InBoundData = true, OutBoundData = true)]
		public string ItemDesc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.QUANTITY.ORDERED", InBoundData = true)]
		public string QuantityOrdered { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.QUANTITY.ACCEPTED", InBoundData = true)]
		public string QuantityAccepted { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ITEM.MSDS.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true, OutBoundData = true)]
		public bool ItemMsdsFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ITEM.MSDS.RECEIVED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true, OutBoundData = true)]
		public bool ItemMsdsReceived { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.QUANTITY.REJECTED", InBoundData = true)]
		public string QuantityRejected { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.RETURN.DATE", InBoundData = true)]
		public Nullable<DateTime> ReturnDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RETURN.VIA", InBoundData = true)]
		public string ReturnVia { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RETURN.AUTH.NUMBER", InBoundData = true)]
		public string ReturnAuthorizationNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RETURN.REASON", InBoundData = true)]
		public string ReturnReason { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RETURN.COMMENTS", InBoundData = true)]
		public string ReturnComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.REORDER", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]
		public bool Reorder { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CONFIRMATION.EMAIL", InBoundData = true)]
		public string ConfirmationEmail { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.RECEIVE.RETURN.PROCUREMENT.ITEMS", GeneratedDateTime = "12/16/2019 2:24:03 PM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxReceiveReturnProcurementItemsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STAFF.USER.ID", InBoundData = true)]        
		public string StaffUserId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ACCEPT.ALL", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool AcceptAll { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.IS.PO.FILTER.APPLIED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool IsPoFilterApplied { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PACKING.SLIP", InBoundData = true)]        
		public string PackingSlip { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ARRIVED.VIA", InBoundData = true)]        
		public string ArrivedVia { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.ITEM.DESC", InBoundData = true)]
		public List<ItemInformation> ItemInformation { get; set; }

		public TxReceiveReturnProcurementItemsRequest()
		{	
			ItemInformation = new List<ItemInformation>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.RECEIVE.RETURN.PROCUREMENT.ITEMS", GeneratedDateTime = "12/16/2019 2:24:03 PM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxReceiveReturnProcurementItemsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.WARNING.MESSAGES", OutBoundData = true)]        
		public List<string> WarningMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.WARNING.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool WarningOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.ITEM.DESC", OutBoundData = true)]
		public List<ItemInformation> ItemInformation { get; set; }

		public TxReceiveReturnProcurementItemsResponse()
		{	
			WarningMessages = new List<string>();
			ErrorMessages = new List<string>();
			ItemInformation = new List<ItemInformation>();
		}
	}
}
