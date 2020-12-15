//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/9/2019 1:35:05 PM by user coles
//
//     Type: CTX
//     Transaction ID: RECOVER.PERSON.USER.ID
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
	[ColleagueDataContract(ColleagueId = "RECOVER.PERSON.USER.ID", GeneratedDateTime = "8/9/2019 1:35:05 PM", User = "coles")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1, PublicTransaction = true)]
	public class RecoverPersonUserIdRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MATCH.CRITERIA.ID", InBoundData = true)]        
		public string MatchCriteriaId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LAST.NAME", InBoundData = true)]        
		public string LastName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FIRST.NAME", InBoundData = true)]        
		public string FirstName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EMAIL.ADDRESS", InBoundData = true)]        
		public string EmailAddress { get; set; }

		public RecoverPersonUserIdRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "RECOVER.PERSON.USER.ID", GeneratedDateTime = "8/9/2019 1:35:05 PM", User = "coles")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1, PublicTransaction = true)]
	public class RecoverPersonUserIdResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.OCCURRED", OutBoundData = true)]        
		public string ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> ErrorMessages { get; set; }

		public RecoverPersonUserIdResponse()
		{	
			ErrorMessages = new List<string>();
		}
	}
}