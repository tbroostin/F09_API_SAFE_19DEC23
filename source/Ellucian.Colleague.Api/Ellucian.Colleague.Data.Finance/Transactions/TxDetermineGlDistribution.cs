//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:43:53 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: TX.DETERMINE.GL.DISTRIBUTION
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
	[ColleagueDataContract(ColleagueId = "TX.DETERMINE.GL.DISTRIBUTION", GeneratedDateTime = "10/4/2017 1:43:53 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class TxDetermineGlDistributionRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.PERSON.ID", InBoundData = true)]        
		public string InPersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.AR.TYPE", InBoundData = true)]        
		public List<string> InArType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.CALLING.PROCESS", InBoundData = true)]        
		public string InCallingProcess { get; set; }

		public TxDetermineGlDistributionRequest()
		{	
			InArType = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.DETERMINE.GL.DISTRIBUTION", GeneratedDateTime = "10/4/2017 1:43:53 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class TxDetermineGlDistributionResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.DISTRIBUTION", OutBoundData = true)]        
		public List<string> OutDistribution { get; set; }

		public TxDetermineGlDistributionResponse()
		{	
			OutDistribution = new List<string>();
		}
	}
}
