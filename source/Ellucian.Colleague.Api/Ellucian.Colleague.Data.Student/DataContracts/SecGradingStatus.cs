//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/16/2021 3:14:05 PM by user jmansfield
//
//     Type: ENTITY
//     Entity: SEC.GRADING.STATUS
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
	[DataContract(Name = "SecGradingStatus")]
	[ColleagueDataContract(GeneratedDateTime = "8/16/2021 3:14:05 PM", User = "jmansfield")]
	[EntityDataContract(EntityName = "SEC.GRADING.STATUS", EntityType = "PHYS")]
	public class SecGradingStatus : IColleagueEntity
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
		/// CDD Name: SGS.MID.GRADE1.CMPL.OPERS
		/// </summary>
		[DataMember(Order = 0, Name = "SGS.MID.GRADE1.CMPL.OPERS")]
		public List<string> SgsMidGrade1CmplOpers { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE1.CMPL.DATES
		/// </summary>
		[DataMember(Order = 1, Name = "SGS.MID.GRADE1.CMPL.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade1CmplDates { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE1.CMPL.TIMES
		/// </summary>
		[DataMember(Order = 2, Name = "SGS.MID.GRADE1.CMPL.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade1CmplTimes { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE2.CMPL.OPERS
		/// </summary>
		[DataMember(Order = 3, Name = "SGS.MID.GRADE2.CMPL.OPERS")]
		public List<string> SgsMidGrade2CmplOpers { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE2.CMPL.DATES
		/// </summary>
		[DataMember(Order = 4, Name = "SGS.MID.GRADE2.CMPL.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade2CmplDates { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE2.CMPL.TIMES
		/// </summary>
		[DataMember(Order = 5, Name = "SGS.MID.GRADE2.CMPL.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade2CmplTimes { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE3.CMPL.OPERS
		/// </summary>
		[DataMember(Order = 6, Name = "SGS.MID.GRADE3.CMPL.OPERS")]
		public List<string> SgsMidGrade3CmplOpers { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE3.CMPL.DATES
		/// </summary>
		[DataMember(Order = 7, Name = "SGS.MID.GRADE3.CMPL.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade3CmplDates { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE3.CMPL.TIMES
		/// </summary>
		[DataMember(Order = 8, Name = "SGS.MID.GRADE3.CMPL.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade3CmplTimes { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE4.CMPL.OPERS
		/// </summary>
		[DataMember(Order = 9, Name = "SGS.MID.GRADE4.CMPL.OPERS")]
		public List<string> SgsMidGrade4CmplOpers { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE4.CMPL.DATES
		/// </summary>
		[DataMember(Order = 10, Name = "SGS.MID.GRADE4.CMPL.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade4CmplDates { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE4.CMPL.TIMES
		/// </summary>
		[DataMember(Order = 11, Name = "SGS.MID.GRADE4.CMPL.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade4CmplTimes { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE5.CMPL.OPERS
		/// </summary>
		[DataMember(Order = 12, Name = "SGS.MID.GRADE5.CMPL.OPERS")]
		public List<string> SgsMidGrade5CmplOpers { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE5.CMPL.DATES
		/// </summary>
		[DataMember(Order = 13, Name = "SGS.MID.GRADE5.CMPL.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade5CmplDates { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE5.CMPL.TIMES
		/// </summary>
		[DataMember(Order = 14, Name = "SGS.MID.GRADE5.CMPL.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade5CmplTimes { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE6.CMPL.OPERS
		/// </summary>
		[DataMember(Order = 15, Name = "SGS.MID.GRADE6.CMPL.OPERS")]
		public List<string> SgsMidGrade6CmplOpers { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE6.CMPL.DATES
		/// </summary>
		[DataMember(Order = 16, Name = "SGS.MID.GRADE6.CMPL.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade6CmplDates { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.MID.GRADE6.CMPL.TIMES
		/// </summary>
		[DataMember(Order = 17, Name = "SGS.MID.GRADE6.CMPL.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SgsMidGrade6CmplTimes { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.ACT.FIN.GRD.CMPL.DT
		/// </summary>
		[DataMember(Order = 24, Name = "SGS.ACT.FIN.GRD.CMPL.DT")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SgsActFinGrdCmplDt { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.ACT.FIN.GRD.CMPL.TM
		/// </summary>
		[DataMember(Order = 25, Name = "SGS.ACT.FIN.GRD.CMPL.TM")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SgsActFinGrdCmplTm { get; set; }
		
		/// <summary>
		/// CDD Name: SGS.ACT.FIN.GRD.CMPL.PER.ID
		/// </summary>
		[DataMember(Order = 26, Name = "SGS.ACT.FIN.GRD.CMPL.PER.ID")]
		public string SgsActFinGrdCmplPerId { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SecGradingStatusSgsMidGrade1Complete> SgsMidGrade1CompleteEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SecGradingStatusSgsMidGrade2Complete> SgsMidGrade2CompleteEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SecGradingStatusSgsMidGrade3Complete> SgsMidGrade3CompleteEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SecGradingStatusSgsMidGrade4Complete> SgsMidGrade4CompleteEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SecGradingStatusSgsMidGrade5Complete> SgsMidGrade5CompleteEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SecGradingStatusSgsMidGrade6Complete> SgsMidGrade6CompleteEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: SGS.MID.GRADE1.COMPLETE
			
			SgsMidGrade1CompleteEntityAssociation = new List<SecGradingStatusSgsMidGrade1Complete>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SgsMidGrade1CmplOpers != null)
			{
				int numSgsMidGrade1Complete = SgsMidGrade1CmplOpers.Count;
				if (SgsMidGrade1CmplDates !=null && SgsMidGrade1CmplDates.Count > numSgsMidGrade1Complete) numSgsMidGrade1Complete = SgsMidGrade1CmplDates.Count;
				if (SgsMidGrade1CmplTimes !=null && SgsMidGrade1CmplTimes.Count > numSgsMidGrade1Complete) numSgsMidGrade1Complete = SgsMidGrade1CmplTimes.Count;

				for (int i = 0; i < numSgsMidGrade1Complete; i++)
				{

					string value0 = "";
					if (SgsMidGrade1CmplOpers != null && i < SgsMidGrade1CmplOpers.Count)
					{
						value0 = SgsMidGrade1CmplOpers[i];
					}


					DateTime? value1 = null;
					if (SgsMidGrade1CmplDates != null && i < SgsMidGrade1CmplDates.Count)
					{
						value1 = SgsMidGrade1CmplDates[i];
					}


					DateTime? value2 = null;
					if (SgsMidGrade1CmplTimes != null && i < SgsMidGrade1CmplTimes.Count)
					{
						value2 = SgsMidGrade1CmplTimes[i];
					}

					SgsMidGrade1CompleteEntityAssociation.Add(new SecGradingStatusSgsMidGrade1Complete( value0, value1, value2));
				}
			}
			// EntityAssociation Name: SGS.MID.GRADE2.COMPLETE
			
			SgsMidGrade2CompleteEntityAssociation = new List<SecGradingStatusSgsMidGrade2Complete>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SgsMidGrade2CmplOpers != null)
			{
				int numSgsMidGrade2Complete = SgsMidGrade2CmplOpers.Count;
				if (SgsMidGrade2CmplDates !=null && SgsMidGrade2CmplDates.Count > numSgsMidGrade2Complete) numSgsMidGrade2Complete = SgsMidGrade2CmplDates.Count;
				if (SgsMidGrade2CmplTimes !=null && SgsMidGrade2CmplTimes.Count > numSgsMidGrade2Complete) numSgsMidGrade2Complete = SgsMidGrade2CmplTimes.Count;

				for (int i = 0; i < numSgsMidGrade2Complete; i++)
				{

					string value0 = "";
					if (SgsMidGrade2CmplOpers != null && i < SgsMidGrade2CmplOpers.Count)
					{
						value0 = SgsMidGrade2CmplOpers[i];
					}


					DateTime? value1 = null;
					if (SgsMidGrade2CmplDates != null && i < SgsMidGrade2CmplDates.Count)
					{
						value1 = SgsMidGrade2CmplDates[i];
					}


					DateTime? value2 = null;
					if (SgsMidGrade2CmplTimes != null && i < SgsMidGrade2CmplTimes.Count)
					{
						value2 = SgsMidGrade2CmplTimes[i];
					}

					SgsMidGrade2CompleteEntityAssociation.Add(new SecGradingStatusSgsMidGrade2Complete( value0, value1, value2));
				}
			}
			// EntityAssociation Name: SGS.MID.GRADE3.COMPLETE
			
			SgsMidGrade3CompleteEntityAssociation = new List<SecGradingStatusSgsMidGrade3Complete>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SgsMidGrade3CmplOpers != null)
			{
				int numSgsMidGrade3Complete = SgsMidGrade3CmplOpers.Count;
				if (SgsMidGrade3CmplDates !=null && SgsMidGrade3CmplDates.Count > numSgsMidGrade3Complete) numSgsMidGrade3Complete = SgsMidGrade3CmplDates.Count;
				if (SgsMidGrade3CmplTimes !=null && SgsMidGrade3CmplTimes.Count > numSgsMidGrade3Complete) numSgsMidGrade3Complete = SgsMidGrade3CmplTimes.Count;

				for (int i = 0; i < numSgsMidGrade3Complete; i++)
				{

					string value0 = "";
					if (SgsMidGrade3CmplOpers != null && i < SgsMidGrade3CmplOpers.Count)
					{
						value0 = SgsMidGrade3CmplOpers[i];
					}


					DateTime? value1 = null;
					if (SgsMidGrade3CmplDates != null && i < SgsMidGrade3CmplDates.Count)
					{
						value1 = SgsMidGrade3CmplDates[i];
					}


					DateTime? value2 = null;
					if (SgsMidGrade3CmplTimes != null && i < SgsMidGrade3CmplTimes.Count)
					{
						value2 = SgsMidGrade3CmplTimes[i];
					}

					SgsMidGrade3CompleteEntityAssociation.Add(new SecGradingStatusSgsMidGrade3Complete( value0, value1, value2));
				}
			}
			// EntityAssociation Name: SGS.MID.GRADE4.COMPLETE
			
			SgsMidGrade4CompleteEntityAssociation = new List<SecGradingStatusSgsMidGrade4Complete>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SgsMidGrade4CmplOpers != null)
			{
				int numSgsMidGrade4Complete = SgsMidGrade4CmplOpers.Count;
				if (SgsMidGrade4CmplDates !=null && SgsMidGrade4CmplDates.Count > numSgsMidGrade4Complete) numSgsMidGrade4Complete = SgsMidGrade4CmplDates.Count;
				if (SgsMidGrade4CmplTimes !=null && SgsMidGrade4CmplTimes.Count > numSgsMidGrade4Complete) numSgsMidGrade4Complete = SgsMidGrade4CmplTimes.Count;

				for (int i = 0; i < numSgsMidGrade4Complete; i++)
				{

					string value0 = "";
					if (SgsMidGrade4CmplOpers != null && i < SgsMidGrade4CmplOpers.Count)
					{
						value0 = SgsMidGrade4CmplOpers[i];
					}


					DateTime? value1 = null;
					if (SgsMidGrade4CmplDates != null && i < SgsMidGrade4CmplDates.Count)
					{
						value1 = SgsMidGrade4CmplDates[i];
					}


					DateTime? value2 = null;
					if (SgsMidGrade4CmplTimes != null && i < SgsMidGrade4CmplTimes.Count)
					{
						value2 = SgsMidGrade4CmplTimes[i];
					}

					SgsMidGrade4CompleteEntityAssociation.Add(new SecGradingStatusSgsMidGrade4Complete( value0, value1, value2));
				}
			}
			// EntityAssociation Name: SGS.MID.GRADE5.COMPLETE
			
			SgsMidGrade5CompleteEntityAssociation = new List<SecGradingStatusSgsMidGrade5Complete>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SgsMidGrade5CmplOpers != null)
			{
				int numSgsMidGrade5Complete = SgsMidGrade5CmplOpers.Count;
				if (SgsMidGrade5CmplDates !=null && SgsMidGrade5CmplDates.Count > numSgsMidGrade5Complete) numSgsMidGrade5Complete = SgsMidGrade5CmplDates.Count;
				if (SgsMidGrade5CmplTimes !=null && SgsMidGrade5CmplTimes.Count > numSgsMidGrade5Complete) numSgsMidGrade5Complete = SgsMidGrade5CmplTimes.Count;

				for (int i = 0; i < numSgsMidGrade5Complete; i++)
				{

					string value0 = "";
					if (SgsMidGrade5CmplOpers != null && i < SgsMidGrade5CmplOpers.Count)
					{
						value0 = SgsMidGrade5CmplOpers[i];
					}


					DateTime? value1 = null;
					if (SgsMidGrade5CmplDates != null && i < SgsMidGrade5CmplDates.Count)
					{
						value1 = SgsMidGrade5CmplDates[i];
					}


					DateTime? value2 = null;
					if (SgsMidGrade5CmplTimes != null && i < SgsMidGrade5CmplTimes.Count)
					{
						value2 = SgsMidGrade5CmplTimes[i];
					}

					SgsMidGrade5CompleteEntityAssociation.Add(new SecGradingStatusSgsMidGrade5Complete( value0, value1, value2));
				}
			}
			// EntityAssociation Name: SGS.MID.GRADE6.COMPLETE
			
			SgsMidGrade6CompleteEntityAssociation = new List<SecGradingStatusSgsMidGrade6Complete>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SgsMidGrade6CmplOpers != null)
			{
				int numSgsMidGrade6Complete = SgsMidGrade6CmplOpers.Count;
				if (SgsMidGrade6CmplDates !=null && SgsMidGrade6CmplDates.Count > numSgsMidGrade6Complete) numSgsMidGrade6Complete = SgsMidGrade6CmplDates.Count;
				if (SgsMidGrade6CmplTimes !=null && SgsMidGrade6CmplTimes.Count > numSgsMidGrade6Complete) numSgsMidGrade6Complete = SgsMidGrade6CmplTimes.Count;

				for (int i = 0; i < numSgsMidGrade6Complete; i++)
				{

					string value0 = "";
					if (SgsMidGrade6CmplOpers != null && i < SgsMidGrade6CmplOpers.Count)
					{
						value0 = SgsMidGrade6CmplOpers[i];
					}


					DateTime? value1 = null;
					if (SgsMidGrade6CmplDates != null && i < SgsMidGrade6CmplDates.Count)
					{
						value1 = SgsMidGrade6CmplDates[i];
					}


					DateTime? value2 = null;
					if (SgsMidGrade6CmplTimes != null && i < SgsMidGrade6CmplTimes.Count)
					{
						value2 = SgsMidGrade6CmplTimes[i];
					}

					SgsMidGrade6CompleteEntityAssociation.Add(new SecGradingStatusSgsMidGrade6Complete( value0, value1, value2));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class SecGradingStatusSgsMidGrade1Complete
	{
		public string SgsMidGrade1CmplOpersAssocMember;	
		public DateTime? SgsMidGrade1CmplDatesAssocMember;	
		public DateTime? SgsMidGrade1CmplTimesAssocMember;	
		public SecGradingStatusSgsMidGrade1Complete() {}
		public SecGradingStatusSgsMidGrade1Complete(
			string inSgsMidGrade1CmplOpers,
			DateTime? inSgsMidGrade1CmplDates,
			DateTime? inSgsMidGrade1CmplTimes)
		{
			SgsMidGrade1CmplOpersAssocMember = inSgsMidGrade1CmplOpers;
			SgsMidGrade1CmplDatesAssocMember = inSgsMidGrade1CmplDates;
			SgsMidGrade1CmplTimesAssocMember = inSgsMidGrade1CmplTimes;
		}
	}
	
	[Serializable]
	public class SecGradingStatusSgsMidGrade2Complete
	{
		public string SgsMidGrade2CmplOpersAssocMember;	
		public DateTime? SgsMidGrade2CmplDatesAssocMember;	
		public DateTime? SgsMidGrade2CmplTimesAssocMember;	
		public SecGradingStatusSgsMidGrade2Complete() {}
		public SecGradingStatusSgsMidGrade2Complete(
			string inSgsMidGrade2CmplOpers,
			DateTime? inSgsMidGrade2CmplDates,
			DateTime? inSgsMidGrade2CmplTimes)
		{
			SgsMidGrade2CmplOpersAssocMember = inSgsMidGrade2CmplOpers;
			SgsMidGrade2CmplDatesAssocMember = inSgsMidGrade2CmplDates;
			SgsMidGrade2CmplTimesAssocMember = inSgsMidGrade2CmplTimes;
		}
	}
	
	[Serializable]
	public class SecGradingStatusSgsMidGrade3Complete
	{
		public string SgsMidGrade3CmplOpersAssocMember;	
		public DateTime? SgsMidGrade3CmplDatesAssocMember;	
		public DateTime? SgsMidGrade3CmplTimesAssocMember;	
		public SecGradingStatusSgsMidGrade3Complete() {}
		public SecGradingStatusSgsMidGrade3Complete(
			string inSgsMidGrade3CmplOpers,
			DateTime? inSgsMidGrade3CmplDates,
			DateTime? inSgsMidGrade3CmplTimes)
		{
			SgsMidGrade3CmplOpersAssocMember = inSgsMidGrade3CmplOpers;
			SgsMidGrade3CmplDatesAssocMember = inSgsMidGrade3CmplDates;
			SgsMidGrade3CmplTimesAssocMember = inSgsMidGrade3CmplTimes;
		}
	}
	
	[Serializable]
	public class SecGradingStatusSgsMidGrade4Complete
	{
		public string SgsMidGrade4CmplOpersAssocMember;	
		public DateTime? SgsMidGrade4CmplDatesAssocMember;	
		public DateTime? SgsMidGrade4CmplTimesAssocMember;	
		public SecGradingStatusSgsMidGrade4Complete() {}
		public SecGradingStatusSgsMidGrade4Complete(
			string inSgsMidGrade4CmplOpers,
			DateTime? inSgsMidGrade4CmplDates,
			DateTime? inSgsMidGrade4CmplTimes)
		{
			SgsMidGrade4CmplOpersAssocMember = inSgsMidGrade4CmplOpers;
			SgsMidGrade4CmplDatesAssocMember = inSgsMidGrade4CmplDates;
			SgsMidGrade4CmplTimesAssocMember = inSgsMidGrade4CmplTimes;
		}
	}
	
	[Serializable]
	public class SecGradingStatusSgsMidGrade5Complete
	{
		public string SgsMidGrade5CmplOpersAssocMember;	
		public DateTime? SgsMidGrade5CmplDatesAssocMember;	
		public DateTime? SgsMidGrade5CmplTimesAssocMember;	
		public SecGradingStatusSgsMidGrade5Complete() {}
		public SecGradingStatusSgsMidGrade5Complete(
			string inSgsMidGrade5CmplOpers,
			DateTime? inSgsMidGrade5CmplDates,
			DateTime? inSgsMidGrade5CmplTimes)
		{
			SgsMidGrade5CmplOpersAssocMember = inSgsMidGrade5CmplOpers;
			SgsMidGrade5CmplDatesAssocMember = inSgsMidGrade5CmplDates;
			SgsMidGrade5CmplTimesAssocMember = inSgsMidGrade5CmplTimes;
		}
	}
	
	[Serializable]
	public class SecGradingStatusSgsMidGrade6Complete
	{
		public string SgsMidGrade6CmplOpersAssocMember;	
		public DateTime? SgsMidGrade6CmplDatesAssocMember;	
		public DateTime? SgsMidGrade6CmplTimesAssocMember;	
		public SecGradingStatusSgsMidGrade6Complete() {}
		public SecGradingStatusSgsMidGrade6Complete(
			string inSgsMidGrade6CmplOpers,
			DateTime? inSgsMidGrade6CmplDates,
			DateTime? inSgsMidGrade6CmplTimes)
		{
			SgsMidGrade6CmplOpersAssocMember = inSgsMidGrade6CmplOpers;
			SgsMidGrade6CmplDatesAssocMember = inSgsMidGrade6CmplDates;
			SgsMidGrade6CmplTimesAssocMember = inSgsMidGrade6CmplTimes;
		}
	}
}