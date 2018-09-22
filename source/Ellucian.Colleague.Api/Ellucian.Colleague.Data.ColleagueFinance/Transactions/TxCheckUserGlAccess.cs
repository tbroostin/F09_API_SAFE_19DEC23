//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:25:40 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: TX.CHECK.USER.GL.ACCESS
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
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.CHECK.USER.GL.ACCESS", GeneratedDateTime = "10/4/2017 1:25:40 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxCheckUserGlAccessRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IO.GL.ACCT.IDS", InBoundData = true)]        
		public List<string> IoGlAcctIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.PERSON.ID", InBoundData = true)]        
		public string InPersonId { get; set; }

		public TxCheckUserGlAccessRequest()
		{	
			IoGlAcctIds = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.CHECK.USER.GL.ACCESS", GeneratedDateTime = "10/4/2017 1:25:40 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxCheckUserGlAccessResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IO.GL.ACCT.IDS", OutBoundData = true)]        
		public List<string> IoGlAcctIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.GL.ACCTS.ACCESSED", OutBoundData = true)]        
		public List<string> OutGlAcctsAccessed { get; set; }

		public TxCheckUserGlAccessResponse()
		{	
			IoGlAcctIds = new List<string>();
			OutGlAcctsAccessed = new List<string>();
		}
	}
}
