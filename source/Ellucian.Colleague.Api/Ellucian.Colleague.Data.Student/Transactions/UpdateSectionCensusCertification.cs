//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/29/2021 10:49:03 AM by user pbhaumik2
//
//     Type: CTX
//     Transaction ID: UPDATE.SECTION.CENSUS.CERTIFICATION
//     Application: ST
//     Environment: devcoll
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
	[ColleagueDataContract(ColleagueId = "UPDATE.SECTION.CENSUS.CERTIFICATION", GeneratedDateTime = "1/29/2021 10:49:03 AM", User = "pbhaumik2")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateSectionCensusCertificationRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.COURSE.SECTION.ID", InBoundData = true)]        
		public string ACourseSectionId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.CERT.CENSUS.DATE", InBoundData = true)]        
		public Nullable<DateTime> ACertCensusDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.PERSON.ID", InBoundData = true)]        
		public string APersonId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.POSITION", InBoundData = true)]        
		public string APosition { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.POSITION.LABEL", InBoundData = true)]        
		public string APositionLabel { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.RECORDED.DATE", InBoundData = true)]        
		public Nullable<DateTime> ARecordedDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "A.RECORDED.TIME", InBoundData = true)]        
		public Nullable<DateTime> ARecordedTime { get; set; }

		public UpdateSectionCensusCertificationRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.SECTION.CENSUS.CERTIFICATION", GeneratedDateTime = "1/29/2021 10:49:03 AM", User = "pbhaumik2")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateSectionCensusCertificationResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool AError { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ERROR.MSGS", OutBoundData = true)]        
		public List<string> AlErrorMsgs { get; set; }

		public UpdateSectionCensusCertificationResponse()
		{	
			AlErrorMsgs = new List<string>();
		}
	}
}