//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/26/2018 4:26:44 PM by user sbhole1
//
//     Type: ENTITY
//     Entity: CS.ACYR
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
	[DataContract(Name = "CsAcyr")]
	[ColleagueDataContract(GeneratedDateTime = "7/26/2018 4:26:44 PM", User = "sbhole1")]
	[EntityDataContract(EntityName = "CS.ACYR", EntityType = "PHYS")]
	public class CsAcyr : IColleagueGuidEntity
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
		/// CDD Name: CS.STD.TOTAL.EXPENSES
		/// </summary>
		[DataMember(Order = 12, Name = "CS.STD.TOTAL.EXPENSES")]
		public int? CsStdTotalExpenses { get; set; }
		
		/// <summary>
		/// CDD Name: CS.BUDGET.ADJ
		/// </summary>
		[DataMember(Order = 13, Name = "CS.BUDGET.ADJ")]
		public int? CsBudgetAdj { get; set; }
		
		/// <summary>
		/// CDD Name: CS.INST.ADJ
		/// </summary>
		[DataMember(Order = 14, Name = "CS.INST.ADJ")]
		public int? CsInstAdj { get; set; }
		
		/// <summary>
		/// CDD Name: CS.NEED
		/// </summary>
		[DataMember(Order = 15, Name = "CS.NEED")]
		public int? CsNeed { get; set; }
		
		/// <summary>
		/// CDD Name: CS.FC
		/// </summary>
		[DataMember(Order = 17, Name = "CS.FC")]
		public string CsFc { get; set; }
		
		/// <summary>
		/// CDD Name: CS.INST.FC
		/// </summary>
		[DataMember(Order = 23, Name = "CS.INST.FC")]
		public int? CsInstFc { get; set; }
		
		/// <summary>
		/// CDD Name: CS.INST.NEED
		/// </summary>
		[DataMember(Order = 24, Name = "CS.INST.NEED")]
		public int? CsInstNeed { get; set; }
		
		/// <summary>
		/// CDD Name: CS.BUDGET.DURATION
		/// </summary>
		[DataMember(Order = 44, Name = "CS.BUDGET.DURATION")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? CsBudgetDuration { get; set; }
		
		/// <summary>
		/// CDD Name: CS.INST.TOTAL.EXPENSES
		/// </summary>
		[DataMember(Order = 45, Name = "CS.INST.TOTAL.EXPENSES")]
		public int? CsInstTotalExpenses { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.ID
		/// </summary>
		[DataMember(Order = 51, Name = "CS.COMP.ID")]
		public List<string> CsCompId { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.FREEZE.FLAG
		/// </summary>
		[DataMember(Order = 52, Name = "CS.COMP.FREEZE.FLAG")]
		public List<string> CsCompFreezeFlag { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.CB.ORIG.AMT
		/// </summary>
		[DataMember(Order = 53, Name = "CS.COMP.CB.ORIG.AMT")]
		public List<int?> CsCompCbOrigAmt { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.INST.ORIG.AMT
		/// </summary>
		[DataMember(Order = 54, Name = "CS.COMP.INST.ORIG.AMT")]
		public List<int?> CsCompInstOrigAmt { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.PELL.ORIG.AMT
		/// </summary>
		[DataMember(Order = 55, Name = "CS.COMP.PELL.ORIG.AMT")]
		public List<int?> CsCompPellOrigAmt { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.CB.OVR.AMT
		/// </summary>
		[DataMember(Order = 56, Name = "CS.COMP.CB.OVR.AMT")]
		public List<int?> CsCompCbOvrAmt { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.INST.OVR.AMT
		/// </summary>
		[DataMember(Order = 57, Name = "CS.COMP.INST.OVR.AMT")]
		public List<int?> CsCompInstOvrAmt { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.PELL.OVR.AMT
		/// </summary>
		[DataMember(Order = 58, Name = "CS.COMP.PELL.OVR.AMT")]
		public List<int?> CsCompPellOvrAmt { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.STATUS
		/// </summary>
		[DataMember(Order = 59, Name = "CS.COMP.STATUS")]
		public List<string> CsCompStatus { get; set; }
		
		/// <summary>
		/// CDD Name: CS.HOUSING.CODE
		/// </summary>
		[DataMember(Order = 77, Name = "CS.HOUSING.CODE")]
		public string CsHousingCode { get; set; }
		
		/// <summary>
		/// CDD Name: CS.LOCATION
		/// </summary>
		[DataMember(Order = 83, Name = "CS.LOCATION")]
		public string CsLocation { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.INCLUDE.LTHT
		/// </summary>
		[DataMember(Order = 100, Name = "CS.COMP.INCLUDE.LTHT")]
		public List<string> CsCompIncludeLtht { get; set; }
		
		/// <summary>
		/// CDD Name: CS.ISIR.TRANS.IDS
		/// </summary>
		[DataMember(Order = 102, Name = "CS.ISIR.TRANS.IDS")]
		public List<string> CsIsirTransIds { get; set; }
		
		/// <summary>
		/// CDD Name: CS.FED.ISIR.ID
		/// </summary>
		[DataMember(Order = 104, Name = "CS.FED.ISIR.ID")]
		public string CsFedIsirId { get; set; }
		
		/// <summary>
		/// CDD Name: CS.INST.ISIR.ID
		/// </summary>
		[DataMember(Order = 105, Name = "CS.INST.ISIR.ID")]
		public string CsInstIsirId { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.CB.COUNT
		/// </summary>
		[DataMember(Order = 114, Name = "CS.COMP.CB.COUNT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> CsCompCbCount { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.INST.COUNT
		/// </summary>
		[DataMember(Order = 115, Name = "CS.COMP.INST.COUNT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> CsCompInstCount { get; set; }
		
		/// <summary>
		/// CDD Name: CS.COMP.ZERO.AMT.FLAG
		/// </summary>
		[DataMember(Order = 116, Name = "CS.COMP.ZERO.AMT.FLAG")]
		public List<string> CsCompZeroAmtFlag { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CsAcyrCsComp> CsCompEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: CS.COMP
			
			CsCompEntityAssociation = new List<CsAcyrCsComp>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(CsCompId != null)
			{
				int numCsComp = CsCompId.Count;
				if (CsCompFreezeFlag !=null && CsCompFreezeFlag.Count > numCsComp) numCsComp = CsCompFreezeFlag.Count;
				if (CsCompCbOrigAmt !=null && CsCompCbOrigAmt.Count > numCsComp) numCsComp = CsCompCbOrigAmt.Count;
				if (CsCompInstOrigAmt !=null && CsCompInstOrigAmt.Count > numCsComp) numCsComp = CsCompInstOrigAmt.Count;
				if (CsCompPellOrigAmt !=null && CsCompPellOrigAmt.Count > numCsComp) numCsComp = CsCompPellOrigAmt.Count;
				if (CsCompCbOvrAmt !=null && CsCompCbOvrAmt.Count > numCsComp) numCsComp = CsCompCbOvrAmt.Count;
				if (CsCompInstOvrAmt !=null && CsCompInstOvrAmt.Count > numCsComp) numCsComp = CsCompInstOvrAmt.Count;
				if (CsCompPellOvrAmt !=null && CsCompPellOvrAmt.Count > numCsComp) numCsComp = CsCompPellOvrAmt.Count;
				if (CsCompStatus !=null && CsCompStatus.Count > numCsComp) numCsComp = CsCompStatus.Count;
				if (CsCompIncludeLtht !=null && CsCompIncludeLtht.Count > numCsComp) numCsComp = CsCompIncludeLtht.Count;
				if (CsCompCbCount !=null && CsCompCbCount.Count > numCsComp) numCsComp = CsCompCbCount.Count;
				if (CsCompInstCount !=null && CsCompInstCount.Count > numCsComp) numCsComp = CsCompInstCount.Count;
				if (CsCompZeroAmtFlag !=null && CsCompZeroAmtFlag.Count > numCsComp) numCsComp = CsCompZeroAmtFlag.Count;

				for (int i = 0; i < numCsComp; i++)
				{

					string value0 = "";
					if (CsCompId != null && i < CsCompId.Count)
					{
						value0 = CsCompId[i];
					}


					string value1 = "";
					if (CsCompFreezeFlag != null && i < CsCompFreezeFlag.Count)
					{
						value1 = CsCompFreezeFlag[i];
					}


					int? value2 = null;
					if (CsCompCbOrigAmt != null && i < CsCompCbOrigAmt.Count)
					{
						value2 = CsCompCbOrigAmt[i];
					}


					int? value3 = null;
					if (CsCompInstOrigAmt != null && i < CsCompInstOrigAmt.Count)
					{
						value3 = CsCompInstOrigAmt[i];
					}


					int? value4 = null;
					if (CsCompPellOrigAmt != null && i < CsCompPellOrigAmt.Count)
					{
						value4 = CsCompPellOrigAmt[i];
					}


					int? value5 = null;
					if (CsCompCbOvrAmt != null && i < CsCompCbOvrAmt.Count)
					{
						value5 = CsCompCbOvrAmt[i];
					}


					int? value6 = null;
					if (CsCompInstOvrAmt != null && i < CsCompInstOvrAmt.Count)
					{
						value6 = CsCompInstOvrAmt[i];
					}


					int? value7 = null;
					if (CsCompPellOvrAmt != null && i < CsCompPellOvrAmt.Count)
					{
						value7 = CsCompPellOvrAmt[i];
					}


					string value8 = "";
					if (CsCompStatus != null && i < CsCompStatus.Count)
					{
						value8 = CsCompStatus[i];
					}


					string value9 = "";
					if (CsCompIncludeLtht != null && i < CsCompIncludeLtht.Count)
					{
						value9 = CsCompIncludeLtht[i];
					}


					Decimal? value10 = null;
					if (CsCompCbCount != null && i < CsCompCbCount.Count)
					{
						value10 = CsCompCbCount[i];
					}


					Decimal? value11 = null;
					if (CsCompInstCount != null && i < CsCompInstCount.Count)
					{
						value11 = CsCompInstCount[i];
					}


					string value12 = "";
					if (CsCompZeroAmtFlag != null && i < CsCompZeroAmtFlag.Count)
					{
						value12 = CsCompZeroAmtFlag[i];
					}

					CsCompEntityAssociation.Add(new CsAcyrCsComp( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class CsAcyrCsComp
	{
		public string CsCompIdAssocMember;	
		public string CsCompFreezeFlagAssocMember;	
		public int? CsCompCbOrigAmtAssocMember;	
		public int? CsCompInstOrigAmtAssocMember;	
		public int? CsCompPellOrigAmtAssocMember;	
		public int? CsCompCbOvrAmtAssocMember;	
		public int? CsCompInstOvrAmtAssocMember;	
		public int? CsCompPellOvrAmtAssocMember;	
		public string CsCompStatusAssocMember;	
		public string CsCompIncludeLthtAssocMember;	
		public Decimal? CsCompCbCountAssocMember;	
		public Decimal? CsCompInstCountAssocMember;	
		public string CsCompZeroAmtFlagAssocMember;	
		public CsAcyrCsComp() {}
		public CsAcyrCsComp(
			string inCsCompId,
			string inCsCompFreezeFlag,
			int? inCsCompCbOrigAmt,
			int? inCsCompInstOrigAmt,
			int? inCsCompPellOrigAmt,
			int? inCsCompCbOvrAmt,
			int? inCsCompInstOvrAmt,
			int? inCsCompPellOvrAmt,
			string inCsCompStatus,
			string inCsCompIncludeLtht,
			Decimal? inCsCompCbCount,
			Decimal? inCsCompInstCount,
			string inCsCompZeroAmtFlag)
		{
			CsCompIdAssocMember = inCsCompId;
			CsCompFreezeFlagAssocMember = inCsCompFreezeFlag;
			CsCompCbOrigAmtAssocMember = inCsCompCbOrigAmt;
			CsCompInstOrigAmtAssocMember = inCsCompInstOrigAmt;
			CsCompPellOrigAmtAssocMember = inCsCompPellOrigAmt;
			CsCompCbOvrAmtAssocMember = inCsCompCbOvrAmt;
			CsCompInstOvrAmtAssocMember = inCsCompInstOvrAmt;
			CsCompPellOvrAmtAssocMember = inCsCompPellOvrAmt;
			CsCompStatusAssocMember = inCsCompStatus;
			CsCompIncludeLthtAssocMember = inCsCompIncludeLtht;
			CsCompCbCountAssocMember = inCsCompCbCount;
			CsCompInstCountAssocMember = inCsCompInstCount;
			CsCompZeroAmtFlagAssocMember = inCsCompZeroAmtFlag;
		}
	}
}