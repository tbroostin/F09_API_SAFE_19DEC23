//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:38:37 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: SFX010
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
	[ColleagueDataContract(ColleagueId = "SFX010", GeneratedDateTime = "10/4/2017 1:38:37 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class GetEcheckPayerRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.PERSON.ID", InBoundData = true)]        
		public string InPersonId { get; set; }

		public GetEcheckPayerRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "SFX010", GeneratedDateTime = "10/4/2017 1:38:37 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class GetEcheckPayerResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.FIRST.NAME", OutBoundData = true)]        
		public string OutFirstName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.MIDDLE.NAME", OutBoundData = true)]        
		public string OutMiddleName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.LAST.NAME", OutBoundData = true)]        
		public string OutLastName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.STREET", OutBoundData = true)]        
		public string OutStreet { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.CITY", OutBoundData = true)]        
		public string OutCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.STATE", OutBoundData = true)]        
		public string OutState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.POSTAL.CODE", OutBoundData = true)]        
		public string OutPostalCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.COUNTRY", OutBoundData = true)]        
		public string OutCountry { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.EMAIL", OutBoundData = true)]        
		public string OutEmail { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.TELEPHONE", OutBoundData = true)]        
		public string OutTelephone { get; set; }

		public GetEcheckPayerResponse()
		{	
		}
	}
}
