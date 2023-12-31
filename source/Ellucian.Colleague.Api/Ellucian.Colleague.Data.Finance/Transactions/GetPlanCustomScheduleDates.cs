//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:38:48 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.PLAN.CUSTOM.SCHEDULE.DATES
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
	[ColleagueDataContract(ColleagueId = "GET.PLAN.CUSTOM.SCHEDULE.DATES", GeneratedDateTime = "10/4/2017 1:38:48 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetPlanCustomScheduleDatesRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PERSON.ID", InBoundData = true)]        
		public string PersonId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "AR.TYPE", InBoundData = true)]        
		public string ArType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TERM.ID", InBoundData = true)]        
		public string TermId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PLAN.ID", InBoundData = true)]        
		public string PlanId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "TEMPLATE.ID", InBoundData = true)]        
		public string TemplateId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "DOWN.PMT.DATE", InBoundData = true)]        
		public Nullable<DateTime> DownPmtDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "FIRST.PMT.DATE", InBoundData = true)]        
		public Nullable<DateTime> FirstPmtDate { get; set; }

		public GetPlanCustomScheduleDatesRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.PLAN.CUSTOM.SCHEDULE.DATES", GeneratedDateTime = "10/4/2017 1:38:48 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetPlanCustomScheduleDatesResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "DOWN.PMT.DATE", OutBoundData = true)]        
		public Nullable<DateTime> DownPmtDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "FIRST.PMT.DATE", OutBoundData = true)]        
		public Nullable<DateTime> FirstPmtDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "ADDNL.PMT.DATES", OutBoundData = true)]        
		public List<Nullable<DateTime>> OutAddnlPmtDates { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MSG", OutBoundData = true)]        
		public string ErrorMsg { get; set; }

		public GetPlanCustomScheduleDatesResponse()
		{	
			OutAddnlPmtDates = new List<Nullable<DateTime>>();
		}
	}
}
