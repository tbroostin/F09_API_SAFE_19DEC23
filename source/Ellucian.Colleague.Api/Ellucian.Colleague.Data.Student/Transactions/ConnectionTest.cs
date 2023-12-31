//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/26/2017 12:23:14 PM by user balvano3
//
//     Type: CTX
//     Transaction ID: CRMT14
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CRMT14", GeneratedDateTime = "10/26/2017 12:23:14 PM", User = "balvano3")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ConnectionTestRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CONNVAR.ORG.NAME", InBoundData = true)]        
		public string RecruiterOrganizationName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CONNVAR.ORG.GUID", InBoundData = true)]        
		public string RecruiterOrganizationId { get; set; }

		public ConnectionTestRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CRMT14", GeneratedDateTime = "10/26/2017 12:23:14 PM", User = "balvano3")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ConnectionTestResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VAR2", OutBoundData = true)]        
		public string ResponseServiceURL { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VAR3", OutBoundData = true)]        
		public string Message { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VAR4", OutBoundData = true)]        
		public string Duration { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VAR5", OutBoundData = true)]        
		public string Success { get; set; }

		public ConnectionTestResponse()
		{	
		}
	}
}
