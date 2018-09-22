//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 9/4/2018 9:15:57 AM by user asainju1
//
//     Type: CTX
//     Transaction ID: UPDATE.VOUCHERS.INTEGRATION
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
	public class LineItems
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.DESCRIPTION", InBoundData = true)]
		public string LineItemDescription { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.COMMODITY.CODE", InBoundData = true)]
		public string LineItemsCommodityCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.QUANTITY", InBoundData = true)]
		public string LineItemsQuantity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.UNIT.MEASURED", InBoundData = true)]
		public string LineItemsUnitMeasured { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N4}")]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.UNIT.PRICE", InBoundData = true)]
		public Nullable<Decimal> LineItemsUnitPrice { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.TAX.CODE", InBoundData = true)]
		public string LineItemsTaxCode { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.DISC.AMT", InBoundData = true)]
		public Nullable<Decimal> LineItemsDiscAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.DISC.PERCENT", InBoundData = true)]
		public string LineItemsDiscPercent { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEMS.COMMENT", InBoundData = true)]
		public string LineItemsComment { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.PO.ID", InBoundData = true)]
		public string LineItemPoId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.FINAL.PAYMENT.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]
		public bool LineItemFinalPaymentFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.BPO.ID", InBoundData = true)]
		public string LineItemBpoId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.RCVS.ID", InBoundData = true)]
		public string LineItemRcvsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.DOC.ID", InBoundData = true)]
		public string LineItemDocId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.ITEM.ID", InBoundData = true)]
		public string LineItemItemId { get; set; }
	}

	[DataContract]
	public class LineItemAccountDetails
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "LIA.ITEM.SEQUENCE", InBoundData = true)]
		public string LiaItemSequence { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LIA.ACCOUNTING.STRING", InBoundData = true)]
		public string LiaAccountingString { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "LIA.AMOUNT", InBoundData = true)]
		public Nullable<Decimal> LiaAmount { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LIA.QUANTITY", InBoundData = true)]
		public string LiaQuantity { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N3}")]
		[SctrqDataMember(AppServerName = "LIA.PERCENTAGE", InBoundData = true)]
		public Nullable<Decimal> LiaPercentage { get; set; }
	}

	[DataContract]
	public class UpdateVouchersIntegrationErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.VOUCHERS.INTEGRATION", GeneratedDateTime = "9/4/2018 9:15:57 AM", User = "asainju1")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class UpdateVouchersIntegrationRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VOU.ID", InBoundData = true)]        
		public string VouId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VOUCHER.GUID", InBoundData = true)]        
		public string VoucherGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.ID", InBoundData = true)]        
		public string VendorId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.ADDRESS.ID", InBoundData = true)]        
		public string VendorAddressId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.TYPE", InBoundData = true)]        
		public string VendorType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.NAME", InBoundData = true)]        
		public List<string> VendorName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.ADDRESS", InBoundData = true)]        
		public List<string> VendorAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.CITY", InBoundData = true)]        
		public string VendorCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.STATE", InBoundData = true)]        
		public string VendorState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.ZIP", InBoundData = true)]        
		public string VendorZip { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.COUNTRY", InBoundData = true)]        
		public string VendorCountry { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "REFERENCE.NO", InBoundData = true)]        
		public string ReferenceNo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.INVOICE.NUMBER", InBoundData = true)]        
		public string VendorInvoiceNumber { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "TRANSACTION.DATE", InBoundData = true)]        
		public Nullable<DateTime> TransactionDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "VENDOR.INVOICE.DATE", InBoundData = true)]        
		public Nullable<DateTime> VendorInvoiceDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "VOID.DATE", InBoundData = true)]        
		public Nullable<DateTime> VoidDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PAYMENT.STATUS", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool PaymentStatus { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "VENDOR.BILLED.AMT.VALUE", InBoundData = true)]        
		public Nullable<Decimal> VendorBilledAmtValue { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VENDOR.BILLED.AMT.CURRENCY", InBoundData = true)]        
		public string VendorBilledAmtCurrency { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "INVOICE.DISC.AMT", InBoundData = true)]        
		public Nullable<Decimal> InvoiceDiscAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PAYMENT.SOURCE.ID", InBoundData = true)]        
		public string PaymentSourceId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PAYMENT.DUE", InBoundData = true)]        
		public Nullable<DateTime> PaymentDue { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PAYMENT.TERMS", InBoundData = true)]        
		public string PaymentTerms { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INVOICE.COMMENT", InBoundData = true)]        
		public string InvoiceComment { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SUBMITTED.BY", InBoundData = true)]        
		public string SubmittedBy { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LINE.ITEM.SEQUENCE.NUMBER", InBoundData = true)]        
		public List<string> LineItemSequenceNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LIA.SUBMITTED.BY", InBoundData = true)]        
		public List<string> LiaSubmittedBy { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "POPULATE.TAX.FORM", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool PopulateTaxForm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BYPASS.APPROVAL.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool ByPassApprovalFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:LINE.ITEM.DESCRIPTION", InBoundData = true)]
		public List<LineItems> LineItems { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:LIA.ACCOUNTING.STRING", InBoundData = true)]
		public List<LineItemAccountDetails> LineItemAccountDetails { get; set; }

		public UpdateVouchersIntegrationRequest()
		{	
			VendorName = new List<string>();
			VendorAddress = new List<string>();
			LineItemSequenceNumber = new List<string>();
			LiaSubmittedBy = new List<string>();
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
			LineItems = new List<LineItems>();
			LineItemAccountDetails = new List<LineItemAccountDetails>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.VOUCHERS.INTEGRATION", GeneratedDateTime = "9/4/2018 9:15:57 AM", User = "asainju1")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class UpdateVouchersIntegrationResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VOU.ID", OutBoundData = true)]        
		public string VouId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VOUCHER.GUID", OutBoundData = true)]        
		public string VoucherGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<UpdateVouchersIntegrationErrors> UpdateVouchersIntegrationErrors { get; set; }

		public UpdateVouchersIntegrationResponse()
		{	
			UpdateVouchersIntegrationErrors = new List<UpdateVouchersIntegrationErrors>();
		}
	}
}
