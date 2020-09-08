//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 2/8/2020 8:05:43 AM by user vaidyasubrahmanyadv
//
//     Type: CTX
//     Transaction ID: TX.DELETE.REQUISITION
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.DELETE.REQUISITION", GeneratedDateTime = "2/8/2020 8:05:43 AM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxDeleteRequisitionRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.USER.ID", InBoundData = true)]        
		public string AUserId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQUISITION.ID", InBoundData = true)]        
		public string ARequisitionId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CONFIRMATION.EMAIL.ADDRESS", InBoundData = true)]        
		public string AConfirmationEmailAddress { get; set; }

		public TxDeleteRequisitionRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "TX.DELETE.REQUISITION", GeneratedDateTime = "2/8/2020 8:05:43 AM", User = "vaidyasubrahmanyadv")]
	[SctrqDataContract(Application = "CF", DataContractVersion = 1)]
	public class TxDeleteRequisitionResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQUISITION.ID", OutBoundData = true)]        
		public string ARequisitionId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.REQUISITION.NUMBER", OutBoundData = true)]        
		public string ARequisitionNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool AErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> AlErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.WARNING.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool AWarningOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.WARNING.MESSAGES", OutBoundData = true)]        
		public List<string> AlWarningMessages { get; set; }

		public TxDeleteRequisitionResponse()
		{	
			AlErrorMessages = new List<string>();
			AlWarningMessages = new List<string>();
		}
	}
}
