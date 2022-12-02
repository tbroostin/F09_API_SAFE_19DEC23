//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/8/2022 10:07:29 AM by user riteshagarwal2
//
//     Type: CTX
//     Transaction ID: DELETE.STUDENT.RELEASE.RECORD
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
	[ColleagueDataContract(ColleagueId = "DELETE.STUDENT.RELEASE.RECORD", GeneratedDateTime = "7/8/2022 10:07:29 AM", User = "riteshagarwal2")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class DeleteStudentReleaseRecordRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.STU.RELEASE.ID", InBoundData = true)]        
		public string InStudentReleasesId { get; set; }

		public DeleteStudentReleaseRecordRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.STUDENT.RELEASE.RECORD", GeneratedDateTime = "7/8/2022 10:07:29 AM", User = "riteshagarwal2")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class DeleteStudentReleaseRecordResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ERROR", OutBoundData = true)]        
		public string OutError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OUT.ERROR.MSGS", OutBoundData = true)]        
		public List<string> OutErrorMessages { get; set; }

		public DeleteStudentReleaseRecordResponse()
		{	
			OutErrorMessages = new List<string>();
		}
	}
}
