//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.1
//     Last generated on 7/30/2020 8:07:58 AM by user tshadley
//
//     Type: CTX
//     Transaction ID: XCTX.F09.SS.EVAL.SELECT
//     Application: ST
//     Environment: F09_TEST18
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
	public class QEval
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.KEY", OutBoundData = true)]
		public string EvalKey { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.TYPE", OutBoundData = true)]
		public string EvalType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.DESC1", OutBoundData = true)]
		public string EvalDesc1 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.DESC2", OutBoundData = true)]
		public string EvalDesc2 { get; set; }
	}

	[DataContract]
	public class EType
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TYPE", OutBoundData = true)]
		public string Type { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TYPE.DESC", OutBoundData = true)]
		public string TypeDesc { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.1")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.EVAL.SELECT", GeneratedDateTime = "7/30/2020 8:07:58 AM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09EvalSelectRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ID", InBoundData = true)]        
		public string Id { get; set; }

		public ctxF09EvalSelectRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.1")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.SS.EVAL.SELECT", GeneratedDateTime = "7/30/2020 8:07:58 AM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09EvalSelectResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ID", OutBoundData = true)]        
		public string Id { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.RESPOND.TYPE", OutBoundData = true)]        
		public string RespondType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.MSG", OutBoundData = true)]        
		public string Msg { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.EVAL.KEY", OutBoundData = true)]
		public List<QEval> QEval { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.TYPE", OutBoundData = true)]
		public List<EType> EType { get; set; }

		public ctxF09EvalSelectResponse()
		{	
			QEval = new List<QEval>();
			EType = new List<EType>();
		}
	}
}
