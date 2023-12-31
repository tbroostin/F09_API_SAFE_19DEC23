//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:20:26 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: JRNL.ENTS
//     Application: CF
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "JrnlEnts")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:20:26 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "JRNL.ENTS", EntityType = "PHYS")]
	public class JrnlEnts : IColleagueEntity
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
		/// CDD Name: JRTS.GL.NO
		/// </summary>
		[DataMember(Order = 0, Name = "JRTS.GL.NO")]
		public List<string> JrtsGlNo { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.DESCRIPTION
		/// </summary>
		[DataMember(Order = 1, Name = "JRTS.DESCRIPTION")]
		public List<string> JrtsDescription { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.DEBIT
		/// </summary>
		[DataMember(Order = 2, Name = "JRTS.DEBIT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> JrtsDebit { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.CREDIT
		/// </summary>
		[DataMember(Order = 3, Name = "JRTS.CREDIT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> JrtsCredit { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.TR.DATE
		/// </summary>
		[DataMember(Order = 4, Name = "JRTS.TR.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? JrtsTrDate { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.SOURCE
		/// </summary>
		[DataMember(Order = 6, Name = "JRTS.SOURCE")]
		public string JrtsSource { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.AUTHOR
		/// </summary>
		[DataMember(Order = 8, Name = "JRTS.AUTHOR")]
		public string JrtsAuthor { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.COMMENTS
		/// </summary>
		[DataMember(Order = 9, Name = "JRTS.COMMENTS")]
		public string JrtsComments { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.PROJECTS.CF.ID
		/// </summary>
		[DataMember(Order = 24, Name = "JRTS.PROJECTS.CF.ID")]
		public List<string> JrtsProjectsCfId { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.PRJ.ITEM.IDS
		/// </summary>
		[DataMember(Order = 25, Name = "JRTS.PRJ.ITEM.IDS")]
		public List<string> JrtsPrjItemIds { get; set; }
		
		/// <summary>
		/// CDD Name: JRNL.ENTS.ADD.DATE
		/// </summary>
		[DataMember(Order = 26, Name = "JRNL.ENTS.ADD.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? JrnlEntsAddDate { get; set; }
		
		/// <summary>
		/// CDD Name: JRNL.ENTS.ADD.OPERATOR
		/// </summary>
		[DataMember(Order = 27, Name = "JRNL.ENTS.ADD.OPERATOR")]
		public string JrnlEntsAddOperator { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.STATUS
		/// </summary>
		[DataMember(Order = 28, Name = "JRTS.STATUS")]
		public string JrtsStatus { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.AUTHORIZATIONS
		/// </summary>
		[DataMember(Order = 31, Name = "JRTS.AUTHORIZATIONS")]
		public List<string> JrtsAuthorizations { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.AUTHORIZATION.DATES
		/// </summary>
		[DataMember(Order = 32, Name = "JRTS.AUTHORIZATION.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> JrtsAuthorizationDates { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.AUTHORIZATION.LEVELS
		/// </summary>
		[DataMember(Order = 33, Name = "JRTS.AUTHORIZATION.LEVELS")]
		public List<string> JrtsAuthorizationLevels { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.NEXT.APPROVAL.IDS
		/// </summary>
		[DataMember(Order = 34, Name = "JRTS.NEXT.APPROVAL.IDS")]
		public List<string> JrtsNextApprovalIds { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.APPROVAL.LEVELS
		/// </summary>
		[DataMember(Order = 35, Name = "JRTS.APPROVAL.LEVELS")]
		public List<string> JrtsApprovalLevels { get; set; }
		
		/// <summary>
		/// CDD Name: JRTS.REVERSAL.FLAG
		/// </summary>
		[DataMember(Order = 37, Name = "JRTS.REVERSAL.FLAG")]
		public string JrtsReversalFlag { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<JrnlEntsJrtsData> JrtsDataEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<JrnlEntsJrtsAuth> JrtsAuthEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<JrnlEntsJrtsAppr> JrtsApprEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: JRTS.DATA
			
			JrtsDataEntityAssociation = new List<JrnlEntsJrtsData>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(JrtsGlNo != null)
			{
				int numJrtsData = JrtsGlNo.Count;
				if (JrtsDescription !=null && JrtsDescription.Count > numJrtsData) numJrtsData = JrtsDescription.Count;
				if (JrtsDebit !=null && JrtsDebit.Count > numJrtsData) numJrtsData = JrtsDebit.Count;
				if (JrtsCredit !=null && JrtsCredit.Count > numJrtsData) numJrtsData = JrtsCredit.Count;
				if (JrtsProjectsCfId !=null && JrtsProjectsCfId.Count > numJrtsData) numJrtsData = JrtsProjectsCfId.Count;
				if (JrtsPrjItemIds !=null && JrtsPrjItemIds.Count > numJrtsData) numJrtsData = JrtsPrjItemIds.Count;

				for (int i = 0; i < numJrtsData; i++)
				{

					string value0 = "";
					if (JrtsGlNo != null && i < JrtsGlNo.Count)
					{
						value0 = JrtsGlNo[i];
					}


					string value1 = "";
					if (JrtsDescription != null && i < JrtsDescription.Count)
					{
						value1 = JrtsDescription[i];
					}


					Decimal? value2 = null;
					if (JrtsDebit != null && i < JrtsDebit.Count)
					{
						value2 = JrtsDebit[i];
					}


					Decimal? value3 = null;
					if (JrtsCredit != null && i < JrtsCredit.Count)
					{
						value3 = JrtsCredit[i];
					}


					string value4 = "";
					if (JrtsProjectsCfId != null && i < JrtsProjectsCfId.Count)
					{
						value4 = JrtsProjectsCfId[i];
					}


					string value5 = "";
					if (JrtsPrjItemIds != null && i < JrtsPrjItemIds.Count)
					{
						value5 = JrtsPrjItemIds[i];
					}

					JrtsDataEntityAssociation.Add(new JrnlEntsJrtsData( value0, value1, value2, value3, value4, value5));
				}
			}
			// EntityAssociation Name: JRTS.AUTH
			
			JrtsAuthEntityAssociation = new List<JrnlEntsJrtsAuth>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(JrtsAuthorizations != null)
			{
				int numJrtsAuth = JrtsAuthorizations.Count;
				if (JrtsAuthorizationDates !=null && JrtsAuthorizationDates.Count > numJrtsAuth) numJrtsAuth = JrtsAuthorizationDates.Count;
				if (JrtsAuthorizationLevels !=null && JrtsAuthorizationLevels.Count > numJrtsAuth) numJrtsAuth = JrtsAuthorizationLevels.Count;

				for (int i = 0; i < numJrtsAuth; i++)
				{

					string value0 = "";
					if (JrtsAuthorizations != null && i < JrtsAuthorizations.Count)
					{
						value0 = JrtsAuthorizations[i];
					}


					DateTime? value1 = null;
					if (JrtsAuthorizationDates != null && i < JrtsAuthorizationDates.Count)
					{
						value1 = JrtsAuthorizationDates[i];
					}


					string value2 = "";
					if (JrtsAuthorizationLevels != null && i < JrtsAuthorizationLevels.Count)
					{
						value2 = JrtsAuthorizationLevels[i];
					}

					JrtsAuthEntityAssociation.Add(new JrnlEntsJrtsAuth( value0, value1, value2));
				}
			}
			// EntityAssociation Name: JRTS.APPR
			
			JrtsApprEntityAssociation = new List<JrnlEntsJrtsAppr>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(JrtsNextApprovalIds != null)
			{
				int numJrtsAppr = JrtsNextApprovalIds.Count;
				if (JrtsApprovalLevels !=null && JrtsApprovalLevels.Count > numJrtsAppr) numJrtsAppr = JrtsApprovalLevels.Count;

				for (int i = 0; i < numJrtsAppr; i++)
				{

					string value0 = "";
					if (JrtsNextApprovalIds != null && i < JrtsNextApprovalIds.Count)
					{
						value0 = JrtsNextApprovalIds[i];
					}


					string value1 = "";
					if (JrtsApprovalLevels != null && i < JrtsApprovalLevels.Count)
					{
						value1 = JrtsApprovalLevels[i];
					}

					JrtsApprEntityAssociation.Add(new JrnlEntsJrtsAppr( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class JrnlEntsJrtsData
	{
		public string JrtsGlNoAssocMember;	
		public string JrtsDescriptionAssocMember;	
		public Decimal? JrtsDebitAssocMember;	
		public Decimal? JrtsCreditAssocMember;	
		public string JrtsProjectsCfIdAssocMember;	
		public string JrtsPrjItemIdsAssocMember;	
		public JrnlEntsJrtsData() {}
		public JrnlEntsJrtsData(
			string inJrtsGlNo,
			string inJrtsDescription,
			Decimal? inJrtsDebit,
			Decimal? inJrtsCredit,
			string inJrtsProjectsCfId,
			string inJrtsPrjItemIds)
		{
			JrtsGlNoAssocMember = inJrtsGlNo;
			JrtsDescriptionAssocMember = inJrtsDescription;
			JrtsDebitAssocMember = inJrtsDebit;
			JrtsCreditAssocMember = inJrtsCredit;
			JrtsProjectsCfIdAssocMember = inJrtsProjectsCfId;
			JrtsPrjItemIdsAssocMember = inJrtsPrjItemIds;
		}
	}
	
	[Serializable]
	public class JrnlEntsJrtsAuth
	{
		public string JrtsAuthorizationsAssocMember;	
		public DateTime? JrtsAuthorizationDatesAssocMember;	
		public string JrtsAuthorizationLevelsAssocMember;	
		public JrnlEntsJrtsAuth() {}
		public JrnlEntsJrtsAuth(
			string inJrtsAuthorizations,
			DateTime? inJrtsAuthorizationDates,
			string inJrtsAuthorizationLevels)
		{
			JrtsAuthorizationsAssocMember = inJrtsAuthorizations;
			JrtsAuthorizationDatesAssocMember = inJrtsAuthorizationDates;
			JrtsAuthorizationLevelsAssocMember = inJrtsAuthorizationLevels;
		}
	}
	
	[Serializable]
	public class JrnlEntsJrtsAppr
	{
		public string JrtsNextApprovalIdsAssocMember;	
		public string JrtsApprovalLevelsAssocMember;	
		public JrnlEntsJrtsAppr() {}
		public JrnlEntsJrtsAppr(
			string inJrtsNextApprovalIds,
			string inJrtsApprovalLevels)
		{
			JrtsNextApprovalIdsAssocMember = inJrtsNextApprovalIds;
			JrtsApprovalLevelsAssocMember = inJrtsApprovalLevels;
		}
	}
}