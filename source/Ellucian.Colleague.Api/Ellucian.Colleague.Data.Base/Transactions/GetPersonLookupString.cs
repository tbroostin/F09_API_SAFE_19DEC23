//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 12:50:41 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.PERSON.LOOKUP.STRING
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.LOOKUP.STRING", GeneratedDateTime = "10/4/2017 12:50:41 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonLookupStringRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SEARCH.STRING", InBoundData = true)]        
		public string SearchString { get; set; }

		public GetPersonLookupStringRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PERSON.LOOKUP.STRING", GeneratedDateTime = "10/4/2017 12:50:41 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetPersonLookupStringResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "INDEX.STRING", OutBoundData = true)]        
		public string IndexString { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		public GetPersonLookupStringResponse()
		{	
		}
	}
}
