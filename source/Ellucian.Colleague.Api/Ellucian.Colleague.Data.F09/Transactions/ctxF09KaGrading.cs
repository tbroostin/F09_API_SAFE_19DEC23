//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/24/2019 4:26:06 PM by user tshadley
//
//     Type: CTX
//     Transaction ID: XCTX.F09.SS.KA.GRADING
//     Application: ST
//     Environment: F09 ProjDB
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

namespace Ellucian.Colleague.Data.F09.Transactions
{
	[DataContract]
	public class GradeOptions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GRADE.CODE", OutBoundData = true)]
		public string GradeCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GRADE.DESC", OutBoundData = true)]
		public string GradeDesc { get; set; }
	}

	[DataContract]
	public class Questions
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.QUESTION", InBoundData = true, OutBoundData = true)]
		public string QQuestion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.ANSWERS", OutBoundData = true)]
		public string QAnswers { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.HEADER", OutBoundData = true)]
		public string QHeader { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.ANSWER.TYPE", OutBoundData = true)]
		public string QAnswerType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.COMMENT", InBoundData = true, OutBoundData = true)]
		public string QComment { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.REQUIRED", OutBoundData = true)]
		public string QRequired { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.TAG", InBoundData = true, OutBoundData = true)]
		public string QTag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.Q.ANSWER", InBoundData = true, OutBoundData = true)]
		public string QAnswer { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.KA.GRADING", GeneratedDateTime = "6/24/2019 4:26:06 PM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09KaGradingRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.FAC.ID", InBoundData = true)]        
		public string FacId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STC.ID", InBoundData = true)]        
		public string StcId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQUEST.TYPE", InBoundData = true)]        
		public string RequestType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.KA.COMMENTS", InBoundData = true)]        
		public string KaComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.GRADE", InBoundData = true)]        
		public string GradeSelected { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.Q.QUESTION", InBoundData = true)]
		public List<Questions> Questions { get; set; }

		public ctxF09KaGradingRequest()
		{	
			Questions = new List<Questions>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.KA.GRADING", GeneratedDateTime = "6/24/2019 4:26:06 PM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09KaGradingResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.FAC.ID", OutBoundData = true)]        
		public string FacId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STC.ID", OutBoundData = true)]        
		public string StcId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.RESPOND.TYPE", OutBoundData = true)]        
		public string RespondType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.MSG", OutBoundData = true)]        
		public string ErrorMsg { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.KA.HEADER.HTML", OutBoundData = true)]        
		public string KaHeaderHtml { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.GRADE.CODE", OutBoundData = true)]
		public List<GradeOptions> GradeOptions { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.Q.QUESTION", OutBoundData = true)]
		public List<Questions> Questions { get; set; }

		public ctxF09KaGradingResponse()
		{	
			GradeOptions = new List<GradeOptions>();
			Questions = new List<Questions>();
		}
	}
}
