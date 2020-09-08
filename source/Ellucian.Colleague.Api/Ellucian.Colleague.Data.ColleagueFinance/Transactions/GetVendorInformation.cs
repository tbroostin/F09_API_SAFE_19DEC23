//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/15/2019 11:34:45 AM by user vaidyasubrahmanyadv
//
//     Type: CTX
//     Transaction ID: GET.VENDOR.INFORMATION
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
	public class VendorContactSummary
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CONTACT.VENDOR.ID", OutBoundData = true)]
		public string ContactVendorId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.CONTACT.NAME", OutBoundData = true)]
		public string VendorContactName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.CONTACT.TITLE", OutBoundData = true)]
		public string VendorContactTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.CONTACT.PHONE", OutBoundData = true)]
		public string VendorContactPhone { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.CONTACT.EMAIL", OutBoundData = true)]
		public string VendorContactEmail { get; set; }
	}

	[DataContract]
	public class VendorInfo
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.ID", OutBoundData = true)]
		public string VendorId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.NAME", OutBoundData = true)]
		public string VendorName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.ADDRESS", OutBoundData = true)]
		public string VendorAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.CITY", OutBoundData = true)]
		public string VendorCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.STATE", OutBoundData = true)]
		public string VendorState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.COUNTRY", OutBoundData = true)]
		public string VendorCountry { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.ZIP", OutBoundData = true)]
		public string VendorZip { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.VENDOR.INFORMATION", GeneratedDateTime = "11/15/2019 11:34:45 AM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class GetVendorInformationRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.IDS", InBoundData = true)]        
		public List<string> VendorIds { get; set; }

		public GetVendorInformationRequest()
		{	
			VendorIds = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.VENDOR.INFORMATION", GeneratedDateTime = "11/15/2019 11:34:45 AM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class GetVendorInformationResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", OutBoundData = true)]        
		public string Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.VENDOR.IDS", OutBoundData = true)]        
		public List<string> VendorIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.CONTACT.VENDOR.ID", OutBoundData = true)]
		public List<VendorContactSummary> VendorContactSummary { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.VENDOR.ID", OutBoundData = true)]
		public List<VendorInfo> VendorInfo { get; set; }

		public GetVendorInformationResponse()
		{	
			ErrorMessages = new List<string>();
			VendorIds = new List<string>();
			VendorContactSummary = new List<VendorContactSummary>();
			VendorInfo = new List<VendorInfo>();
		}
	}
}
