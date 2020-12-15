//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/30/2020 1:45:17 PM by user asainju1
//
//     Type: CTX
//     Transaction ID: GET.PERSON.HIERARCHY.ADDRESSES
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class GetPersonHierarchyAddressesOutput
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "PERSON.IDS", InBoundData = true, OutBoundData = true)]
		public string PersonIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.ID", OutBoundData = true)]
		public string OutAddressId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.LINES", OutBoundData = true)]
		public string OutAddressLines { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.LABEL", OutBoundData = true)]
		public string OutAddressLabel { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.CITY", InBoundData = true, OutBoundData = true)]
		public string OutAddressCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.STATE", OutBoundData = true)]
		public string OutAddressState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.ZIP", OutBoundData = true)]
		public string OutAddressZip { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.COUNTRY", OutBoundData = true)]
		public string OutAddressCountry { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.COUNTRY.DESC", OutBoundData = true)]
		public string OutAddressCountryDesc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.MODIFIER", OutBoundData = true)]
		public string OutAddressModifier { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ADDRESS.ROUTE.CODE", OutBoundData = true)]
		public string OutAddressRouteCode { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.HIERARCHY.ADDRESSES", GeneratedDateTime = "3/30/2020 1:45:17 PM", User = "asainju1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 2)]
	public class GetPersonHierarchyAddressesRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.HIERARCHY", InBoundData = true)]        
		public string InHierarchy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "IN.DATE", InBoundData = true)]        
		public Nullable<DateTime> InDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:OUT.ADDRESS.CITY", InBoundData = true)]
		public List<GetPersonHierarchyAddressesOutput> GetPersonHierarchyAddressesOutput { get; set; }

		public GetPersonHierarchyAddressesRequest()
		{	
			GetPersonHierarchyAddressesOutput = new List<GetPersonHierarchyAddressesOutput>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.HIERARCHY.ADDRESSES", GeneratedDateTime = "3/30/2020 1:45:17 PM", User = "asainju1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 2)]
	public class GetPersonHierarchyAddressesResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:OUT.ADDRESS.CITY", OutBoundData = true)]
		public List<GetPersonHierarchyAddressesOutput> GetPersonHierarchyAddressesOutput { get; set; }

		public GetPersonHierarchyAddressesResponse()
		{	
			GetPersonHierarchyAddressesOutput = new List<GetPersonHierarchyAddressesOutput>();
		}
	}
}