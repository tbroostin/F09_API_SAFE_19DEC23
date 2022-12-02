//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/27/2022 7:37:30 PM by user dvetk-srm
//
//     Type: CTX
//     Transaction ID: UPDATE.AUDIT.LOG.PARMS
//     Application: UT
//     Environment: dvetk
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
	[ColleagueDataContract(ColleagueId = "UPDATE.AUDIT.LOG.PARMS", GeneratedDateTime = "7/27/2022 7:37:30 PM", User = "dvetk-srm")]
	[SctrqDataContract(Application = "UT", DataContractVersion = 1)]
	public class UpdateAuditLogParmsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AUDIT.LOG.CATEGORY", InBoundData = true)]        
		public string AuditLogCategory { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AUDIT.LOG.ENABLED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, InBoundData = true)]        
		public bool AuditLogEnabled { get; set; }

		public UpdateAuditLogParmsRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.AUDIT.LOG.PARMS", GeneratedDateTime = "7/27/2022 7:37:30 PM", User = "dvetk-srm")]
	[SctrqDataContract(Application = "UT", DataContractVersion = 1)]
	public class UpdateAuditLogParmsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }


		public UpdateAuditLogParmsResponse()
		{	
		}
	}
}
