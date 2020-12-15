//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/18/2019 1:15:30 PM by user tglsql
//
//     Type: CTX
//     Transaction ID: TX.UPDATE.WORKING.BUDGET.LINE.ITEMS.2
//     Application: CF
//     Environment: dvcoll_wstst01
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

namespace Ellucian.Colleague.Data.BudgetManagement.Transactions
{
	[DataContract]
	public class budgetLineItems2
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LINE.GL.NO", InBoundData = true, OutBoundData = true)]
		public string AlLineGlNo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LINE.NEW.AMT", InBoundData = true, OutBoundData = true)]
		public Nullable<long> AlLineNewAmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LINE.NOTES", InBoundData = true, OutBoundData = true)]
		public string AlLineNotes { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.UPDATE.WORKING.BUDGET.LINE.ITEMS.2", GeneratedDateTime = "7/18/2019 1:15:30 PM", User = "tglsql")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxUpdateWorkingBudgetLineItems2Request
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.WORKING.BUDGET", InBoundData = true)]        
		public string AWorkingBudget { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ID", InBoundData = true)]        
		public string APersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.LINE.GL.NO", InBoundData = true)]
		public List<budgetLineItems2> budgetLineItems2 { get; set; }

		public TxUpdateWorkingBudgetLineItems2Request()
		{	
			budgetLineItems2 = new List<budgetLineItems2>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.UPDATE.WORKING.BUDGET.LINE.ITEMS.2", GeneratedDateTime = "7/18/2019 1:15:30 PM", User = "tglsql")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxUpdateWorkingBudgetLineItems2Response
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.LINE.GL.NO", OutBoundData = true)]
		public List<budgetLineItems2> budgetLineItems2 { get; set; }

		public TxUpdateWorkingBudgetLineItems2Response()
		{	
			budgetLineItems2 = new List<budgetLineItems2>();
		}
	}
}