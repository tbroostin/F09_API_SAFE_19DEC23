//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/6/2021 12:41:56 PM by user sreedharpuligundla
//
//     Type: CTX
//     Transaction ID: UPDATE.OFFICE.HOURS
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
	[ColleagueDataContract(ColleagueId = "UPDATE.OFFICE.HOURS", GeneratedDateTime = "1/6/2021 12:41:56 PM", User = "sreedharpuligundla")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateOfficeHoursRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IN.FACULTY.ID", InBoundData = true)]        
		public string InFacultyId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "IN.NEW.START.TIME", InBoundData = true)]        
		public Nullable<DateTime> InNewStartTime { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "IN.NEW.END.TIME", InBoundData = true)]        
		public Nullable<DateTime> InNewEndTime { get; set; }

		[DataMember(IsRequired = true)]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "IN.NEW.START.DATE", InBoundData = true)]        
		public Nullable<DateTime> InNewStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "IN.NEW.END.DATE", InBoundData = true)]        
		public Nullable<DateTime> InNewEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.REPEAT", InBoundData = true)]        
		public string InNewRepeat { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.BUILDING", InBoundData = true)]        
		public string InNewBuilding { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.ROOM", InBoundData = true)]        
		public string InNewRoom { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.MONDAY", InBoundData = true)]        
		public string InNewMonday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.TUESDAY", InBoundData = true)]        
		public string InNewTuesday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.WEDNESDAY", InBoundData = true)]        
		public string InNewWednesday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.THURSDAY", InBoundData = true)]        
		public string InNewThursday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.FRIDAY", InBoundData = true)]        
		public string InNewFriday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.SATURDAY", InBoundData = true)]        
		public string InNewSaturday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.NEW.SUNDAY", InBoundData = true)]        
		public string InNewSunday { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "IN.OLD.START.TIME", InBoundData = true)]        
		public Nullable<DateTime> InOldStartTime { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "IN.OLD.END.TIME", InBoundData = true)]        
		public Nullable<DateTime> InOldEndTime { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "IN.OLD.START.DATE", InBoundData = true)]        
		public Nullable<DateTime> InOldStartDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "IN.OLD.END.DATE", InBoundData = true)]        
		public Nullable<DateTime> InOldEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.REPEAT", InBoundData = true)]        
		public string InOldRepeat { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.BUILDING", InBoundData = true)]        
		public string InOldBuilding { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.ROOM", InBoundData = true)]        
		public string InOldRoom { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.MONDAY", InBoundData = true)]        
		public string InOldMonday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.TUESDAY", InBoundData = true)]        
		public string InOldTuesday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.WEDNESDAY", InBoundData = true)]        
		public string InOldWednesday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.THURSDAY", InBoundData = true)]        
		public string InOldThursday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.FRIDAY", InBoundData = true)]        
		public string InOldFriday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.SATURDAY", InBoundData = true)]        
		public string InOldSaturday { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "IN.OLD.SUNDAY", InBoundData = true)]        
		public string InOldSunday { get; set; }

		public UpdateOfficeHoursRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.OFFICE.HOURS", GeneratedDateTime = "1/6/2021 12:41:56 PM", User = "sreedharpuligundla")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateOfficeHoursResponse
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
		public List<string> OutErrorMsgs { get; set; }

		public UpdateOfficeHoursResponse()
		{	
			OutErrorMsgs = new List<string>();
		}
	}
}
