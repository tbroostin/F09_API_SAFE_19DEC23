//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/7/2019 9:10:39 AM by user coles
//
//     Type: CTX
//     Transaction ID: RESET.PASSWORD.WITH.TOKEN
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
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "RESET.PASSWORD.WITH.TOKEN", GeneratedDateTime = "10/7/2019 9:10:39 AM", User = "coles")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1, PublicTransaction = true)]
	public class ResetPasswordWithTokenRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "USER.ID", InBoundData = true)]        
		public string UserId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "TOKEN", InBoundData = true)]        
		public string Token { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "NEW.PASSWORD", InBoundData = true)]        
		public string NewPassword { get; set; }

		public ResetPasswordWithTokenRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "RESET.PASSWORD.WITH.TOKEN", GeneratedDateTime = "10/7/2019 9:10:39 AM", User = "coles")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1, PublicTransaction = true)]
	public class ResetPasswordWithTokenResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "USER.ID", OutBoundData = true)]        
		public string UserId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODE", OutBoundData = true)]        
		public string ErrorCode { get; set; }

		public ResetPasswordWithTokenResponse()
		{	
			ErrorMessages = new List<string>();
		}
	}
}
