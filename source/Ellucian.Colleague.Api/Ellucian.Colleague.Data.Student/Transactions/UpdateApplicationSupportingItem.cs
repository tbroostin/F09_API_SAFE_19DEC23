//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/11/2018 8:33:30 AM by user mbscs
//
//     Type: CTX
//     Transaction ID: UPDATE.APPL.SUPPORTING.ITEM
//     Application: CORE
//     Environment: dvcoll_wstst01
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

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[DataContract]
	public class UpdateAdmApplSupportingItemsErrors
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
	[ColleagueDataContract(ColleagueId = "UPDATE.APPL.SUPPORTING.ITEM", GeneratedDateTime = "4/11/2018 8:33:30 AM", User = "mbscs")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class UpdateApplicationSupportingItemRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MAILING.ID", InBoundData = true)]        
		public string PersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GUID", InBoundData = true)]        
		public string Guid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.APPLICATION", InBoundData = true)]        
		public string ApplicationId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.TYPE", InBoundData = true)]        
		public string CorrespondenceType { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "CORR.ASSIGN.DATE", InBoundData = true)]        
		public Nullable<DateTime> CorrespondenceAssignDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.INSTANCE", InBoundData = true)]        
		public string CorrespondenceInstanceName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.STATUS", InBoundData = true)]        
		public string CorrespondenceStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.REQUIRED", InBoundData = true)]        
		public string CorrespondenceRequired { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.COMMENT", InBoundData = true)]        
		public string CorrespondenceComment { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "CORR.RECEIVED.DATE", InBoundData = true)]        
		public Nullable<DateTime> CorrespondenceReceivedDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "CORR.ACTION.DATE", InBoundData = true)]        
		public Nullable<DateTime> CorrespondenceActionDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		public UpdateApplicationSupportingItemRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.APPL.SUPPORTING.ITEM", GeneratedDateTime = "4/11/2018 8:33:30 AM", User = "mbscs")]
	[SctrqDataContract(Application = "CORE", DataContractVersion = 1)]
	public class UpdateApplicationSupportingItemResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MAILING.ID", OutBoundData = true)]        
		public string PersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GUID", OutBoundData = true)]        
		public string Guid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.APPLICATION", OutBoundData = true)]        
		public string ApplicationId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.TYPE", OutBoundData = true)]        
		public string CorrespondenceType { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "CORR.ASSIGN.DATE", OutBoundData = true)]        
		public Nullable<DateTime> CorrespondenceAssignDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CORR.INSTANCE", OutBoundData = true)]        
		public string CorrespondenceInstanceName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool Error { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<UpdateAdmApplSupportingItemsErrors> UpdateAdmApplSupportingItemsErrors { get; set; }

		public UpdateApplicationSupportingItemResponse()
		{	
			UpdateAdmApplSupportingItemsErrors = new List<UpdateAdmApplSupportingItemsErrors>();
		}
	}
}
