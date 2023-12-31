//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/8/2018 11:55:15 AM by user tshadley
//
//     Type: CTX
//     Transaction ID: XCTX.GET.ACTIVE.RESTRICTIONS
//     Application: ST
//     Environment: FGU3
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

namespace Ellucian.Colleague.Data.F09.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.GET.ACTIVE.RESTRICTIONS", GeneratedDateTime = "10/8/2018 11:55:15 AM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxGetActiveRestrictionsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ID", InBoundData = true)]        
		public string Id { get; set; }

		public ctxGetActiveRestrictionsRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.GET.ACTIVE.RESTRICTIONS", GeneratedDateTime = "10/8/2018 11:55:15 AM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxGetActiveRestrictionsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RESTRICTIONS", OutBoundData = true)]        
		public List<string> ActiveRestrictions { get; set; }

		public ctxGetActiveRestrictionsResponse()
		{	
			ActiveRestrictions = new List<string>();
		}
	}
}
