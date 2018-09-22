//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 12:56:42 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: UPDATE.ORG.ENTITY.ROLE.REL
//     Application: UT
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
	[ColleagueDataContract(ColleagueId = "UPDATE.ORG.ENTITY.ROLE.REL", GeneratedDateTime = "10/4/2017 12:56:42 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "UT", DataContractVersion = 1)]
	public class UpdateOrgEntityRoleRelMultiRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ENTITY.ROLE.REL", InBoundData = true)]        
		public string OrgEntityRoleRelId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ENTITY.ROLE", InBoundData = true)]        
		public string OrgEntityRoleId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RELATED.ORG.ENTITY.ROLE", InBoundData = true)]        
		public string RelatedOrgEntityRoleId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "RELATIONSHIP.CATEGORY", InBoundData = true)]        
		public string RelationshipCategory { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ACTION", InBoundData = true)]        
		public string Action { get; set; }

		public UpdateOrgEntityRoleRelMultiRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.ORG.ENTITY.ROLE.REL", GeneratedDateTime = "10/4/2017 12:56:42 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "UT", DataContractVersion = 1)]
	public class UpdateOrgEntityRoleRelMultiResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORG.ENTITY.ROLE.REL", OutBoundData = true)]        
		public string OrgEntityRoleRelId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.OCCURRED", OutBoundData = true)]        
		public string ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MESSAGE", OutBoundData = true)]        
		public string Message { get; set; }

		public UpdateOrgEntityRoleRelMultiResponse()
		{	
		}
	}
}
