//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/20/2018 9:32:05 AM by user asainju1
//
//     Type: CTX
//     Transaction ID: UPDATE.RESTRICTION
//     Application: ST
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
	public class RestrictionErrorMessages
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MSG", OutBoundData = true)]
		public string ErrorMsg { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODE", OutBoundData = true)]
		public string ErrorCode { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.RESTRICTION", GeneratedDateTime = "3/20/2018 9:32:05 AM", User = "asainju1")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateRestrictionRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STUDENT.RESTRICTIONS.ID", InBoundData = true)]        
		public string StudentRestrictionsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STR.STUDENT", InBoundData = true)]        
		public string StrStudent { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STR.RESTRICTION", InBoundData = true)]        
		public string StrRestriction { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "STR.START.DATE", InBoundData = true)]        
		public Nullable<DateTime> StrStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "STR.END.DATE", InBoundData = true)]        
		public Nullable<DateTime> StrEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STR.PRTL.DISPLAY.FLAG", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.YesNo, InBoundData = true)]        
		public Nullable<bool> StrNotify { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STR.COMMENTS", InBoundData = true)]        
		public string StrComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STR.GUID", InBoundData = true)]        
		public string StrGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		public UpdateRestrictionRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.RESTRICTION", GeneratedDateTime = "3/20/2018 9:32:05 AM", User = "asainju1")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateRestrictionResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STUDENT.RESTRICTIONS.ID", OutBoundData = true)]        
		public string StudentRestrictionsId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STR.GUID", OutBoundData = true)]        
		public string StrGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODE", OutBoundData = true)]
		public List<RestrictionErrorMessages> RestrictionErrorMessages { get; set; }

		public UpdateRestrictionResponse()
		{	
			RestrictionErrorMessages = new List<RestrictionErrorMessages>();
		}
	}
}
