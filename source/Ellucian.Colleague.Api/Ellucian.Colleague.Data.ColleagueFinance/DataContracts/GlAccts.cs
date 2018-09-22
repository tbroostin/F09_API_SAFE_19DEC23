//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/16/2018 3:45:10 PM by user sbhole1
//
//     Type: ENTITY
//     Entity: GL.ACCTS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "GlAccts")]
	[ColleagueDataContract(GeneratedDateTime = "5/16/2018 3:45:10 PM", User = "sbhole1")]
	[EntityDataContract(EntityName = "GL.ACCTS", EntityType = "PHYS")]
	public class GlAccts : IColleagueGuidEntity
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
		/// CDD Name: GL.ACCTS.ADD.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "GL.ACCTS.ADD.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlAcctsAddDate { get; set; }
		
		/// <summary>
		/// CDD Name: GL.INACTIVE
		/// </summary>
		[DataMember(Order = 4, Name = "GL.INACTIVE")]
		public string GlInactive { get; set; }
		
		/// <summary>
		/// CDD Name: AVAIL.FUNDS.CONTROLLER
		/// </summary>
		[DataMember(Order = 6, Name = "AVAIL.FUNDS.CONTROLLER")]
		public List<string> AvailFundsController { get; set; }
		
		/// <summary>
		/// CDD Name: GL.FREEZE.FLAGS
		/// </summary>
		[DataMember(Order = 7, Name = "GL.FREEZE.FLAGS")]
		public List<string> GlFreezeFlags { get; set; }
		
		/// <summary>
		/// CDD Name: GL.BUDGET.POSTED
		/// </summary>
		[DataMember(Order = 8, Name = "GL.BUDGET.POSTED")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlBudgetPosted { get; set; }
		
		/// <summary>
		/// CDD Name: GL.BUDGET.MEMOS
		/// </summary>
		[DataMember(Order = 9, Name = "GL.BUDGET.MEMOS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlBudgetMemos { get; set; }
		
		/// <summary>
		/// CDD Name: GL.ACTUAL.POSTED
		/// </summary>
		[DataMember(Order = 10, Name = "GL.ACTUAL.POSTED")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlActualPosted { get; set; }
		
		/// <summary>
		/// CDD Name: GL.ACTUAL.MEMOS
		/// </summary>
		[DataMember(Order = 11, Name = "GL.ACTUAL.MEMOS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlActualMemos { get; set; }
		
		/// <summary>
		/// CDD Name: GL.ENCUMBRANCE.POSTED
		/// </summary>
		[DataMember(Order = 12, Name = "GL.ENCUMBRANCE.POSTED")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlEncumbrancePosted { get; set; }
		
		/// <summary>
		/// CDD Name: GL.ENCUMBRANCE.MEMOS
		/// </summary>
		[DataMember(Order = 13, Name = "GL.ENCUMBRANCE.MEMOS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlEncumbranceMemos { get; set; }
		
		/// <summary>
		/// CDD Name: GL.REQUISITION.MEMOS
		/// </summary>
		[DataMember(Order = 14, Name = "GL.REQUISITION.MEMOS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> GlRequisitionMemos { get; set; }
		
		/// <summary>
		/// CDD Name: GL.BUDGET.LINKAGE
		/// </summary>
		[DataMember(Order = 15, Name = "GL.BUDGET.LINKAGE")]
		public List<string> GlBudgetLinkage { get; set; }
		
		/// <summary>
		/// CDD Name: FA.BUDGET.POSTED
		/// </summary>
		[DataMember(Order = 16, Name = "FA.BUDGET.POSTED")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> FaBudgetPosted { get; set; }
		
		/// <summary>
		/// CDD Name: FA.BUDGET.MEMO
		/// </summary>
		[DataMember(Order = 17, Name = "FA.BUDGET.MEMO")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> FaBudgetMemo { get; set; }
		
		/// <summary>
		/// CDD Name: FA.ACTUAL.POSTED
		/// </summary>
		[DataMember(Order = 18, Name = "FA.ACTUAL.POSTED")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> FaActualPosted { get; set; }
		
		/// <summary>
		/// CDD Name: FA.ACTUAL.MEMO
		/// </summary>
		[DataMember(Order = 19, Name = "FA.ACTUAL.MEMO")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> FaActualMemo { get; set; }
		
		/// <summary>
		/// CDD Name: FA.ENCUMBRANCE.POSTED
		/// </summary>
		[DataMember(Order = 20, Name = "FA.ENCUMBRANCE.POSTED")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> FaEncumbrancePosted { get; set; }
		
		/// <summary>
		/// CDD Name: FA.ENCUMBRANCE.MEMO
		/// </summary>
		[DataMember(Order = 21, Name = "FA.ENCUMBRANCE.MEMO")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> FaEncumbranceMemo { get; set; }
		
		/// <summary>
		/// CDD Name: FA.REQUISITION.MEMO
		/// </summary>
		[DataMember(Order = 22, Name = "FA.REQUISITION.MEMO")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> FaRequisitionMemo { get; set; }
		
		/// <summary>
		/// CDD Name: GL.POOLED.TYPE
		/// </summary>
		[DataMember(Order = 23, Name = "GL.POOLED.TYPE")]
		public List<string> GlPooledType { get; set; }
		
		/// <summary>
		/// CDD Name: GL.INCLUDE.IN.POOL
		/// </summary>
		[DataMember(Order = 33, Name = "GL.INCLUDE.IN.POOL")]
		public List<string> GlIncludeInPool { get; set; }
		
		/// <summary>
		/// CDD Name: GL.PROJECTS.ID
		/// </summary>
		[DataMember(Order = 41, Name = "GL.PROJECTS.ID")]
		public List<string> GlProjectsId { get; set; }
		
		/// <summary>
		/// CDD Name: GL.PRJLN.ID
		/// </summary>
		[DataMember(Order = 42, Name = "GL.PRJLN.ID")]
		public List<string> GlPrjlnId { get; set; }
		
		/// <summary>
		/// CDD Name: GL.ACCTS.ADDTIME
		/// </summary>
		[DataMember(Order = 43, Name = "GL.ACCTS.ADDTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlAcctsAddtime { get; set; }
		
		/// <summary>
		/// CDD Name: GL.ACCTS.CHGTIME
		/// </summary>
		[DataMember(Order = 44, Name = "GL.ACCTS.CHGTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlAcctsChgtime { get; set; }
		
		/// <summary>
		/// CDD Name: GL.ACCTS.CHGDATE
		/// </summary>
		[DataMember(Order = 45, Name = "GL.ACCTS.CHGDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlAcctsChgdate { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<GlAcctsMemos> MemosEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<GlAcctsGlProjects> GlProjectsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: MEMOS
			
			MemosEntityAssociation = new List<GlAcctsMemos>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(AvailFundsController != null)
			{
				int numMemos = AvailFundsController.Count;
				if (GlFreezeFlags !=null && GlFreezeFlags.Count > numMemos) numMemos = GlFreezeFlags.Count;
				if (GlBudgetPosted !=null && GlBudgetPosted.Count > numMemos) numMemos = GlBudgetPosted.Count;
				if (GlBudgetMemos !=null && GlBudgetMemos.Count > numMemos) numMemos = GlBudgetMemos.Count;
				if (GlActualPosted !=null && GlActualPosted.Count > numMemos) numMemos = GlActualPosted.Count;
				if (GlActualMemos !=null && GlActualMemos.Count > numMemos) numMemos = GlActualMemos.Count;
				if (GlEncumbrancePosted !=null && GlEncumbrancePosted.Count > numMemos) numMemos = GlEncumbrancePosted.Count;
				if (GlEncumbranceMemos !=null && GlEncumbranceMemos.Count > numMemos) numMemos = GlEncumbranceMemos.Count;
				if (GlRequisitionMemos !=null && GlRequisitionMemos.Count > numMemos) numMemos = GlRequisitionMemos.Count;
				if (GlBudgetLinkage !=null && GlBudgetLinkage.Count > numMemos) numMemos = GlBudgetLinkage.Count;
				if (FaBudgetPosted !=null && FaBudgetPosted.Count > numMemos) numMemos = FaBudgetPosted.Count;
				if (FaBudgetMemo !=null && FaBudgetMemo.Count > numMemos) numMemos = FaBudgetMemo.Count;
				if (FaActualPosted !=null && FaActualPosted.Count > numMemos) numMemos = FaActualPosted.Count;
				if (FaActualMemo !=null && FaActualMemo.Count > numMemos) numMemos = FaActualMemo.Count;
				if (FaEncumbrancePosted !=null && FaEncumbrancePosted.Count > numMemos) numMemos = FaEncumbrancePosted.Count;
				if (FaEncumbranceMemo !=null && FaEncumbranceMemo.Count > numMemos) numMemos = FaEncumbranceMemo.Count;
				if (FaRequisitionMemo !=null && FaRequisitionMemo.Count > numMemos) numMemos = FaRequisitionMemo.Count;
				if (GlPooledType !=null && GlPooledType.Count > numMemos) numMemos = GlPooledType.Count;
				if (GlIncludeInPool !=null && GlIncludeInPool.Count > numMemos) numMemos = GlIncludeInPool.Count;

				for (int i = 0; i < numMemos; i++)
				{

					string value0 = "";
					if (AvailFundsController != null && i < AvailFundsController.Count)
					{
						value0 = AvailFundsController[i];
					}


					string value1 = "";
					if (GlFreezeFlags != null && i < GlFreezeFlags.Count)
					{
						value1 = GlFreezeFlags[i];
					}


					Decimal? value2 = null;
					if (GlBudgetPosted != null && i < GlBudgetPosted.Count)
					{
						value2 = GlBudgetPosted[i];
					}


					Decimal? value3 = null;
					if (GlBudgetMemos != null && i < GlBudgetMemos.Count)
					{
						value3 = GlBudgetMemos[i];
					}


					Decimal? value4 = null;
					if (GlActualPosted != null && i < GlActualPosted.Count)
					{
						value4 = GlActualPosted[i];
					}


					Decimal? value5 = null;
					if (GlActualMemos != null && i < GlActualMemos.Count)
					{
						value5 = GlActualMemos[i];
					}


					Decimal? value6 = null;
					if (GlEncumbrancePosted != null && i < GlEncumbrancePosted.Count)
					{
						value6 = GlEncumbrancePosted[i];
					}


					Decimal? value7 = null;
					if (GlEncumbranceMemos != null && i < GlEncumbranceMemos.Count)
					{
						value7 = GlEncumbranceMemos[i];
					}


					Decimal? value8 = null;
					if (GlRequisitionMemos != null && i < GlRequisitionMemos.Count)
					{
						value8 = GlRequisitionMemos[i];
					}


					string value9 = "";
					if (GlBudgetLinkage != null && i < GlBudgetLinkage.Count)
					{
						value9 = GlBudgetLinkage[i];
					}


					Decimal? value10 = null;
					if (FaBudgetPosted != null && i < FaBudgetPosted.Count)
					{
						value10 = FaBudgetPosted[i];
					}


					Decimal? value11 = null;
					if (FaBudgetMemo != null && i < FaBudgetMemo.Count)
					{
						value11 = FaBudgetMemo[i];
					}


					Decimal? value12 = null;
					if (FaActualPosted != null && i < FaActualPosted.Count)
					{
						value12 = FaActualPosted[i];
					}


					Decimal? value13 = null;
					if (FaActualMemo != null && i < FaActualMemo.Count)
					{
						value13 = FaActualMemo[i];
					}


					Decimal? value14 = null;
					if (FaEncumbrancePosted != null && i < FaEncumbrancePosted.Count)
					{
						value14 = FaEncumbrancePosted[i];
					}


					Decimal? value15 = null;
					if (FaEncumbranceMemo != null && i < FaEncumbranceMemo.Count)
					{
						value15 = FaEncumbranceMemo[i];
					}


					Decimal? value16 = null;
					if (FaRequisitionMemo != null && i < FaRequisitionMemo.Count)
					{
						value16 = FaRequisitionMemo[i];
					}


					string value17 = "";
					if (GlPooledType != null && i < GlPooledType.Count)
					{
						value17 = GlPooledType[i];
					}


					string value18 = "";
					if (GlIncludeInPool != null && i < GlIncludeInPool.Count)
					{
						value18 = GlIncludeInPool[i];
					}

					MemosEntityAssociation.Add(new GlAcctsMemos( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15, value16, value17, value18));
				}
			}
			// EntityAssociation Name: GL.PROJECTS
			
			GlProjectsEntityAssociation = new List<GlAcctsGlProjects>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(GlProjectsId != null)
			{
				int numGlProjects = GlProjectsId.Count;
				if (GlPrjlnId !=null && GlPrjlnId.Count > numGlProjects) numGlProjects = GlPrjlnId.Count;

				for (int i = 0; i < numGlProjects; i++)
				{

					string value0 = "";
					if (GlProjectsId != null && i < GlProjectsId.Count)
					{
						value0 = GlProjectsId[i];
					}


					string value1 = "";
					if (GlPrjlnId != null && i < GlPrjlnId.Count)
					{
						value1 = GlPrjlnId[i];
					}

					GlProjectsEntityAssociation.Add(new GlAcctsGlProjects( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class GlAcctsMemos
	{
		public string AvailFundsControllerAssocMember;	
		public string GlFreezeFlagsAssocMember;	
		public Decimal? GlBudgetPostedAssocMember;	
		public Decimal? GlBudgetMemosAssocMember;	
		public Decimal? GlActualPostedAssocMember;	
		public Decimal? GlActualMemosAssocMember;	
		public Decimal? GlEncumbrancePostedAssocMember;	
		public Decimal? GlEncumbranceMemosAssocMember;	
		public Decimal? GlRequisitionMemosAssocMember;	
		public string GlBudgetLinkageAssocMember;	
		public Decimal? FaBudgetPostedAssocMember;	
		public Decimal? FaBudgetMemoAssocMember;	
		public Decimal? FaActualPostedAssocMember;	
		public Decimal? FaActualMemoAssocMember;	
		public Decimal? FaEncumbrancePostedAssocMember;	
		public Decimal? FaEncumbranceMemoAssocMember;	
		public Decimal? FaRequisitionMemoAssocMember;	
		public string GlPooledTypeAssocMember;	
		public string GlIncludeInPoolAssocMember;	
		public GlAcctsMemos() {}
		public GlAcctsMemos(
			string inAvailFundsController,
			string inGlFreezeFlags,
			Decimal? inGlBudgetPosted,
			Decimal? inGlBudgetMemos,
			Decimal? inGlActualPosted,
			Decimal? inGlActualMemos,
			Decimal? inGlEncumbrancePosted,
			Decimal? inGlEncumbranceMemos,
			Decimal? inGlRequisitionMemos,
			string inGlBudgetLinkage,
			Decimal? inFaBudgetPosted,
			Decimal? inFaBudgetMemo,
			Decimal? inFaActualPosted,
			Decimal? inFaActualMemo,
			Decimal? inFaEncumbrancePosted,
			Decimal? inFaEncumbranceMemo,
			Decimal? inFaRequisitionMemo,
			string inGlPooledType,
			string inGlIncludeInPool)
		{
			AvailFundsControllerAssocMember = inAvailFundsController;
			GlFreezeFlagsAssocMember = inGlFreezeFlags;
			GlBudgetPostedAssocMember = inGlBudgetPosted;
			GlBudgetMemosAssocMember = inGlBudgetMemos;
			GlActualPostedAssocMember = inGlActualPosted;
			GlActualMemosAssocMember = inGlActualMemos;
			GlEncumbrancePostedAssocMember = inGlEncumbrancePosted;
			GlEncumbranceMemosAssocMember = inGlEncumbranceMemos;
			GlRequisitionMemosAssocMember = inGlRequisitionMemos;
			GlBudgetLinkageAssocMember = inGlBudgetLinkage;
			FaBudgetPostedAssocMember = inFaBudgetPosted;
			FaBudgetMemoAssocMember = inFaBudgetMemo;
			FaActualPostedAssocMember = inFaActualPosted;
			FaActualMemoAssocMember = inFaActualMemo;
			FaEncumbrancePostedAssocMember = inFaEncumbrancePosted;
			FaEncumbranceMemoAssocMember = inFaEncumbranceMemo;
			FaRequisitionMemoAssocMember = inFaRequisitionMemo;
			GlPooledTypeAssocMember = inGlPooledType;
			GlIncludeInPoolAssocMember = inGlIncludeInPool;
		}
	}
	
	[Serializable]
	public class GlAcctsGlProjects
	{
		public string GlProjectsIdAssocMember;	
		public string GlPrjlnIdAssocMember;	
		public GlAcctsGlProjects() {}
		public GlAcctsGlProjects(
			string inGlProjectsId,
			string inGlPrjlnId)
		{
			GlProjectsIdAssocMember = inGlProjectsId;
			GlPrjlnIdAssocMember = inGlPrjlnId;
		}
	}
}