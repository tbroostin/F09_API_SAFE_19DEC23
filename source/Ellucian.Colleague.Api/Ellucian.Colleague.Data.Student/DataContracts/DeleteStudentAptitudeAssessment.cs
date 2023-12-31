//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:09:39 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: DELETE.STU.APTITUDE.ASSESSMENT
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[DataContract]
	public class DeleteStudentAptitudeAssessmentErrors
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
	[ColleagueDataContract(ColleagueId = "DELETE.STU.APTITUDE.ASSESSMENT", GeneratedDateTime = "10/4/2017 3:09:39 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class DeleteStudentAptitudeAssessmentRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STUDENT.NON.COURSES.ID", InBoundData = true)]        
		public string StudentNonCoursesId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STNC.GUID", InBoundData = true)]        
		public string StncGuid { get; set; }

		public DeleteStudentAptitudeAssessmentRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.STU.APTITUDE.ASSESSMENT", GeneratedDateTime = "10/4/2017 3:09:39 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class DeleteStudentAptitudeAssessmentResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "STUDENT.NON.COURSES.ID", OutBoundData = true)]        
		public string StudentNonCoursesId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<DeleteStudentAptitudeAssessmentErrors> DeleteStudentAptitudeAssessmentErrors { get; set; }

		public DeleteStudentAptitudeAssessmentResponse()
		{	
			DeleteStudentAptitudeAssessmentErrors = new List<DeleteStudentAptitudeAssessmentErrors>();
		}
	}
}
