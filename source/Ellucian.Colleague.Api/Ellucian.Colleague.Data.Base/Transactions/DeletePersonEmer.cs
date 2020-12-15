//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/16/2019 4:01:10 PM by user asainju1
//
//     Type: CTX
//     Transaction ID: DELETE.PERSON.EMER
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.Transactions
{
	[DataContract]
	public class DeletePersonEmerErrors
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
	[ColleagueDataContract(ColleagueId = "DELETE.PERSON.EMER", GeneratedDateTime = "7/16/2019 4:01:10 PM", User = "asainju1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class DeletePersonEmerRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "EMER.NAME.GUID", InBoundData = true)]        
		public string EmerNameGuid { get; set; }

		public DeletePersonEmerRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.PERSON.EMER", GeneratedDateTime = "7/16/2019 4:01:10 PM", User = "asainju1")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class DeletePersonEmerResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<DeletePersonEmerErrors> DeletePersonEmerErrors { get; set; }

		public DeletePersonEmerResponse()
		{	
			DeletePersonEmerErrors = new List<DeletePersonEmerErrors>();
		}
	}
}