//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/19/2020 8:22:56 AM by user vaidyasubrahmanyadv
//
//     Type: CTX
//     Transaction ID: TX.GET.REIMBURSE.PERSON.ADDRESS
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
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.GET.REIMBURSE.PERSON.ADDRESS", GeneratedDateTime = "3/19/2020 8:22:56 AM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxGetReimbursePersonAddressRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ID", InBoundData = true)]        
		public string APersonId { get; set; }

		public TxGetReimbursePersonAddressRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.GET.REIMBURSE.PERSON.ADDRESS", GeneratedDateTime = "3/19/2020 8:22:56 AM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxGetReimbursePersonAddressResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ID", OutBoundData = true)]        
		public string APersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.NAME", OutBoundData = true)]        
		public string APersonName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ADDR.ID", OutBoundData = true)]        
		public string APersonAddrId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.FORMATTED.ADDRESS", OutBoundData = true)]        
		public string APersonFormattedAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ADDRESS", OutBoundData = true)]        
		public string APersonAddress { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.CITY", OutBoundData = true)]        
		public string APersonCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.STATE", OutBoundData = true)]        
		public string APersonState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.COUNTRY", OutBoundData = true)]        
		public string APersonCountry { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ZIP", OutBoundData = true)]        
		public string APersonZip { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool AError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> AlErrorMessages { get; set; }

		public TxGetReimbursePersonAddressResponse()
		{	
			AlErrorMessages = new List<string>();
		}
	}
}
