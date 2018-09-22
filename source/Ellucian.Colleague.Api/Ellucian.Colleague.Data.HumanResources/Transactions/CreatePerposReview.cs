//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/16/2018 2:16:27 PM by user dvcoll-srm
//
//     Type: CTX
//     Transaction ID: CREATE.PERPOS.REVIEW
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.Transactions
{
	[DataContract]
	public class CreateReviewErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.PERPOS.REVIEW", GeneratedDateTime = "3/16/2018 2:16:27 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "HR", DataContractVersion = 1)]
	public class CreatePerposReviewRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.GUID", InBoundData = true)]        
		public string PerfEvalGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.PERPOS.ID", InBoundData = true)]        
		public string PerfEvalPerposId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PERF.EVAL.RATINGS.DATE", InBoundData = true)]        
		public Nullable<DateTime> PerfEvalRatingsDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.PERSON.ID", InBoundData = true)]        
		public string PerfEvalPersonId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PERF.EVAL.RATINGS.NXT.DATE", InBoundData = true)]        
		public Nullable<DateTime> PerfEvalRatingsNxtDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.RATINGS", InBoundData = true)]        
		public string PerfEvalRatings { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.RATINGS.CYCLE", InBoundData = true)]        
		public string PerfEvalRatingsCycle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.RATINGS.HRPID", InBoundData = true)]        
		public string PerfEvalRatingsHrpid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.RATINGS.COMMENT", InBoundData = true)]        
		public string PerfEvalRatingsComment { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		public CreatePerposReviewRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.PERPOS.REVIEW", GeneratedDateTime = "3/16/2018 2:16:27 PM", User = "dvcoll-srm")]
	[SctrqDataContract(Application = "HR", DataContractVersion = 1)]
	public class CreatePerposReviewResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.GUID", OutBoundData = true)]        
		public string PerfEvalGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PERF.EVAL.PERPOS.ID", OutBoundData = true)]        
		public string PerfEvalPerposId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "PERF.EVAL.RATINGS.DATE", OutBoundData = true)]        
		public Nullable<DateTime> PerfEvalRatingsDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.CODES", OutBoundData = true)]        
		public List<string> WarningCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.MESSAGES", OutBoundData = true)]        
		public string WarningMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<CreateReviewErrors> CreateReviewErrors { get; set; }

		public CreatePerposReviewResponse()
		{	
			WarningCodes = new List<string>();
			CreateReviewErrors = new List<CreateReviewErrors>();
		}
	}
}
