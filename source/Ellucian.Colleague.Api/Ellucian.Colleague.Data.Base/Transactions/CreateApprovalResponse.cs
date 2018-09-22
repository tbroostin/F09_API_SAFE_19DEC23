//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 12:47:27 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: CREATE.APPROVAL.RESPONSE
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.APPROVAL.RESPONSE", GeneratedDateTime = "10/4/2017 12:47:27 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class CreateApprovalResponseRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "APPROVAL.DOCUMENT.ID", InBoundData = true)]        
		public string ApprovalDocumentId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IS.APPROVED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool IsApproved { get; set; }

		[DataMember(IsRequired = true)]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "APPROVAL.DATE", InBoundData = true)]        
		public Nullable<DateTime> ApprovalDate { get; set; }

		[DataMember(IsRequired = true)]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "APPROVAL.TIME", InBoundData = true)]        
		public Nullable<DateTime> ApprovalTime { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "APPROVAL.USERID", InBoundData = true)]        
		public string ApprovalUserid { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "PERSON.ID", InBoundData = true)]        
		public string PersonId { get; set; }

		public CreateApprovalResponseRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.APPROVAL.RESPONSE", GeneratedDateTime = "10/4/2017 12:47:27 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class CreateApprovalResponseResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "APPROVAL.RESPONSE.ID", OutBoundData = true)]        
		public string ApprovalResponseId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		public CreateApprovalResponseResponse()
		{	
		}
	}
}
