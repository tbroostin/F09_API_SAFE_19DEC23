//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/25/2019 3:07:55 PM by user pbhaumik2
//
//     Type: CTX
//     Transaction ID: GET.PERSON.SEARCH.KEY.LIST
//     Application: CORE
//     Environment: devcoll
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
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.SEARCH.KEY.LIST", GeneratedDateTime = "6/25/2019 3:07:55 PM", User = "pbhaumik2")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonSearchKeyListRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEARCH.STRING", InBoundData = true)]        
		public string SearchString { get; set; }

		public GetPersonSearchKeyListRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.SEARCH.KEY.LIST", GeneratedDateTime = "6/25/2019 3:07:55 PM", User = "pbhaumik2")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonSearchKeyListResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "KEY.LIST", OutBoundData = true)]        
		public List<string> KeyList { get; set; }

		public GetPersonSearchKeyListResponse()
		{	
			KeyList = new List<string>();
		}
	}
}