//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 2/16/2022 3:34:27 PM by user nravioli
//
//     Type: CTX
//     Transaction ID: GET.CUSTOM.SUBR.VERBIAGE
//     Application: ST
//     Environment: DVColl
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

namespace Ellucian.Colleague.Data.FinancialAid.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.CUSTOM.SUBR.VERBIAGE", GeneratedDateTime = "2/16/2022 3:34:27 PM", User = "nravioli")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetCustomSubrVerbiageRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "YEAR", InBoundData = true)]        
		public string Year { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "SUBROUTINE.NAME", InBoundData = true)]        
		public string SubroutineName { get; set; }

		public GetCustomSubrVerbiageRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.CUSTOM.SUBR.VERBIAGE", GeneratedDateTime = "2/16/2022 3:34:27 PM", User = "nravioli")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetCustomSubrVerbiageResponse
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
		[SctrqDataMember(AppServerName = "CUSTOM.VERBIAGE", OutBoundData = true)]        
		public string CustomVerbiage { get; set; }

		public GetCustomSubrVerbiageResponse()
		{	
		}
	}
}