//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 2:22:40 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: UPDATE.OUTSIDE.AWARD
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

namespace Ellucian.Colleague.Data.FinancialAid.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.OUTSIDE.AWARD", GeneratedDateTime = "10/4/2017 2:22:40 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateOutsideAwardRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "OUTSIDE.AWARD.ID", InBoundData = true)]        
		public string OutsideAwardId { get; set; }

		[DataMember(IsRequired = true)]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "OUTSIDE.AWARD.AMOUNT", InBoundData = true)]        
		public Nullable<Decimal> OutsideAwardAmount { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "OUTSIDE.AWARD.NAME", InBoundData = true)]        
		public string OutsideAwardName { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "OUTSIDE.AWARD.TYPE", InBoundData = true)]        
		public string OutsideAwardType { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "OUTSIDE.AWARD.FUNDING.SOURCE", InBoundData = true)]        
		public string OutsideAwardFundingSource { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "OUTSIDE.AWARD.YEAR", InBoundData = true)]        
		public string OutsideAwardYear { get; set; }

		public UpdateOutsideAwardRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.OUTSIDE.AWARD", GeneratedDateTime = "10/4/2017 2:22:40 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateOutsideAwardResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "OUTSIDE.AWARD.ID", OutBoundData = true)]        
		public string OutsideAwardId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODE", OutBoundData = true)]        
		public string ErrorCode { get; set; }

		public UpdateOutsideAwardResponse()
		{	
		}
	}
}
