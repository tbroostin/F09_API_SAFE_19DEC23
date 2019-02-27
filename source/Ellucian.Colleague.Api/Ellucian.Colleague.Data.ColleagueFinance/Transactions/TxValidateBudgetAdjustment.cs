//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 9/5/2018 2:25:46 PM by user jsullivan
//
//     Type: CTX
//     Transaction ID: TX.VALIDATE.BUDGET.ADJUSTMENT
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.VALIDATE.BUDGET.ADJUSTMENT", GeneratedDateTime = "9/5/2018 2:25:46 PM", User = "jsullivan")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxValidateBudgetAdjustmentRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ID", InBoundData = true)]        
		public string APersonId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.TR.DATE", InBoundData = true)]        
		public Nullable<DateTime> ATrDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REASON", InBoundData = true)]        
		public string AReason { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GL.ACCOUNTS", InBoundData = true)]        
		public List<string> AlGlAccounts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DEBITS", InBoundData = true)]        
		public List<string> AlDebits { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CREDITS", InBoundData = true)]        
		public List<string> AlCredits { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.NEXT.APPROVER.IDS", InBoundData = true)]        
		public List<string> AlNextApproverIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.APPROVAL.IDS", InBoundData = true)]        
		public List<string> AlApprovalIds { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.APPROVAL.DATES", InBoundData = true)]        
		public List<Nullable<DateTime>> AlApprovalDates { get; set; }

		public TxValidateBudgetAdjustmentRequest()
		{	
			AlGlAccounts = new List<string>();
			AlDebits = new List<string>();
			AlCredits = new List<string>();
			AlNextApproverIds = new List<string>();
			AlApprovalIds = new List<string>();
			AlApprovalDates = new List<Nullable<DateTime>>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.VALIDATE.BUDGET.ADJUSTMENT", GeneratedDateTime = "9/5/2018 2:25:46 PM", User = "jsullivan")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxValidateBudgetAdjustmentResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", OutBoundData = true)]        
		public string AError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> AlMessage { get; set; }

		public TxValidateBudgetAdjustmentResponse()
		{	
			AlMessage = new List<string>();
		}
	}
}
