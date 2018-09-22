//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/5/2017 2:40:45 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: GET.GRADUATION.APPLICATION.FEE
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.GRADUATION.APPLICATION.FEE", GeneratedDateTime = "10/5/2017 2:40:45 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetGraduationApplicationFeeRequest
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
		[SctrqDataMember(AppServerName = "A.PROGRAM.CODE", InBoundData = true)]        
		public string ProgramCode { get; set; }

		public GetGraduationApplicationFeeRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "GET.GRADUATION.APPLICATION.FEE", GeneratedDateTime = "10/5/2017 2:40:45 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class GetGraduationApplicationFeeResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "A.APPLICATION.FEE", OutBoundData = true)]        
		public Nullable<Decimal> ApplicationFee { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DISTRIBUTION.CODE", OutBoundData = true)]        
		public string DistributionCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.OCCURRED", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorOccurred { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.MESSAGE", OutBoundData = true)]        
		public string ErrorMessage { get; set; }

		public GetGraduationApplicationFeeResponse()
		{	
		}
	}
}
