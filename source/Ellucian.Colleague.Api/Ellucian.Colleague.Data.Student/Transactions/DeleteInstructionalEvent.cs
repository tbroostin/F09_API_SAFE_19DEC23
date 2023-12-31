//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/5/2017 2:38:21 PM by user dvcoll-schandraseka
//
//     Type: CTX
//     Transaction ID: DELETE.INSTRUCTIONAL.EVENT
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
	[DataContract]
	public class DeleteInstructionalEventErrors
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODES", OutBoundData = true)]
		public string ErrorCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGES", OutBoundData = true)]
		public string ErrorMessages { get; set; }
	}

	[DataContract]
	public class DeleteInstructionalEventWarnings
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.CODES", OutBoundData = true)]
		public string WarningCodes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "WARNING.MESSAGES", OutBoundData = true)]
		public string WarningMessages { get; set; }
	}

	[DataContract]
	public class Faculty
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "FAC.CSF.IDS", InBoundData = true)]
		public string FacCsfIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FAC.FACULTY", InBoundData = true)]
		public string FacFaculty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FAC.INSTR.METHOD", InBoundData = true)]
		public string FacInstrMethod { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "FAC.START.DATE", InBoundData = true)]
		public Nullable<DateTime> FacStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "FAC.END.DATE", InBoundData = true)]
		public Nullable<DateTime> FacEndDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "FAC.FACULTY.LOAD", InBoundData = true)]
		public Nullable<Decimal> FacFacultyLoad { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "FAC.FACULTY.PCT", InBoundData = true)]
		public Nullable<Decimal> FacFacultyPct { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FAC.PAC.LP.ASGMT", InBoundData = true)]
		public string FacPacLpAsgmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "FAC.TEACHING.ARRANGEMENT", InBoundData = true)]
		public string FacTeachingArrangement { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.INSTRUCTIONAL.EVENT", GeneratedDateTime = "10/5/2017 2:38:21 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class DeleteInstructionalEventRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "COURSE.SEC.MEETING.ID", InBoundData = true)]        
		public string CourseSecMeetingId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "CSM.GUID", InBoundData = true)]        
		public string CsmGuid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "DELETE.CSF.IDS", InBoundData = true)]        
		public List<string> DeleteCsfIds { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:FAC.CSF.IDS", InBoundData = true)]
		public List<Faculty> Faculty { get; set; }

		public DeleteInstructionalEventRequest()
		{	
			DeleteCsfIds = new List<string>();
			Faculty = new List<Faculty>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "DELETE.INSTRUCTIONAL.EVENT", GeneratedDateTime = "10/5/2017 2:38:21 PM", User = "dvcoll-schandraseka")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class DeleteInstructionalEventResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:ERROR.CODES", OutBoundData = true)]
		public List<DeleteInstructionalEventErrors> DeleteInstructionalEventErrors { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:WARNING.CODES", OutBoundData = true)]
		public List<DeleteInstructionalEventWarnings> DeleteInstructionalEventWarnings { get; set; }

		public DeleteInstructionalEventResponse()
		{	
			DeleteInstructionalEventErrors = new List<DeleteInstructionalEventErrors>();
			DeleteInstructionalEventWarnings = new List<DeleteInstructionalEventWarnings>();
		}
	}
}
