//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:23:04 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: VALIDATE.GL.STRING
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "VALIDATE.GL.STRING", GeneratedDateTime = "10/4/2017 1:23:04 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class ValidateGlStringRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GL.ACCT.ID", InBoundData = true)]        
		public string GlAcctId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PROJECT.ID", InBoundData = true)]        
		public string ProjectId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "GL.DATE", InBoundData = true)]        
		public Nullable<DateTime> GlDate { get; set; }

		public ValidateGlStringRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "VALIDATE.GL.STRING", GeneratedDateTime = "10/4/2017 1:23:04 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class ValidateGlStringResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GL.ACCT.ID", OutBoundData = true)]        
		public string GlAcctId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "DESCRIPTION", OutBoundData = true)]        
		public string Description { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", OutBoundData = true)]        
		public string Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODE", OutBoundData = true)]        
		public List<string> ErrorCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		public ValidateGlStringResponse()
		{	
			ErrorCode = new List<string>();
			ErrorMessages = new List<string>();
		}
	}
}
