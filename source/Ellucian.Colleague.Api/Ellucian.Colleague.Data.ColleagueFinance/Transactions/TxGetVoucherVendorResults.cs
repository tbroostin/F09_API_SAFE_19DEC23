//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/6/2021 10:56:14 AM by user favas_mk
//
//     Type: CTX
//     Transaction ID: TX.GET.VOUCHER.VENDOR.RESULTS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.Transactions
{
	[DataContract]
	public class VoucherVendorSearchResults
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.IDS", OutBoundData = true)]
		public string AlVendorIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.NAMES", OutBoundData = true)]
		public string AlVendorNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.ADDR.IDS", OutBoundData = true)]
		public string AlVendorAddrIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.FORMATTED.ADDRESSES", OutBoundData = true)]
		public string AlVendorFormattedAddresses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.ADDRESS", OutBoundData = true)]
		public string AlVendorAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VEND.ADDR.TYPE.CODES", OutBoundData = true)]
		public string AlVendAddrTypeCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VEND.ADDR.TYPE.DESC", OutBoundData = true)]
		public string AlVendAddrTypeDesc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.CITY", OutBoundData = true)]
		public string AlVendorCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.STATE", OutBoundData = true)]
		public string AlVendorState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.ZIP", OutBoundData = true)]
		public string AlVendorZip { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.COUNTRY", OutBoundData = true)]
		public string AlVendorCountry { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.TAX.FORM", OutBoundData = true)]
		public string AlVendorTaxForm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.TAX.FORM.CODE", OutBoundData = true)]
		public string AlVendorTaxFormCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.TAX.FORM.LOC", OutBoundData = true)]
		public string AlVendorTaxFormLoc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.1099MI.WTHHLD", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool AlVendor1099miWthhld { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.1099NEC.WTHHLD", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]
		public bool AlVendor1099necWthhld { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VEN.AP.TYPES", OutBoundData = true)]
		public string AlVenApTypes { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.GET.VOUCHER.VENDOR.RESULTS", GeneratedDateTime = "1/6/2021 10:56:14 AM", User = "favas_mk")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxGetVoucherVendorResultsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.SEARCH.CRITERIA", InBoundData = true)]        
		public string ASearchCriteria { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.AP.TYPE", InBoundData = true)]        
		public string AApType { get; set; }

		public TxGetVoucherVendorResultsRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.GET.VOUCHER.VENDOR.RESULTS", GeneratedDateTime = "1/6/2021 10:56:14 AM", User = "favas_mk")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxGetVoucherVendorResultsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CORP.SEARCH.CRITERIA", OutBoundData = true)]        
		public string ACorpSearchCriteria { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool AError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> AlErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.VENDOR.IDS", OutBoundData = true)]
		public List<VoucherVendorSearchResults> VoucherVendorSearchResults { get; set; }

		public TxGetVoucherVendorResultsResponse()
		{	
			AlErrorMessages = new List<string>();
			VoucherVendorSearchResults = new List<VoucherVendorSearchResults>();
		}
	}
}
