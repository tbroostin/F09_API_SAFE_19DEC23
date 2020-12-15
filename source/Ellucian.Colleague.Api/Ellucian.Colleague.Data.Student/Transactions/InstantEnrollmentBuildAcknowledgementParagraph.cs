//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/17/2020 11:16:12 AM by user beaton
//
//     Type: CTX
//     Transaction ID: INST.ENROLL.BUILD.ACK.PARA
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
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.BUILD.ACK.PARA", GeneratedDateTime = "3/17/2020 11:16:12 AM", User = "beaton")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstantEnrollmentBuildAcknowledgementParagraphRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.CASH.RCPTS.ID", InBoundData = true)]        
		public string CashRcptsId { get; set; }

		public InstantEnrollmentBuildAcknowledgementParagraphRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "INST.ENROLL.BUILD.ACK.PARA", GeneratedDateTime = "3/17/2020 11:16:12 AM", User = "beaton")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class InstantEnrollmentBuildAcknowledgementParagraphResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PARA.TEXT", OutBoundData = true)]        
		public List<string> ParaText { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", OutBoundData = true)]        
		public string AError { get; set; }

		public InstantEnrollmentBuildAcknowledgementParagraphResponse()
		{	
			ParaText = new List<string>();
		}
	}
}