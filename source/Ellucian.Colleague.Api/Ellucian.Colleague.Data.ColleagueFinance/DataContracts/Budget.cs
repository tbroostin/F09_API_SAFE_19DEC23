//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/20/2017 10:28:11 AM by user bsf1
//
//     Type: ENTITY
//     Entity: BUDGET
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Budget")]
	[ColleagueDataContract(GeneratedDateTime = "12/20/2017 10:28:11 AM", User = "bsf1")]
	[EntityDataContract(EntityName = "BUDGET", EntityType = "PHYS")]
	public class Budget : IColleagueGuidEntity
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
		/// CDD Name: BU.TITLE
		/// </summary>
		[DataMember(Order = 0, Name = "BU.TITLE")]
		public string BuTitle { get; set; }
		
		/// <summary>
		/// CDD Name: BU.CURRENT.VERSION.NAME
		/// </summary>
		[DataMember(Order = 27, Name = "BU.CURRENT.VERSION.NAME")]
		public string BuCurrentVersionName { get; set; }
		
		/// <summary>
		/// CDD Name: BU.CURRENT.VERSION.DESC
		/// </summary>
		[DataMember(Order = 28, Name = "BU.CURRENT.VERSION.DESC")]
		public string BuCurrentVersionDesc { get; set; }
		
		/// <summary>
		/// CDD Name: BU.VERSION
		/// </summary>
		[DataMember(Order = 30, Name = "BU.VERSION")]
		public List<string> BuVersion { get; set; }
		
		/// <summary>
		/// CDD Name: BU.VERSION.DATE
		/// </summary>
		[DataMember(Order = 31, Name = "BU.VERSION.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> BuVersionDate { get; set; }
		
		/// <summary>
		/// CDD Name: BU.VERSION.TIME
		/// </summary>
		[DataMember(Order = 32, Name = "BU.VERSION.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> BuVersionTime { get; set; }
		
		/// <summary>
		/// CDD Name: BU.VERSION.OPERATOR
		/// </summary>
		[DataMember(Order = 33, Name = "BU.VERSION.OPERATOR")]
		public List<string> BuVersionOperator { get; set; }
		
		/// <summary>
		/// CDD Name: BU.VERSION.DESC
		/// </summary>
		[DataMember(Order = 34, Name = "BU.VERSION.DESC")]
		public List<string> BuVersionDesc { get; set; }
		
		/// <summary>
		/// CDD Name: BU.FISCAL.YEAR
		/// </summary>
		[DataMember(Order = 37, Name = "BU.FISCAL.YEAR")]
		public string BuFiscalYear { get; set; }
		
		/// <summary>
		/// CDD Name: BU.STATUS
		/// </summary>
		[DataMember(Order = 49, Name = "BU.STATUS")]
		public string BuStatus { get; set; }
		
		/// <summary>
		/// CDD Name: BU.LOCATION
		/// </summary>
		[DataMember(Order = 61, Name = "BU.LOCATION")]
		public string BuLocation { get; set; }
		
		/// <summary>
		/// CDD Name: BU.VERSION.STATUS
		/// </summary>
		[DataMember(Order = 67, Name = "BU.VERSION.STATUS")]
		public List<string> BuVersionStatus { get; set; }
		
		/// <summary>
		/// CDD Name: BU.VERSION.PB.ID
		/// </summary>
		[DataMember(Order = 71, Name = "BU.VERSION.PB.ID")]
		public List<string> BuVersionPbId { get; set; }
		
		/// <summary>
		/// CDD Name: BU.BUDGET.CODES.INTG.IDX
		/// </summary>
		[DataMember(Order = 72, Name = "BU.BUDGET.CODES.INTG.IDX")]
		public string BuBudgetCodesIntgIdx { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<BudgetBuversn> BuversnEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: BUVERSN
			
			BuversnEntityAssociation = new List<BudgetBuversn>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(BuVersion != null)
			{
				int numBuversn = BuVersion.Count;
				if (BuVersionDate !=null && BuVersionDate.Count > numBuversn) numBuversn = BuVersionDate.Count;
				if (BuVersionTime !=null && BuVersionTime.Count > numBuversn) numBuversn = BuVersionTime.Count;
				if (BuVersionOperator !=null && BuVersionOperator.Count > numBuversn) numBuversn = BuVersionOperator.Count;
				if (BuVersionDesc !=null && BuVersionDesc.Count > numBuversn) numBuversn = BuVersionDesc.Count;
				if (BuVersionStatus !=null && BuVersionStatus.Count > numBuversn) numBuversn = BuVersionStatus.Count;
				if (BuVersionPbId !=null && BuVersionPbId.Count > numBuversn) numBuversn = BuVersionPbId.Count;

				for (int i = 0; i < numBuversn; i++)
				{

					string value0 = "";
					if (BuVersion != null && i < BuVersion.Count)
					{
						value0 = BuVersion[i];
					}


					DateTime? value1 = null;
					if (BuVersionDate != null && i < BuVersionDate.Count)
					{
						value1 = BuVersionDate[i];
					}


					DateTime? value2 = null;
					if (BuVersionTime != null && i < BuVersionTime.Count)
					{
						value2 = BuVersionTime[i];
					}


					string value3 = "";
					if (BuVersionOperator != null && i < BuVersionOperator.Count)
					{
						value3 = BuVersionOperator[i];
					}


					string value4 = "";
					if (BuVersionDesc != null && i < BuVersionDesc.Count)
					{
						value4 = BuVersionDesc[i];
					}


					string value5 = "";
					if (BuVersionStatus != null && i < BuVersionStatus.Count)
					{
						value5 = BuVersionStatus[i];
					}


					string value6 = "";
					if (BuVersionPbId != null && i < BuVersionPbId.Count)
					{
						value6 = BuVersionPbId[i];
					}

					BuversnEntityAssociation.Add(new BudgetBuversn( value0, value1, value2, value3, value4, value5, value6));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class BudgetBuversn
	{
		public string BuVersionAssocMember;	
		public DateTime? BuVersionDateAssocMember;	
		public DateTime? BuVersionTimeAssocMember;	
		public string BuVersionOperatorAssocMember;	
		public string BuVersionDescAssocMember;	
		public string BuVersionStatusAssocMember;	
		public string BuVersionPbIdAssocMember;	
		public BudgetBuversn() {}
		public BudgetBuversn(
			string inBuVersion,
			DateTime? inBuVersionDate,
			DateTime? inBuVersionTime,
			string inBuVersionOperator,
			string inBuVersionDesc,
			string inBuVersionStatus,
			string inBuVersionPbId)
		{
			BuVersionAssocMember = inBuVersion;
			BuVersionDateAssocMember = inBuVersionDate;
			BuVersionTimeAssocMember = inBuVersionTime;
			BuVersionOperatorAssocMember = inBuVersionOperator;
			BuVersionDescAssocMember = inBuVersionDesc;
			BuVersionStatusAssocMember = inBuVersionStatus;
			BuVersionPbIdAssocMember = inBuVersionPbId;
		}
	}
}