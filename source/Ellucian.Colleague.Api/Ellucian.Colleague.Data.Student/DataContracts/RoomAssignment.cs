//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/6/2017 2:40:32 PM by user sbhole1
//
//     Type: ENTITY
//     Entity: ROOM.ASSIGNMENT
//     Application: ST
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "RoomAssignment")]
	[ColleagueDataContract(GeneratedDateTime = "10/6/2017 2:40:32 PM", User = "sbhole1")]
	[EntityDataContract(EntityName = "ROOM.ASSIGNMENT", EntityType = "PHYS")]
	public class RoomAssignment : IColleagueGuidEntity
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		/// <summary>
		/// Record Key
		/// </summary>
		[DataMember]
		public string Recordkey { get; set; }
		
		public void setKey(string key)
		{
			Recordkey = key;
		}
	
		/// <summary>
		/// Record GUID
		/// </summary>
		[DataMember(Name = "RecordGuid")]
		public string RecordGuid { get; set; }

		/// <summary>
		/// Record Model Name
		/// </summary>
		[DataMember(Name = "RecordModelName")]
		public string RecordModelName { get; set; }	
		
		/// <summary>
		/// CDD Name: RMAS.BLDG
		/// </summary>
		[DataMember(Order = 0, Name = "RMAS.BLDG")]
		public string RmasBldg { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.ROOM
		/// </summary>
		[DataMember(Order = 1, Name = "RMAS.ROOM")]
		public string RmasRoom { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.PERSON.ID
		/// </summary>
		[DataMember(Order = 2, Name = "RMAS.PERSON.ID")]
		public string RmasPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.TERM
		/// </summary>
		[DataMember(Order = 3, Name = "RMAS.TERM")]
		public string RmasTerm { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.START.DATE
		/// </summary>
		[DataMember(Order = 4, Name = "RMAS.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RmasStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.END.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "RMAS.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RmasEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.STATUS
		/// </summary>
		[DataMember(Order = 6, Name = "RMAS.STATUS")]
		public List<string> RmasStatus { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.STATUS.DATE
		/// </summary>
		[DataMember(Order = 7, Name = "RMAS.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> RmasStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.RESIDENT.STAFF.INDIC
		/// </summary>
		[DataMember(Order = 11, Name = "RMAS.RESIDENT.STAFF.INDIC")]
		public string RmasResidentStaffIndic { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.PREFERENCE
		/// </summary>
		[DataMember(Order = 13, Name = "RMAS.PREFERENCE")]
		public string RmasPreference { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.ROOM.RATE.TABLE
		/// </summary>
		[DataMember(Order = 14, Name = "RMAS.ROOM.RATE.TABLE")]
		public string RmasRoomRateTable { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.RATE.PERIOD
		/// </summary>
		[DataMember(Order = 15, Name = "RMAS.RATE.PERIOD")]
		public string RmasRatePeriod { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.CONTRACT
		/// </summary>
		[DataMember(Order = 16, Name = "RMAS.CONTRACT")]
		public string RmasContract { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.OVERRIDE.RATE
		/// </summary>
		[DataMember(Order = 19, Name = "RMAS.OVERRIDE.RATE")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? RmasOverrideRate { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.RATE.OVERRIDE.REASON
		/// </summary>
		[DataMember(Order = 20, Name = "RMAS.RATE.OVERRIDE.REASON")]
		public string RmasRateOverrideReason { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.ADDNL.AMTS
		/// </summary>
		[DataMember(Order = 21, Name = "RMAS.ADDNL.AMTS")]
		public List<string> RmasAddnlAmts { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.COMMENTS
		/// </summary>
		[DataMember(Order = 27, Name = "RMAS.COMMENTS")]
		public string RmasComments { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.LOTTERY.NO
		/// </summary>
		[DataMember(Order = 28, Name = "RMAS.LOTTERY.NO")]
		public long? RmasLotteryNo { get; set; }
		
		/// <summary>
		/// CDD Name: RMAS.INTG.KEY.IDX
		/// </summary>
		[DataMember(Order = 44, Name = "RMAS.INTG.KEY.IDX")]
		public string RmasIntgKeyIdx { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<RoomAssignmentRmasStatuses> RmasStatusesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: RMAS.STATUSES
			
			RmasStatusesEntityAssociation = new List<RoomAssignmentRmasStatuses>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(RmasStatus != null)
			{
				int numRmasStatuses = RmasStatus.Count;
				if (RmasStatusDate != null && RmasStatusDate.Count > numRmasStatuses) numRmasStatuses = RmasStatusDate.Count;

				for (int i = 0; i < numRmasStatuses; i++)
				{

					string value0 = "";
					if (RmasStatus != null && i < RmasStatus.Count)
					{
						value0 = RmasStatus[i];
					}


					DateTime? value1 = null;
					if (RmasStatusDate != null && i < RmasStatusDate.Count)
					{
						value1 = RmasStatusDate[i];
					}

					RmasStatusesEntityAssociation.Add(new RoomAssignmentRmasStatuses( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class RoomAssignmentRmasStatuses
	{
		public string RmasStatusAssocMember;	
		public DateTime? RmasStatusDateAssocMember;	
		public RoomAssignmentRmasStatuses() {}
		public RoomAssignmentRmasStatuses(
			string inRmasStatus,
			DateTime? inRmasStatusDate)
		{
			RmasStatusAssocMember = inRmasStatus;
			RmasStatusDateAssocMember = inRmasStatusDate;
		}
	}
}