//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/18/2022 10:28:54 AM by user ramyajoshi
//
//     Type: CTX
//     Transaction ID: UPDATE.STUDENT.RELEASE.RECORDS
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

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.STUDENT.RELEASE.RECORDS", GeneratedDateTime = "7/18/2022 10:28:54 AM", User = "ramyajoshi")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateStudentReleaseRecordsRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.STU.REC.REL.ID", InBoundData = true)]        
		public string InStuRecRelId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.NEW.REL.PIN ", InBoundData = true)]        
		public string InNewRelPin  { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.NEW.REL.ACCESS.GIVEN", InBoundData = true)]        
		public List<string> InNewRelAccessGiven { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "IN.NEW.REL.END.DATE", InBoundData = true)]        
		public Nullable<DateTime> InNewRelEndDate { get; set; }

		public UpdateStudentReleaseRecordsRequest()
		{	
			InNewRelAccessGiven = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.STUDENT.RELEASE.RECORDS", GeneratedDateTime = "7/18/2022 10:28:54 AM", User = "ramyajoshi")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateStudentReleaseRecordsResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ERROR ", OutBoundData = true)]        
		public string OutError  { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ERROR.MESSAGES", OutBoundData = true)]        
		public List<string> OutErrorMessages { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.STU.REL.ID", OutBoundData = true)]        
		public string OutStuRelId { get; set; }

		public UpdateStudentReleaseRecordsResponse()
		{	
			OutErrorMessages = new List<string>();
		}
	}
}