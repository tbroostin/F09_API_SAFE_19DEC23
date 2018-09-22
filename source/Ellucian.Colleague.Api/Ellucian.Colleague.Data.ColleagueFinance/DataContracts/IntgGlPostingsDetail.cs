//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/27/2017 8:56:53 AM by user sbhole1
//
//     Type: ENTITY
//     Entity: INTG.GL.POSTINGS.DETAIL
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
	[DataContract(Name = "IntgGlPostingsDetail")]
	[ColleagueDataContract(GeneratedDateTime = "10/27/2017 8:56:53 AM", User = "sbhole1")]
	[EntityDataContract(EntityName = "INTG.GL.POSTINGS.DETAIL", EntityType = "PHYS")]
	public class IntgGlPostingsDetail : IColleagueEntity
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
		/// CDD Name: IGPD.CREDIT
		/// </summary>
		[DataMember(Order = 4, Name = "IGPD.CREDIT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> IgpdCredit { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.GL.NO
		/// </summary>
		[DataMember(Order = 5, Name = "IGPD.GL.NO")]
		public List<string> IgpdGlNo { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.DEBIT
		/// </summary>
		[DataMember(Order = 6, Name = "IGPD.DEBIT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> IgpdDebit { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.DESCRIPTION
		/// </summary>
		[DataMember(Order = 7, Name = "IGPD.DESCRIPTION")]
		public List<string> IgpdDescription { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.PRJ.ITEMS.IDS
		/// </summary>
		[DataMember(Order = 8, Name = "IGPD.PRJ.ITEMS.IDS")]
		public List<string> IgpdPrjItemsIds { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.PROJECT.IDS
		/// </summary>
		[DataMember(Order = 9, Name = "IGPD.PROJECT.IDS")]
		public List<string> IgpdProjectIds { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.TRAN.SEQ.NO
		/// </summary>
		[DataMember(Order = 10, Name = "IGPD.TRAN.SEQ.NO")]
		public List<string> IgpdTranSeqNo { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.SUBMITTED.BY
		/// </summary>
		[DataMember(Order = 11, Name = "IGPD.SUBMITTED.BY")]
		public List<string> IgpdSubmittedBy { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.GIFT.UNITS
		/// </summary>
		[DataMember(Order = 12, Name = "IGPD.GIFT.UNITS")]
		public List<string> IgpdGiftUnits { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.ENC.REF.NO
		/// </summary>
		[DataMember(Order = 13, Name = "IGPD.ENC.REF.NO")]
		public List<string> IgpdEncRefNo { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.ENC.LINE.ITEM.NO
		/// </summary>
		[DataMember(Order = 14, Name = "IGPD.ENC.LINE.ITEM.NO")]
		public List<string> IgpdEncLineItemNo { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.ENC.SEQ.NO
		/// </summary>
		[DataMember(Order = 15, Name = "IGPD.ENC.SEQ.NO")]
		public List<string> IgpdEncSeqNo { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.ENC.ADJ.TYPE
		/// </summary>
		[DataMember(Order = 16, Name = "IGPD.ENC.ADJ.TYPE")]
		public List<string> IgpdEncAdjType { get; set; }
		
		/// <summary>
		/// CDD Name: IGPD.ENC.COMMITMENT.TYPE
		/// </summary>
		[DataMember(Order = 17, Name = "IGPD.ENC.COMMITMENT.TYPE")]
		public List<string> IgpdEncCommitmentType { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<IntgGlPostingsDetailIgpdTranDetails> IgpdTranDetailsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: IGPD.TRAN.DETAILS
			
			IgpdTranDetailsEntityAssociation = new List<IntgGlPostingsDetailIgpdTranDetails>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(IgpdGlNo != null)
			{
				int numIgpdTranDetails = IgpdGlNo.Count;
				if (IgpdCredit !=null && IgpdCredit.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdCredit.Count;
				if (IgpdDebit !=null && IgpdDebit.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdDebit.Count;
				if (IgpdDescription !=null && IgpdDescription.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdDescription.Count;
				if (IgpdPrjItemsIds !=null && IgpdPrjItemsIds.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdPrjItemsIds.Count;
				if (IgpdProjectIds !=null && IgpdProjectIds.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdProjectIds.Count;
				if (IgpdTranSeqNo !=null && IgpdTranSeqNo.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdTranSeqNo.Count;
				if (IgpdSubmittedBy !=null && IgpdSubmittedBy.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdSubmittedBy.Count;
				if (IgpdGiftUnits !=null && IgpdGiftUnits.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdGiftUnits.Count;
				if (IgpdEncRefNo !=null && IgpdEncRefNo.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdEncRefNo.Count;
				if (IgpdEncLineItemNo !=null && IgpdEncLineItemNo.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdEncLineItemNo.Count;
				if (IgpdEncSeqNo !=null && IgpdEncSeqNo.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdEncSeqNo.Count;
				if (IgpdEncAdjType !=null && IgpdEncAdjType.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdEncAdjType.Count;
				if (IgpdEncCommitmentType !=null && IgpdEncCommitmentType.Count > numIgpdTranDetails) numIgpdTranDetails = IgpdEncCommitmentType.Count;

				for (int i = 0; i < numIgpdTranDetails; i++)
				{

					Decimal? value0 = null;
					if (IgpdCredit != null && i < IgpdCredit.Count)
					{
						value0 = IgpdCredit[i];
					}


					string value1 = "";
					if (IgpdGlNo != null && i < IgpdGlNo.Count)
					{
						value1 = IgpdGlNo[i];
					}


					Decimal? value2 = null;
					if (IgpdDebit != null && i < IgpdDebit.Count)
					{
						value2 = IgpdDebit[i];
					}


					string value3 = "";
					if (IgpdDescription != null && i < IgpdDescription.Count)
					{
						value3 = IgpdDescription[i];
					}


					string value4 = "";
					if (IgpdPrjItemsIds != null && i < IgpdPrjItemsIds.Count)
					{
						value4 = IgpdPrjItemsIds[i];
					}


					string value5 = "";
					if (IgpdProjectIds != null && i < IgpdProjectIds.Count)
					{
						value5 = IgpdProjectIds[i];
					}


					string value6 = "";
					if (IgpdTranSeqNo != null && i < IgpdTranSeqNo.Count)
					{
						value6 = IgpdTranSeqNo[i];
					}


					string value7 = "";
					if (IgpdSubmittedBy != null && i < IgpdSubmittedBy.Count)
					{
						value7 = IgpdSubmittedBy[i];
					}


					string value8 = "";
					if (IgpdGiftUnits != null && i < IgpdGiftUnits.Count)
					{
						value8 = IgpdGiftUnits[i];
					}


					string value9 = "";
					if (IgpdEncRefNo != null && i < IgpdEncRefNo.Count)
					{
						value9 = IgpdEncRefNo[i];
					}


					string value10 = "";
					if (IgpdEncLineItemNo != null && i < IgpdEncLineItemNo.Count)
					{
						value10 = IgpdEncLineItemNo[i];
					}


					string value11 = "";
					if (IgpdEncSeqNo != null && i < IgpdEncSeqNo.Count)
					{
						value11 = IgpdEncSeqNo[i];
					}


					string value12 = "";
					if (IgpdEncAdjType != null && i < IgpdEncAdjType.Count)
					{
						value12 = IgpdEncAdjType[i];
					}


					string value13 = "";
					if (IgpdEncCommitmentType != null && i < IgpdEncCommitmentType.Count)
					{
						value13 = IgpdEncCommitmentType[i];
					}

					IgpdTranDetailsEntityAssociation.Add(new IntgGlPostingsDetailIgpdTranDetails( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class IntgGlPostingsDetailIgpdTranDetails
	{
		public Decimal? IgpdCreditAssocMember;	
		public string IgpdGlNoAssocMember;	
		public Decimal? IgpdDebitAssocMember;	
		public string IgpdDescriptionAssocMember;	
		public string IgpdPrjItemsIdsAssocMember;	
		public string IgpdProjectIdsAssocMember;	
		public string IgpdTranSeqNoAssocMember;	
		public string IgpdSubmittedByAssocMember;	
		public string IgpdGiftUnitsAssocMember;	
		public string IgpdEncRefNoAssocMember;	
		public string IgpdEncLineItemNoAssocMember;	
		public string IgpdEncSeqNoAssocMember;	
		public string IgpdEncAdjTypeAssocMember;	
		public string IgpdEncCommitmentTypeAssocMember;	
		public IntgGlPostingsDetailIgpdTranDetails() {}
		public IntgGlPostingsDetailIgpdTranDetails(
			Decimal? inIgpdCredit,
			string inIgpdGlNo,
			Decimal? inIgpdDebit,
			string inIgpdDescription,
			string inIgpdPrjItemsIds,
			string inIgpdProjectIds,
			string inIgpdTranSeqNo,
			string inIgpdSubmittedBy,
			string inIgpdGiftUnits,
			string inIgpdEncRefNo,
			string inIgpdEncLineItemNo,
			string inIgpdEncSeqNo,
			string inIgpdEncAdjType,
			string inIgpdEncCommitmentType)
		{
			IgpdCreditAssocMember = inIgpdCredit;
			IgpdGlNoAssocMember = inIgpdGlNo;
			IgpdDebitAssocMember = inIgpdDebit;
			IgpdDescriptionAssocMember = inIgpdDescription;
			IgpdPrjItemsIdsAssocMember = inIgpdPrjItemsIds;
			IgpdProjectIdsAssocMember = inIgpdProjectIds;
			IgpdTranSeqNoAssocMember = inIgpdTranSeqNo;
			IgpdSubmittedByAssocMember = inIgpdSubmittedBy;
			IgpdGiftUnitsAssocMember = inIgpdGiftUnits;
			IgpdEncRefNoAssocMember = inIgpdEncRefNo;
			IgpdEncLineItemNoAssocMember = inIgpdEncLineItemNo;
			IgpdEncSeqNoAssocMember = inIgpdEncSeqNo;
			IgpdEncAdjTypeAssocMember = inIgpdEncAdjType;
			IgpdEncCommitmentTypeAssocMember = inIgpdEncCommitmentType;
		}
	}
}