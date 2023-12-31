//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/13/2019 12:56:34 PM by user otorres
//
//     Type: ENTITY
//     Entity: PERPOSWG
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
	[DataContract(Name = "Perposwg")]
	[ColleagueDataContract(GeneratedDateTime = "11/13/2019 12:56:34 PM", User = "otorres")]
	[EntityDataContract(EntityName = "PERPOSWG", EntityType = "PHYS")]
	public class Perposwg : IColleagueEntity
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
		/// CDD Name: PPWG.HRP.ID
		/// </summary>
		[DataMember(Order = 0, Name = "PPWG.HRP.ID")]
		public string PpwgHrpId { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.POSITION.ID
		/// </summary>
		[DataMember(Order = 1, Name = "PPWG.POSITION.ID")]
		public string PpwgPositionId { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PERPOS.ID
		/// </summary>
		[DataMember(Order = 2, Name = "PPWG.PERPOS.ID")]
		public string PpwgPerposId { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PAYCLASS.ID
		/// </summary>
		[DataMember(Order = 3, Name = "PPWG.PAYCLASS.ID")]
		public string PpwgPayclassId { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PAYCYCLE.ID
		/// </summary>
		[DataMember(Order = 4, Name = "PPWG.PAYCYCLE.ID")]
		public string PpwgPaycycleId { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.POSPAY.ID
		/// </summary>
		[DataMember(Order = 5, Name = "PPWG.POSPAY.ID")]
		public string PpwgPospayId { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.START.DATE
		/// </summary>
		[DataMember(Order = 6, Name = "PPWG.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PpwgStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.END.DATE
		/// </summary>
		[DataMember(Order = 7, Name = "PPWG.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? PpwgEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.GRADE
		/// </summary>
		[DataMember(Order = 9, Name = "PPWG.GRADE")]
		public string PpwgGrade { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.STEP
		/// </summary>
		[DataMember(Order = 10, Name = "PPWG.STEP")]
		public string PpwgStep { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PAY.RATE
		/// </summary>
		[DataMember(Order = 12, Name = "PPWG.PAY.RATE")]
		public string PpwgPayRate { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.BASE.AMT
		/// </summary>
		[DataMember(Order = 15, Name = "PPWG.BASE.AMT")]
		public string PpwgBaseAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.BASE.ET
		/// </summary>
		[DataMember(Order = 16, Name = "PPWG.BASE.ET")]
		public string PpwgBaseEt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PAYROLL.DESIGNATION
		/// </summary>
		[DataMember(Order = 20, Name = "PPWG.PAYROLL.DESIGNATION")]
		public string PpwgPayrollDesignation { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.FNDSRC.ID
		/// </summary>
		[DataMember(Order = 27, Name = "PPWG.PI.FNDSRC.ID")]
		public List<string> PpwgPiFndsrcId { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.GL.NO
		/// </summary>
		[DataMember(Order = 28, Name = "PPWG.PI.GL.NO")]
		public List<string> PpwgPiGlNo { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.HRLY.OR.SLRY
		/// </summary>
		[DataMember(Order = 29, Name = "PPWG.PI.HRLY.OR.SLRY")]
		public List<string> PpwgPiHrlyOrSlry { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.ET
		/// </summary>
		[DataMember(Order = 30, Name = "PPWG.PI.ET")]
		public List<string> PpwgPiEt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.PAY.RATE
		/// </summary>
		[DataMember(Order = 31, Name = "PPWG.PI.PAY.RATE")]
		public List<string> PpwgPiPayRate { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.REG.HRS
		/// </summary>
		[DataMember(Order = 32, Name = "PPWG.PI.REG.HRS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgPiRegHrs { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.PERIOD.PAY
		/// </summary>
		[DataMember(Order = 33, Name = "PPWG.PI.PERIOD.PAY")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgPiPeriodPay { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.CYCLE.WORK.TIME.AMT
		/// </summary>
		[DataMember(Order = 34, Name = "PPWG.CYCLE.WORK.TIME.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PpwgCycleWorkTimeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.CYCLE.WORK.TIME.UNITS
		/// </summary>
		[DataMember(Order = 35, Name = "PPWG.CYCLE.WORK.TIME.UNITS")]
		public string PpwgCycleWorkTimeUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.YEAR.WORK.TIME.AMT
		/// </summary>
		[DataMember(Order = 36, Name = "PPWG.YEAR.WORK.TIME.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PpwgYearWorkTimeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.YEAR.WORK.TIME.UNITS
		/// </summary>
		[DataMember(Order = 37, Name = "PPWG.YEAR.WORK.TIME.UNITS")]
		public string PpwgYearWorkTimeUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.TYPE
		/// </summary>
		[DataMember(Order = 42, Name = "PPWG.TYPE")]
		public string PpwgType { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.DESC
		/// </summary>
		[DataMember(Order = 43, Name = "PPWG.DESC")]
		public string PpwgDesc { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.NO.PAYMENTS
		/// </summary>
		[DataMember(Order = 45, Name = "PPWG.NO.PAYMENTS")]
		public int? PpwgNoPayments { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.NO.PAYMENTS.TAKEN
		/// </summary>
		[DataMember(Order = 46, Name = "PPWG.NO.PAYMENTS.TAKEN")]
		public int? PpwgNoPaymentsTaken { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.PCT.DIST
		/// </summary>
		[DataMember(Order = 51, Name = "PPWG.PI.PCT.DIST")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgPiPctDist { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.SUSPEND.PAY.FLAG
		/// </summary>
		[DataMember(Order = 52, Name = "PPWG.SUSPEND.PAY.FLAG")]
		public string PpwgSuspendPayFlag { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.EXPENSE.AMTS
		/// </summary>
		[DataMember(Order = 63, Name = "PPWG.PI.EXPENSE.AMTS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgPiExpenseAmts { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.ACCRUAL.AMTS
		/// </summary>
		[DataMember(Order = 64, Name = "PPWG.PI.ACCRUAL.AMTS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgPiAccrualAmts { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PROJECTS.IDS
		/// </summary>
		[DataMember(Order = 66, Name = "PPWG.PROJECTS.IDS")]
		public List<string> PpwgProjectsIds { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.COURSE.SEC.ASGMT
		/// </summary>
		[DataMember(Order = 86, Name = "PPWG.COURSE.SEC.ASGMT")]
		public List<string> PpwgCourseSecAsgmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.TOTAL.HOURS.TAUGHT
		/// </summary>
		[DataMember(Order = 87, Name = "PPWG.TOTAL.HOURS.TAUGHT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgTotalHoursTaught { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PRJ.ITEM.IDS
		/// </summary>
		[DataMember(Order = 104, Name = "PPWG.PRJ.ITEM.IDS")]
		public List<string> PpwgPrjItemIds { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.PI.GL.PCT.DIST
		/// </summary>
		[DataMember(Order = 105, Name = "PPWG.PI.GL.PCT.DIST")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgPiGlPctDist { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.ADVISOR.ASGMT
		/// </summary>
		[DataMember(Order = 108, Name = "PPWG.ADVISOR.ASGMT")]
		public List<string> PpwgAdvisorAsgmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.ADVISOR.TOTAL.HOURS
		/// </summary>
		[DataMember(Order = 109, Name = "PPWG.ADVISOR.TOTAL.HOURS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgAdvisorTotalHours { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.MEMBERSHIP.ASGMT
		/// </summary>
		[DataMember(Order = 110, Name = "PPWG.MEMBERSHIP.ASGMT")]
		public List<string> PpwgMembershipAsgmt { get; set; }
		
		/// <summary>
		/// CDD Name: PPWG.MEMBERSHIP.TOTAL.HOURS
		/// </summary>
		[DataMember(Order = 111, Name = "PPWG.MEMBERSHIP.TOTAL.HOURS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> PpwgMembershipTotalHours { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposwgPpwitems> PpwitemsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposwgPpwgcsasgmt> PpwgcsasgmtEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposwgPpwgadvasgmt> PpwgadvasgmtEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PerposwgPpwgmemasgmt> PpwgmemasgmtEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: PPWITEMS
			
			PpwitemsEntityAssociation = new List<PerposwgPpwitems>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PpwgPiFndsrcId != null)
			{
				int numPpwitems = PpwgPiFndsrcId.Count;
				if (PpwgPiGlNo !=null && PpwgPiGlNo.Count > numPpwitems) numPpwitems = PpwgPiGlNo.Count;
				if (PpwgPiHrlyOrSlry !=null && PpwgPiHrlyOrSlry.Count > numPpwitems) numPpwitems = PpwgPiHrlyOrSlry.Count;
				if (PpwgPiEt !=null && PpwgPiEt.Count > numPpwitems) numPpwitems = PpwgPiEt.Count;
				if (PpwgPiPayRate !=null && PpwgPiPayRate.Count > numPpwitems) numPpwitems = PpwgPiPayRate.Count;
				if (PpwgPiRegHrs !=null && PpwgPiRegHrs.Count > numPpwitems) numPpwitems = PpwgPiRegHrs.Count;
				if (PpwgPiPeriodPay !=null && PpwgPiPeriodPay.Count > numPpwitems) numPpwitems = PpwgPiPeriodPay.Count;
				if (PpwgPiPctDist !=null && PpwgPiPctDist.Count > numPpwitems) numPpwitems = PpwgPiPctDist.Count;
				if (PpwgPiExpenseAmts !=null && PpwgPiExpenseAmts.Count > numPpwitems) numPpwitems = PpwgPiExpenseAmts.Count;
				if (PpwgPiAccrualAmts !=null && PpwgPiAccrualAmts.Count > numPpwitems) numPpwitems = PpwgPiAccrualAmts.Count;
				if (PpwgProjectsIds !=null && PpwgProjectsIds.Count > numPpwitems) numPpwitems = PpwgProjectsIds.Count;
				if (PpwgPrjItemIds !=null && PpwgPrjItemIds.Count > numPpwitems) numPpwitems = PpwgPrjItemIds.Count;
				if (PpwgPiGlPctDist !=null && PpwgPiGlPctDist.Count > numPpwitems) numPpwitems = PpwgPiGlPctDist.Count;

				for (int i = 0; i < numPpwitems; i++)
				{

					string value0 = "";
					if (PpwgPiFndsrcId != null && i < PpwgPiFndsrcId.Count)
					{
						value0 = PpwgPiFndsrcId[i];
					}


					string value1 = "";
					if (PpwgPiGlNo != null && i < PpwgPiGlNo.Count)
					{
						value1 = PpwgPiGlNo[i];
					}


					string value2 = "";
					if (PpwgPiHrlyOrSlry != null && i < PpwgPiHrlyOrSlry.Count)
					{
						value2 = PpwgPiHrlyOrSlry[i];
					}


					string value3 = "";
					if (PpwgPiEt != null && i < PpwgPiEt.Count)
					{
						value3 = PpwgPiEt[i];
					}


					string value4 = "";
					if (PpwgPiPayRate != null && i < PpwgPiPayRate.Count)
					{
						value4 = PpwgPiPayRate[i];
					}


					Decimal? value5 = null;
					if (PpwgPiRegHrs != null && i < PpwgPiRegHrs.Count)
					{
						value5 = PpwgPiRegHrs[i];
					}


					Decimal? value6 = null;
					if (PpwgPiPeriodPay != null && i < PpwgPiPeriodPay.Count)
					{
						value6 = PpwgPiPeriodPay[i];
					}


					Decimal? value7 = null;
					if (PpwgPiPctDist != null && i < PpwgPiPctDist.Count)
					{
						value7 = PpwgPiPctDist[i];
					}


					Decimal? value8 = null;
					if (PpwgPiExpenseAmts != null && i < PpwgPiExpenseAmts.Count)
					{
						value8 = PpwgPiExpenseAmts[i];
					}


					Decimal? value9 = null;
					if (PpwgPiAccrualAmts != null && i < PpwgPiAccrualAmts.Count)
					{
						value9 = PpwgPiAccrualAmts[i];
					}


					string value10 = "";
					if (PpwgProjectsIds != null && i < PpwgProjectsIds.Count)
					{
						value10 = PpwgProjectsIds[i];
					}


					string value11 = "";
					if (PpwgPrjItemIds != null && i < PpwgPrjItemIds.Count)
					{
						value11 = PpwgPrjItemIds[i];
					}


					Decimal? value12 = null;
					if (PpwgPiGlPctDist != null && i < PpwgPiGlPctDist.Count)
					{
						value12 = PpwgPiGlPctDist[i];
					}

					PpwitemsEntityAssociation.Add(new PerposwgPpwitems( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12));
				}
			}
			// EntityAssociation Name: PPWGCSASGMT
			
			PpwgcsasgmtEntityAssociation = new List<PerposwgPpwgcsasgmt>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PpwgCourseSecAsgmt != null)
			{
				int numPpwgcsasgmt = PpwgCourseSecAsgmt.Count;
				if (PpwgTotalHoursTaught !=null && PpwgTotalHoursTaught.Count > numPpwgcsasgmt) numPpwgcsasgmt = PpwgTotalHoursTaught.Count;

				for (int i = 0; i < numPpwgcsasgmt; i++)
				{

					string value0 = "";
					if (PpwgCourseSecAsgmt != null && i < PpwgCourseSecAsgmt.Count)
					{
						value0 = PpwgCourseSecAsgmt[i];
					}


					Decimal? value1 = null;
					if (PpwgTotalHoursTaught != null && i < PpwgTotalHoursTaught.Count)
					{
						value1 = PpwgTotalHoursTaught[i];
					}

					PpwgcsasgmtEntityAssociation.Add(new PerposwgPpwgcsasgmt( value0, value1));
				}
			}
			// EntityAssociation Name: PPWGADVASGMT
			
			PpwgadvasgmtEntityAssociation = new List<PerposwgPpwgadvasgmt>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PpwgAdvisorAsgmt != null)
			{
				int numPpwgadvasgmt = PpwgAdvisorAsgmt.Count;
				if (PpwgAdvisorTotalHours !=null && PpwgAdvisorTotalHours.Count > numPpwgadvasgmt) numPpwgadvasgmt = PpwgAdvisorTotalHours.Count;

				for (int i = 0; i < numPpwgadvasgmt; i++)
				{

					string value0 = "";
					if (PpwgAdvisorAsgmt != null && i < PpwgAdvisorAsgmt.Count)
					{
						value0 = PpwgAdvisorAsgmt[i];
					}


					Decimal? value1 = null;
					if (PpwgAdvisorTotalHours != null && i < PpwgAdvisorTotalHours.Count)
					{
						value1 = PpwgAdvisorTotalHours[i];
					}

					PpwgadvasgmtEntityAssociation.Add(new PerposwgPpwgadvasgmt( value0, value1));
				}
			}
			// EntityAssociation Name: PPWGMEMASGMT
			
			PpwgmemasgmtEntityAssociation = new List<PerposwgPpwgmemasgmt>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PpwgMembershipAsgmt != null)
			{
				int numPpwgmemasgmt = PpwgMembershipAsgmt.Count;
				if (PpwgMembershipTotalHours !=null && PpwgMembershipTotalHours.Count > numPpwgmemasgmt) numPpwgmemasgmt = PpwgMembershipTotalHours.Count;

				for (int i = 0; i < numPpwgmemasgmt; i++)
				{

					string value0 = "";
					if (PpwgMembershipAsgmt != null && i < PpwgMembershipAsgmt.Count)
					{
						value0 = PpwgMembershipAsgmt[i];
					}


					Decimal? value1 = null;
					if (PpwgMembershipTotalHours != null && i < PpwgMembershipTotalHours.Count)
					{
						value1 = PpwgMembershipTotalHours[i];
					}

					PpwgmemasgmtEntityAssociation.Add(new PerposwgPpwgmemasgmt( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class PerposwgPpwitems
	{
		public string PpwgPiFndsrcIdAssocMember;	
		public string PpwgPiGlNoAssocMember;	
		public string PpwgPiHrlyOrSlryAssocMember;	
		public string PpwgPiEtAssocMember;	
		public string PpwgPiPayRateAssocMember;	
		public Decimal? PpwgPiRegHrsAssocMember;	
		public Decimal? PpwgPiPeriodPayAssocMember;	
		public Decimal? PpwgPiPctDistAssocMember;	
		public Decimal? PpwgPiExpenseAmtsAssocMember;	
		public Decimal? PpwgPiAccrualAmtsAssocMember;	
		public string PpwgProjectsIdsAssocMember;	
		public string PpwgPrjItemIdsAssocMember;	
		public Decimal? PpwgPiGlPctDistAssocMember;	
		public PerposwgPpwitems() {}
		public PerposwgPpwitems(
			string inPpwgPiFndsrcId,
			string inPpwgPiGlNo,
			string inPpwgPiHrlyOrSlry,
			string inPpwgPiEt,
			string inPpwgPiPayRate,
			Decimal? inPpwgPiRegHrs,
			Decimal? inPpwgPiPeriodPay,
			Decimal? inPpwgPiPctDist,
			Decimal? inPpwgPiExpenseAmts,
			Decimal? inPpwgPiAccrualAmts,
			string inPpwgProjectsIds,
			string inPpwgPrjItemIds,
			Decimal? inPpwgPiGlPctDist)
		{
			PpwgPiFndsrcIdAssocMember = inPpwgPiFndsrcId;
			PpwgPiGlNoAssocMember = inPpwgPiGlNo;
			PpwgPiHrlyOrSlryAssocMember = inPpwgPiHrlyOrSlry;
			PpwgPiEtAssocMember = inPpwgPiEt;
			PpwgPiPayRateAssocMember = inPpwgPiPayRate;
			PpwgPiRegHrsAssocMember = inPpwgPiRegHrs;
			PpwgPiPeriodPayAssocMember = inPpwgPiPeriodPay;
			PpwgPiPctDistAssocMember = inPpwgPiPctDist;
			PpwgPiExpenseAmtsAssocMember = inPpwgPiExpenseAmts;
			PpwgPiAccrualAmtsAssocMember = inPpwgPiAccrualAmts;
			PpwgProjectsIdsAssocMember = inPpwgProjectsIds;
			PpwgPrjItemIdsAssocMember = inPpwgPrjItemIds;
			PpwgPiGlPctDistAssocMember = inPpwgPiGlPctDist;
		}
	}
	
	[Serializable]
	public class PerposwgPpwgcsasgmt
	{
		public string PpwgCourseSecAsgmtAssocMember;	
		public Decimal? PpwgTotalHoursTaughtAssocMember;	
		public PerposwgPpwgcsasgmt() {}
		public PerposwgPpwgcsasgmt(
			string inPpwgCourseSecAsgmt,
			Decimal? inPpwgTotalHoursTaught)
		{
			PpwgCourseSecAsgmtAssocMember = inPpwgCourseSecAsgmt;
			PpwgTotalHoursTaughtAssocMember = inPpwgTotalHoursTaught;
		}
	}
	
	[Serializable]
	public class PerposwgPpwgadvasgmt
	{
		public string PpwgAdvisorAsgmtAssocMember;	
		public Decimal? PpwgAdvisorTotalHoursAssocMember;	
		public PerposwgPpwgadvasgmt() {}
		public PerposwgPpwgadvasgmt(
			string inPpwgAdvisorAsgmt,
			Decimal? inPpwgAdvisorTotalHours)
		{
			PpwgAdvisorAsgmtAssocMember = inPpwgAdvisorAsgmt;
			PpwgAdvisorTotalHoursAssocMember = inPpwgAdvisorTotalHours;
		}
	}
	
	[Serializable]
	public class PerposwgPpwgmemasgmt
	{
		public string PpwgMembershipAsgmtAssocMember;	
		public Decimal? PpwgMembershipTotalHoursAssocMember;	
		public PerposwgPpwgmemasgmt() {}
		public PerposwgPpwgmemasgmt(
			string inPpwgMembershipAsgmt,
			Decimal? inPpwgMembershipTotalHours)
		{
			PpwgMembershipAsgmtAssocMember = inPpwgMembershipAsgmt;
			PpwgMembershipTotalHoursAssocMember = inPpwgMembershipTotalHours;
		}
	}
}