//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/20/2020 4:53:53 PM by user bsf1
//
//     Type: CTX
//     Transaction ID: GET.BULK.REQUEST.STATUS
//     Application: CORE
//     Environment: dvcoll-2019
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class errors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODE", OutBoundData = true)]
		public string code { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.ID", OutBoundData = true)]
		public string id { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]
		public string message { get; set; }
	}

	[DataContract]
	public class processingSteps
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.COUNT.OUT", OutBoundData = true)]
		public string count { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.ELAPSED.TIME.OUT", OutBoundData = true)]
		public string elapsedTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.FILE.OUT", OutBoundData = true)]
		public string file { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.JOB.NUMBER.OUT", OutBoundData = true)]
		public string stepJobNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.SEQ.OUT", OutBoundData = true)]
		public string seq { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.START.TIME.OUT", OutBoundData = true)]
		public string startTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.STATUS.OUT", OutBoundData = true)]
		public string status { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STEP.NAME.OUT", OutBoundData = true)]
		public string step { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.BULK.REQUEST.STATUS", GeneratedDateTime = "4/20/2020 4:53:53 PM", User = "bsf1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetBulkRequestStatusRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "REQUESTOR.TRACKING.ID", InBoundData = true)]        
		public string requestorTrackingId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RESOURCE.NAME", InBoundData = true)]        
		public string resourceName { get; set; }

		public GetBulkRequestStatusRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.BULK.REQUEST.STATUS", GeneratedDateTime = "4/20/2020 4:53:53 PM", User = "bsf1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class GetBulkRequestStatusResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "APPLICATION.ID", OutBoundData = true)]        
		public string applicationId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "JOB.NUMBER", OutBoundData = true)]        
		public string jobNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "REPRESENTATION", OutBoundData = true)]        
		public string representation { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "REQUESTOR.TRACKING.ID", OutBoundData = true)]        
		public string requestorTrackingId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RESOURCE.NAME", OutBoundData = true)]        
		public string resourceName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "JOB.STATUS", OutBoundData = true)]        
		public string jobStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TENANT.ID", OutBoundData = true)]        
		public string tenantId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TOTAL.COUNT", OutBoundData = true)]        
		public string xTotalCount { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.ID", OutBoundData = true)]
		public List<errors> errors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:STEP.SEQ.OUT", OutBoundData = true)]
		public List<processingSteps> processingSteps { get; set; }

		public GetBulkRequestStatusResponse()
		{	
			errors = new List<errors>();
			processingSteps = new List<processingSteps>();
		}
	}
}