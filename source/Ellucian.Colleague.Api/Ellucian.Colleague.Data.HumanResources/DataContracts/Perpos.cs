//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 2/11/2019 12:58:52 PM by user wwollnercs
//
//     Type: ENTITY
//     Entity: PERPOS
//     Application: HR
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.HumanResources.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Perpos")]
	[ColleagueDataContract(GeneratedDateTime = "2/11/2019 12:58:52 PM", User = "wwollnercs")]
	[EntityDataContract(EntityName = "PERPOS", EntityType = "PHYS")]
	public class Perpos : IColleagueGuidEntity
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
		/// CDD Name: PERPOS.HRP.ID
		/// </summary>
		[DataMember(Order = 0, Name = "PERPOS.HRP.ID")]
		public string PerposHrpId { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.POSITION.ID
		/// </summary>
		[DataMember(Order = 1, Name = "PERPOS.POSITION.ID")]
		public string PerposPositionId { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.END.REASON
		/// </summary>
		[DataMember(Order = 2, Name = "PERPOS.END.REASON")]
		public string PerposEndReason { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.FTE
		/// </summary>
		[DataMember(Order = 4, Name = "PERPOS.FTE")]
		[DisplayFormat(DataFormatString = "{0:N3}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PerposFte { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.START.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "PERPOS.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PerposStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.END.DATE
		/// </summary>
		[DataMember(Order = 6, Name = "PERPOS.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PerposEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.SUPERVISOR.HRP.ID
		/// </summary>
		[DataMember(Order = 20, Name = "PERPOS.SUPERVISOR.HRP.ID")]
		public string PerposSupervisorHrpId { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.WORK.WEEK.ID
		/// </summary>
		[DataMember(Order = 35, Name = "PERPOS.WORK.WEEK.ID")]
		public string PerposWorkWeekId { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.ALT.SUPERVISOR.ID
		/// </summary>
		[DataMember(Order = 59, Name = "PERPOS.ALT.SUPERVISOR.ID")]
		public string PerposAltSupervisorId { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.EVAL.RATINGS.DATE
		/// </summary>
		[DataMember(Order = 61, Name = "PERPOS.EVAL.RATINGS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> PerposEvalRatingsDate { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.EVAL.RATINGS
		/// </summary>
		[DataMember(Order = 62, Name = "PERPOS.EVAL.RATINGS")]
		public List<string> PerposEvalRatings { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.EVAL.RATINGS.HRPID
		/// </summary>
		[DataMember(Order = 63, Name = "PERPOS.EVAL.RATINGS.HRPID")]
		public List<string> PerposEvalRatingsHrpid { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.EVAL.RATINGS.NXT.DATE
		/// </summary>
		[DataMember(Order = 66, Name = "PERPOS.EVAL.RATINGS.NXT.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> PerposEvalRatingsNxtDate { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.EVAL.RATINGS.CYCLE
		/// </summary>
		[DataMember(Order = 67, Name = "PERPOS.EVAL.RATINGS.CYCLE")]
		public List<string> PerposEvalRatingsCycle { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.SUNDAY.UNITS
		/// </summary>
		[DataMember(Order = 68, Name = "PERPOS.DFLT.SUNDAY.UNITS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PerposDfltSundayUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.SUNDAY.PRJ
		/// </summary>
		[DataMember(Order = 69, Name = "PERPOS.DFLT.SUNDAY.PRJ")]
		public List<string> PerposDfltSundayPrj { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.MONDAY.UNITS
		/// </summary>
		[DataMember(Order = 70, Name = "PERPOS.DFLT.MONDAY.UNITS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PerposDfltMondayUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.MONDAY.PRJ
		/// </summary>
		[DataMember(Order = 71, Name = "PERPOS.DFLT.MONDAY.PRJ")]
		public List<string> PerposDfltMondayPrj { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.TUESDAY.UNITS
		/// </summary>
		[DataMember(Order = 72, Name = "PERPOS.DFLT.TUESDAY.UNITS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PerposDfltTuesdayUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.TUESDAY.PRJ
		/// </summary>
		[DataMember(Order = 73, Name = "PERPOS.DFLT.TUESDAY.PRJ")]
		public List<string> PerposDfltTuesdayPrj { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.WEDNESDAY.UNITS
		/// </summary>
		[DataMember(Order = 74, Name = "PERPOS.DFLT.WEDNESDAY.UNITS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PerposDfltWednesdayUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.WEDNESDAY.PRJ
		/// </summary>
		[DataMember(Order = 75, Name = "PERPOS.DFLT.WEDNESDAY.PRJ")]
		public List<string> PerposDfltWednesdayPrj { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.THURSDAY.UNITS
		/// </summary>
		[DataMember(Order = 76, Name = "PERPOS.DFLT.THURSDAY.UNITS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PerposDfltThursdayUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.THURSDAY.PRJ
		/// </summary>
		[DataMember(Order = 77, Name = "PERPOS.DFLT.THURSDAY.PRJ")]
		public List<string> PerposDfltThursdayPrj { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.FRIDAY.UNITS
		/// </summary>
		[DataMember(Order = 78, Name = "PERPOS.DFLT.FRIDAY.UNITS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PerposDfltFridayUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.FRIDAY.PRJ
		/// </summary>
		[DataMember(Order = 79, Name = "PERPOS.DFLT.FRIDAY.PRJ")]
		public List<string> PerposDfltFridayPrj { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.SATURDAY.UNITS
		/// </summary>
		[DataMember(Order = 80, Name = "PERPOS.DFLT.SATURDAY.UNITS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PerposDfltSaturdayUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.DFLT.SATURDAY.PRJ
		/// </summary>
		[DataMember(Order = 81, Name = "PERPOS.DFLT.SATURDAY.PRJ")]
		public List<string> PerposDfltSaturdayPrj { get; set; }
		
		/// <summary>
		/// CDD Name: PERPOS.EVAL.RATINGS.COMMENT
		/// </summary>
		[DataMember(Order = 82, Name = "PERPOS.EVAL.RATINGS.COMMENT")]
		public List<string> PerposEvalRatingsComment { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposEvals> PerposEvalsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposDfltSunday> PerposDfltSundayEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposDfltMonday> PerposDfltMondayEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposDfltTuesday> PerposDfltTuesdayEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposDfltWednesday> PerposDfltWednesdayEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposDfltThursday> PerposDfltThursdayEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposDfltFriday> PerposDfltFridayEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposPerposDfltSaturday> PerposDfltSaturdayEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: PERPOS.EVALS
			
			PerposEvalsEntityAssociation = new List<PerposPerposEvals>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposEvalRatingsDate != null)
			{
				int numPerposEvals = PerposEvalRatingsDate.Count;
				if (PerposEvalRatings !=null && PerposEvalRatings.Count > numPerposEvals) numPerposEvals = PerposEvalRatings.Count;
				if (PerposEvalRatingsHrpid !=null && PerposEvalRatingsHrpid.Count > numPerposEvals) numPerposEvals = PerposEvalRatingsHrpid.Count;
				if (PerposEvalRatingsNxtDate !=null && PerposEvalRatingsNxtDate.Count > numPerposEvals) numPerposEvals = PerposEvalRatingsNxtDate.Count;
				if (PerposEvalRatingsCycle !=null && PerposEvalRatingsCycle.Count > numPerposEvals) numPerposEvals = PerposEvalRatingsCycle.Count;
				if (PerposEvalRatingsComment !=null && PerposEvalRatingsComment.Count > numPerposEvals) numPerposEvals = PerposEvalRatingsComment.Count;

				for (int i = 0; i < numPerposEvals; i++)
				{

					DateTime? value0 = null;
					if (PerposEvalRatingsDate != null && i < PerposEvalRatingsDate.Count)
					{
						value0 = PerposEvalRatingsDate[i];
					}


					string value1 = "";
					if (PerposEvalRatings != null && i < PerposEvalRatings.Count)
					{
						value1 = PerposEvalRatings[i];
					}


					string value2 = "";
					if (PerposEvalRatingsHrpid != null && i < PerposEvalRatingsHrpid.Count)
					{
						value2 = PerposEvalRatingsHrpid[i];
					}


					DateTime? value3 = null;
					if (PerposEvalRatingsNxtDate != null && i < PerposEvalRatingsNxtDate.Count)
					{
						value3 = PerposEvalRatingsNxtDate[i];
					}


					string value4 = "";
					if (PerposEvalRatingsCycle != null && i < PerposEvalRatingsCycle.Count)
					{
						value4 = PerposEvalRatingsCycle[i];
					}


					string value5 = "";
					if (PerposEvalRatingsComment != null && i < PerposEvalRatingsComment.Count)
					{
						value5 = PerposEvalRatingsComment[i];
					}

					PerposEvalsEntityAssociation.Add(new PerposPerposEvals( value0, value1, value2, value3, value4, value5));
				}
			}
			// EntityAssociation Name: PERPOS.DFLT.SUNDAY
			
			PerposDfltSundayEntityAssociation = new List<PerposPerposDfltSunday>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposDfltSundayUnits != null)
			{
				int numPerposDfltSunday = PerposDfltSundayUnits.Count;
				if (PerposDfltSundayPrj !=null && PerposDfltSundayPrj.Count > numPerposDfltSunday) numPerposDfltSunday = PerposDfltSundayPrj.Count;

				for (int i = 0; i < numPerposDfltSunday; i++)
				{

					Decimal? value0 = null;
					if (PerposDfltSundayUnits != null && i < PerposDfltSundayUnits.Count)
					{
						value0 = PerposDfltSundayUnits[i];
					}


					string value1 = "";
					if (PerposDfltSundayPrj != null && i < PerposDfltSundayPrj.Count)
					{
						value1 = PerposDfltSundayPrj[i];
					}

					PerposDfltSundayEntityAssociation.Add(new PerposPerposDfltSunday( value0, value1));
				}
			}
			// EntityAssociation Name: PERPOS.DFLT.MONDAY
			
			PerposDfltMondayEntityAssociation = new List<PerposPerposDfltMonday>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposDfltMondayUnits != null)
			{
				int numPerposDfltMonday = PerposDfltMondayUnits.Count;
				if (PerposDfltMondayPrj !=null && PerposDfltMondayPrj.Count > numPerposDfltMonday) numPerposDfltMonday = PerposDfltMondayPrj.Count;

				for (int i = 0; i < numPerposDfltMonday; i++)
				{

					Decimal? value0 = null;
					if (PerposDfltMondayUnits != null && i < PerposDfltMondayUnits.Count)
					{
						value0 = PerposDfltMondayUnits[i];
					}


					string value1 = "";
					if (PerposDfltMondayPrj != null && i < PerposDfltMondayPrj.Count)
					{
						value1 = PerposDfltMondayPrj[i];
					}

					PerposDfltMondayEntityAssociation.Add(new PerposPerposDfltMonday( value0, value1));
				}
			}
			// EntityAssociation Name: PERPOS.DFLT.TUESDAY
			
			PerposDfltTuesdayEntityAssociation = new List<PerposPerposDfltTuesday>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposDfltTuesdayUnits != null)
			{
				int numPerposDfltTuesday = PerposDfltTuesdayUnits.Count;
				if (PerposDfltTuesdayPrj !=null && PerposDfltTuesdayPrj.Count > numPerposDfltTuesday) numPerposDfltTuesday = PerposDfltTuesdayPrj.Count;

				for (int i = 0; i < numPerposDfltTuesday; i++)
				{

					Decimal? value0 = null;
					if (PerposDfltTuesdayUnits != null && i < PerposDfltTuesdayUnits.Count)
					{
						value0 = PerposDfltTuesdayUnits[i];
					}


					string value1 = "";
					if (PerposDfltTuesdayPrj != null && i < PerposDfltTuesdayPrj.Count)
					{
						value1 = PerposDfltTuesdayPrj[i];
					}

					PerposDfltTuesdayEntityAssociation.Add(new PerposPerposDfltTuesday( value0, value1));
				}
			}
			// EntityAssociation Name: PERPOS.DFLT.WEDNESDAY
			
			PerposDfltWednesdayEntityAssociation = new List<PerposPerposDfltWednesday>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposDfltWednesdayUnits != null)
			{
				int numPerposDfltWednesday = PerposDfltWednesdayUnits.Count;
				if (PerposDfltWednesdayPrj !=null && PerposDfltWednesdayPrj.Count > numPerposDfltWednesday) numPerposDfltWednesday = PerposDfltWednesdayPrj.Count;

				for (int i = 0; i < numPerposDfltWednesday; i++)
				{

					Decimal? value0 = null;
					if (PerposDfltWednesdayUnits != null && i < PerposDfltWednesdayUnits.Count)
					{
						value0 = PerposDfltWednesdayUnits[i];
					}


					string value1 = "";
					if (PerposDfltWednesdayPrj != null && i < PerposDfltWednesdayPrj.Count)
					{
						value1 = PerposDfltWednesdayPrj[i];
					}

					PerposDfltWednesdayEntityAssociation.Add(new PerposPerposDfltWednesday( value0, value1));
				}
			}
			// EntityAssociation Name: PERPOS.DFLT.THURSDAY
			
			PerposDfltThursdayEntityAssociation = new List<PerposPerposDfltThursday>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposDfltThursdayUnits != null)
			{
				int numPerposDfltThursday = PerposDfltThursdayUnits.Count;
				if (PerposDfltThursdayPrj !=null && PerposDfltThursdayPrj.Count > numPerposDfltThursday) numPerposDfltThursday = PerposDfltThursdayPrj.Count;

				for (int i = 0; i < numPerposDfltThursday; i++)
				{

					Decimal? value0 = null;
					if (PerposDfltThursdayUnits != null && i < PerposDfltThursdayUnits.Count)
					{
						value0 = PerposDfltThursdayUnits[i];
					}


					string value1 = "";
					if (PerposDfltThursdayPrj != null && i < PerposDfltThursdayPrj.Count)
					{
						value1 = PerposDfltThursdayPrj[i];
					}

					PerposDfltThursdayEntityAssociation.Add(new PerposPerposDfltThursday( value0, value1));
				}
			}
			// EntityAssociation Name: PERPOS.DFLT.FRIDAY
			
			PerposDfltFridayEntityAssociation = new List<PerposPerposDfltFriday>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposDfltFridayUnits != null)
			{
				int numPerposDfltFriday = PerposDfltFridayUnits.Count;
				if (PerposDfltFridayPrj !=null && PerposDfltFridayPrj.Count > numPerposDfltFriday) numPerposDfltFriday = PerposDfltFridayPrj.Count;

				for (int i = 0; i < numPerposDfltFriday; i++)
				{

					Decimal? value0 = null;
					if (PerposDfltFridayUnits != null && i < PerposDfltFridayUnits.Count)
					{
						value0 = PerposDfltFridayUnits[i];
					}


					string value1 = "";
					if (PerposDfltFridayPrj != null && i < PerposDfltFridayPrj.Count)
					{
						value1 = PerposDfltFridayPrj[i];
					}

					PerposDfltFridayEntityAssociation.Add(new PerposPerposDfltFriday( value0, value1));
				}
			}
			// EntityAssociation Name: PERPOS.DFLT.SATURDAY
			
			PerposDfltSaturdayEntityAssociation = new List<PerposPerposDfltSaturday>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PerposDfltSaturdayUnits != null)
			{
				int numPerposDfltSaturday = PerposDfltSaturdayUnits.Count;
				if (PerposDfltSaturdayPrj !=null && PerposDfltSaturdayPrj.Count > numPerposDfltSaturday) numPerposDfltSaturday = PerposDfltSaturdayPrj.Count;

				for (int i = 0; i < numPerposDfltSaturday; i++)
				{

					Decimal? value0 = null;
					if (PerposDfltSaturdayUnits != null && i < PerposDfltSaturdayUnits.Count)
					{
						value0 = PerposDfltSaturdayUnits[i];
					}


					string value1 = "";
					if (PerposDfltSaturdayPrj != null && i < PerposDfltSaturdayPrj.Count)
					{
						value1 = PerposDfltSaturdayPrj[i];
					}

					PerposDfltSaturdayEntityAssociation.Add(new PerposPerposDfltSaturday( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class PerposPerposEvals
	{
		public DateTime? PerposEvalRatingsDateAssocMember;	
		public string PerposEvalRatingsAssocMember;	
		public string PerposEvalRatingsHrpidAssocMember;	
		public DateTime? PerposEvalRatingsNxtDateAssocMember;	
		public string PerposEvalRatingsCycleAssocMember;	
		public string PerposEvalRatingsCommentAssocMember;	
		public PerposPerposEvals() {}
		public PerposPerposEvals(
			DateTime? inPerposEvalRatingsDate,
			string inPerposEvalRatings,
			string inPerposEvalRatingsHrpid,
			DateTime? inPerposEvalRatingsNxtDate,
			string inPerposEvalRatingsCycle,
			string inPerposEvalRatingsComment)
		{
			PerposEvalRatingsDateAssocMember = inPerposEvalRatingsDate;
			PerposEvalRatingsAssocMember = inPerposEvalRatings;
			PerposEvalRatingsHrpidAssocMember = inPerposEvalRatingsHrpid;
			PerposEvalRatingsNxtDateAssocMember = inPerposEvalRatingsNxtDate;
			PerposEvalRatingsCycleAssocMember = inPerposEvalRatingsCycle;
			PerposEvalRatingsCommentAssocMember = inPerposEvalRatingsComment;
		}
	}
	
	[Serializable]
	public class PerposPerposDfltSunday
	{
		public Decimal? PerposDfltSundayUnitsAssocMember;	
		public string PerposDfltSundayPrjAssocMember;	
		public PerposPerposDfltSunday() {}
		public PerposPerposDfltSunday(
			Decimal? inPerposDfltSundayUnits,
			string inPerposDfltSundayPrj)
		{
			PerposDfltSundayUnitsAssocMember = inPerposDfltSundayUnits;
			PerposDfltSundayPrjAssocMember = inPerposDfltSundayPrj;
		}
	}
	
	[Serializable]
	public class PerposPerposDfltMonday
	{
		public Decimal? PerposDfltMondayUnitsAssocMember;	
		public string PerposDfltMondayPrjAssocMember;	
		public PerposPerposDfltMonday() {}
		public PerposPerposDfltMonday(
			Decimal? inPerposDfltMondayUnits,
			string inPerposDfltMondayPrj)
		{
			PerposDfltMondayUnitsAssocMember = inPerposDfltMondayUnits;
			PerposDfltMondayPrjAssocMember = inPerposDfltMondayPrj;
		}
	}
	
	[Serializable]
	public class PerposPerposDfltTuesday
	{
		public Decimal? PerposDfltTuesdayUnitsAssocMember;	
		public string PerposDfltTuesdayPrjAssocMember;	
		public PerposPerposDfltTuesday() {}
		public PerposPerposDfltTuesday(
			Decimal? inPerposDfltTuesdayUnits,
			string inPerposDfltTuesdayPrj)
		{
			PerposDfltTuesdayUnitsAssocMember = inPerposDfltTuesdayUnits;
			PerposDfltTuesdayPrjAssocMember = inPerposDfltTuesdayPrj;
		}
	}
	
	[Serializable]
	public class PerposPerposDfltWednesday
	{
		public Decimal? PerposDfltWednesdayUnitsAssocMember;	
		public string PerposDfltWednesdayPrjAssocMember;	
		public PerposPerposDfltWednesday() {}
		public PerposPerposDfltWednesday(
			Decimal? inPerposDfltWednesdayUnits,
			string inPerposDfltWednesdayPrj)
		{
			PerposDfltWednesdayUnitsAssocMember = inPerposDfltWednesdayUnits;
			PerposDfltWednesdayPrjAssocMember = inPerposDfltWednesdayPrj;
		}
	}
	
	[Serializable]
	public class PerposPerposDfltThursday
	{
		public Decimal? PerposDfltThursdayUnitsAssocMember;	
		public string PerposDfltThursdayPrjAssocMember;	
		public PerposPerposDfltThursday() {}
		public PerposPerposDfltThursday(
			Decimal? inPerposDfltThursdayUnits,
			string inPerposDfltThursdayPrj)
		{
			PerposDfltThursdayUnitsAssocMember = inPerposDfltThursdayUnits;
			PerposDfltThursdayPrjAssocMember = inPerposDfltThursdayPrj;
		}
	}
	
	[Serializable]
	public class PerposPerposDfltFriday
	{
		public Decimal? PerposDfltFridayUnitsAssocMember;	
		public string PerposDfltFridayPrjAssocMember;	
		public PerposPerposDfltFriday() {}
		public PerposPerposDfltFriday(
			Decimal? inPerposDfltFridayUnits,
			string inPerposDfltFridayPrj)
		{
			PerposDfltFridayUnitsAssocMember = inPerposDfltFridayUnits;
			PerposDfltFridayPrjAssocMember = inPerposDfltFridayPrj;
		}
	}
	
	[Serializable]
	public class PerposPerposDfltSaturday
	{
		public Decimal? PerposDfltSaturdayUnitsAssocMember;	
		public string PerposDfltSaturdayPrjAssocMember;	
		public PerposPerposDfltSaturday() {}
		public PerposPerposDfltSaturday(
			Decimal? inPerposDfltSaturdayUnits,
			string inPerposDfltSaturdayPrj)
		{
			PerposDfltSaturdayUnitsAssocMember = inPerposDfltSaturdayUnits;
			PerposDfltSaturdayPrjAssocMember = inPerposDfltSaturdayPrj;
		}
	}
}