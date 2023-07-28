//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/11/2023 2:51:46 PM by user nravioli
//
//     Type: CTX
//     Transaction ID: INTERACT.WITH.HOUSING
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
	[ColleagueDataContract(ColleagueId = "INTERACT.WITH.HOUSING", GeneratedDateTime = "4/11/2023 2:51:46 PM", User = "nravioli")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InteractWithHousingRequest
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
		[SctrqDataMember(AppServerName = "AWARD.YEAR", InBoundData = true)]        
		public string AwardYear { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "GET.SET", InBoundData = true)]        
		public string GetSet { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.CODE", InBoundData = true)]        
		public string HousingCode { get; set; }

		public InteractWithHousingRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INTERACT.WITH.HOUSING", GeneratedDateTime = "4/11/2023 2:51:46 PM", User = "nravioli")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InteractWithHousingResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.CODE", OutBoundData = true)]        
		public string HousingCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]        
		public string ErrorMessages { get; set; }

		public InteractWithHousingResponse()
		{	
		}
	}
}