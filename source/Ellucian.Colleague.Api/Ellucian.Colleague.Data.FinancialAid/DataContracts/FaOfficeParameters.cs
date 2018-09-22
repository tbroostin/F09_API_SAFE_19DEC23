//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/9/2018 7:30:40 AM by user otorres
//
//     Type: ENTITY
//     Entity: FA.OFFICE.PARAMETERS
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.FinancialAid.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "FaOfficeParameters")]
	[ColleagueDataContract(GeneratedDateTime = "8/9/2018 7:30:40 AM", User = "otorres")]
	[EntityDataContract(EntityName = "FA.OFFICE.PARAMETERS", EntityType = "PHYS")]
	public class FaOfficeParameters : IColleagueEntity
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
		/// CDD Name: FOP.FA.OFFICE.CODE
		/// </summary>
		[DataMember(Order = 0, Name = "FOP.FA.OFFICE.CODE")]
		public string FopFaOfficeCode { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.YEAR
		/// </summary>
		[DataMember(Order = 1, Name = "FOP.YEAR")]
		public string FopYear { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SELF.SERVICE.AVAIL
		/// </summary>
		[DataMember(Order = 2, Name = "FOP.SELF.SERVICE.AVAIL")]
		public string FopSelfServiceAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.AWARDING.AVAIL
		/// </summary>
		[DataMember(Order = 3, Name = "FOP.AWARDING.AVAIL")]
		public string FopAwardingAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.AWARD.CHANGES.AVAIL
		/// </summary>
		[DataMember(Order = 4, Name = "FOP.AWARD.CHANGES.AVAIL")]
		public string FopAwardChangesAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.PROFILE.AVAIL
		/// </summary>
		[DataMember(Order = 5, Name = "FOP.PROFILE.AVAIL")]
		public string FopProfileAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWD.PDS.FROM.VIEW
		/// </summary>
		[DataMember(Order = 7, Name = "FOP.EXCL.AWD.PDS.FROM.VIEW")]
		public List<string> FopExclAwdPdsFromView { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.ACT.CAT.FROM.VIEW
		/// </summary>
		[DataMember(Order = 8, Name = "FOP.EXCL.ACT.CAT.FROM.VIEW")]
		public List<string> FopExclActCatFromView { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWD.CAT.FROM.VIEW
		/// </summary>
		[DataMember(Order = 9, Name = "FOP.EXCL.AWD.CAT.FROM.VIEW")]
		public List<string> FopExclAwdCatFromView { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWARDS.FROM.VIEW
		/// </summary>
		[DataMember(Order = 10, Name = "FOP.EXCL.AWARDS.FROM.VIEW")]
		public List<string> FopExclAwardsFromView { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.ACT.CAT.FROM.CHG
		/// </summary>
		[DataMember(Order = 11, Name = "FOP.EXCL.ACT.CAT.FROM.CHG")]
		public List<string> FopExclActCatFromChg { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWARDS.FROM.CHG
		/// </summary>
		[DataMember(Order = 12, Name = "FOP.EXCL.AWARDS.FROM.CHG")]
		public List<string> FopExclAwardsFromChg { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.NEG.UNMET.NEED
		/// </summary>
		[DataMember(Order = 13, Name = "FOP.NEG.UNMET.NEED")]
		public string FopNegUnmetNeed { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.LOAN.AMT.CHANGES
		/// </summary>
		[DataMember(Order = 15, Name = "FOP.LOAN.AMT.CHANGES")]
		public string FopLoanAmtChanges { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHANGE.ACCEPTED.LOANS
		/// </summary>
		[DataMember(Order = 16, Name = "FOP.CHANGE.ACCEPTED.LOANS")]
		public string FopChangeAcceptedLoans { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.ACC.AWDS.ACT.ST
		/// </summary>
		[DataMember(Order = 17, Name = "FOP.ACC.AWDS.ACT.ST")]
		public string FopAccAwdsActSt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.REJ.AWDS.ACT.ST
		/// </summary>
		[DataMember(Order = 18, Name = "FOP.REJ.AWDS.ACT.ST")]
		public string FopRejAwdsActSt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.ACC.AWDS.CC.CODE
		/// </summary>
		[DataMember(Order = 19, Name = "FOP.ACC.AWDS.CC.CODE")]
		public string FopAccAwdsCcCode { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.ACC.AWDS.CC.ST
		/// </summary>
		[DataMember(Order = 20, Name = "FOP.ACC.AWDS.CC.ST")]
		public string FopAccAwdsCcSt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.REJ.AWDS.CC.CODE
		/// </summary>
		[DataMember(Order = 21, Name = "FOP.REJ.AWDS.CC.CODE")]
		public string FopRejAwdsCcCode { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.REJ.AWDS.CC.ST
		/// </summary>
		[DataMember(Order = 22, Name = "FOP.REJ.AWDS.CC.ST")]
		public string FopRejAwdsCcSt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.NEW.LOAN.CC.CODE
		/// </summary>
		[DataMember(Order = 23, Name = "FOP.NEW.LOAN.CC.CODE")]
		public string FopNewLoanCcCode { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHG.LOAN.CC.CODE
		/// </summary>
		[DataMember(Order = 24, Name = "FOP.CHG.LOAN.CC.CODE")]
		public string FopChgLoanCcCode { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWD.CAT.FROM.CHG
		/// </summary>
		[DataMember(Order = 25, Name = "FOP.EXCL.AWD.CAT.FROM.CHG")]
		public List<string> FopExclAwdCatFromChg { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.GR.AVG.GRANT.AMT
		/// </summary>
		[DataMember(Order = 26, Name = "FOP.GR.AVG.GRANT.AMT")]
		public int? FopGrAvgGrantAmt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.GR.AVG.LOAN.AMT
		/// </summary>
		[DataMember(Order = 27, Name = "FOP.GR.AVG.LOAN.AMT")]
		public int? FopGrAvgLoanAmt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.GR.AVG.SCHOLARSHIP.AMT
		/// </summary>
		[DataMember(Order = 28, Name = "FOP.GR.AVG.SCHOLARSHIP.AMT")]
		public int? FopGrAvgScholarshipAmt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.UG.AVG.GRANT.AMT
		/// </summary>
		[DataMember(Order = 29, Name = "FOP.UG.AVG.GRANT.AMT")]
		public int? FopUgAvgGrantAmt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.UG.AVG.LOAN.AMT
		/// </summary>
		[DataMember(Order = 30, Name = "FOP.UG.AVG.LOAN.AMT")]
		public int? FopUgAvgLoanAmt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.UG.AVG.SCHOLARSHIP.AMT
		/// </summary>
		[DataMember(Order = 31, Name = "FOP.UG.AVG.SCHOLARSHIP.AMT")]
		public int? FopUgAvgScholarshipAmt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.YEAR.DESCRIPTION
		/// </summary>
		[DataMember(Order = 32, Name = "FOP.YEAR.DESCRIPTION")]
		public string FopYearDescription { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.NEW.LOAN.CC.STATUS
		/// </summary>
		[DataMember(Order = 34, Name = "FOP.NEW.LOAN.CC.STATUS")]
		public string FopNewLoanCcStatus { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHG.LOAN.CC.STATUS
		/// </summary>
		[DataMember(Order = 35, Name = "FOP.CHG.LOAN.CC.STATUS")]
		public string FopChgLoanCcStatus { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.ACT.CAT.FROM.AWDLTR
		/// </summary>
		[DataMember(Order = 36, Name = "FOP.EXCL.ACT.CAT.FROM.AWDLTR")]
		public List<string> FopExclActCatFromAwdltr { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.AWARD.LETTER.AVAIL
		/// </summary>
		[DataMember(Order = 37, Name = "FOP.AWARD.LETTER.AVAIL")]
		public string FopAwardLetterAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.LOAN.REQUESTS.AVAIL
		/// </summary>
		[DataMember(Order = 38, Name = "FOP.LOAN.REQUESTS.AVAIL")]
		public string FopLoanRequestsAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SUPPRESS.INSTANCE.DATA
		/// </summary>
		[DataMember(Order = 39, Name = "FOP.SUPPRESS.INSTANCE.DATA")]
		public string FopSuppressInstanceData { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SHOPPING.SHEET.AVAIL
		/// </summary>
		[DataMember(Order = 40, Name = "FOP.SHOPPING.SHEET.AVAIL")]
		public string FopShoppingSheetAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.PAPER.COPY.OPTION.DESC
		/// </summary>
		[DataMember(Order = 42, Name = "FOP.PAPER.COPY.OPTION.DESC")]
		public string FopPaperCopyOptionDesc { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.REVIEW.LOAN.CHANGES
		/// </summary>
		[DataMember(Order = 43, Name = "FOP.REVIEW.LOAN.CHANGES")]
		public string FopReviewLoanChanges { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.REVIEW.DECLINED.AWARDS
		/// </summary>
		[DataMember(Order = 44, Name = "FOP.REVIEW.DECLINED.AWARDS")]
		public string FopReviewDeclinedAwards { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.COUNSELOR.PHONE.TYPE
		/// </summary>
		[DataMember(Order = 45, Name = "FOP.COUNSELOR.PHONE.TYPE")]
		public string FopCounselorPhoneType { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.AWD.LTR.HIST.AVAIL
		/// </summary>
		[DataMember(Order = 46, Name = "FOP.AWD.LTR.HIST.AVAIL")]
		public string FopAwdLtrHistAvail { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.ACT.ST.FROM.CHG
		/// </summary>
		[DataMember(Order = 48, Name = "FOP.EXCL.ACT.ST.FROM.CHG")]
		public List<string> FopExclActStFromChg { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHECKLIST.NO.FIN.AID
		/// </summary>
		[DataMember(Order = 49, Name = "FOP.CHECKLIST.NO.FIN.AID")]
		public string FopChecklistNoFinAid { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.ANNUAL.ACCREJ.ONLY
		/// </summary>
		[DataMember(Order = 50, Name = "FOP.ANNUAL.ACCREJ.ONLY")]
		public string FopAnnualAccrejOnly { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.USE.DEFAULT.CONTACT
		/// </summary>
		[DataMember(Order = 51, Name = "FOP.USE.DEFAULT.CONTACT")]
		public string FopUseDefaultContact { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.USE.MAILING.CODE.DESC
		/// </summary>
		[DataMember(Order = 52, Name = "FOP.USE.MAILING.CODE.DESC")]
		public string FopUseMailingCodeDesc { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SUPRESS.LOAN.LIMIT
		/// </summary>
		[DataMember(Order = 53, Name = "FOP.SUPRESS.LOAN.LIMIT")]
		public string FopSupressLoanLimit { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHECKLIST.ITEMS
		/// </summary>
		[DataMember(Order = 54, Name = "FOP.CHECKLIST.ITEMS")]
		public List<string> FopChecklistItems { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHECKLIST.RULE.TABLE.ID
		/// </summary>
		[DataMember(Order = 55, Name = "FOP.CHECKLIST.RULE.TABLE.ID")]
		public List<string> FopChecklistRuleTableId { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHECKLIST.DISPLAY.ACTION
		/// </summary>
		[DataMember(Order = 56, Name = "FOP.CHECKLIST.DISPLAY.ACTION")]
		public List<string> FopChecklistDisplayAction { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.CHECKLIST.ASSIGN.BY.DFLT
		/// </summary>
		[DataMember(Order = 57, Name = "FOP.CHECKLIST.ASSIGN.BY.DFLT")]
		public List<string> FopChecklistAssignByDflt { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.IGNORE.ACT.ST.FROM.EVAL
		/// </summary>
		[DataMember(Order = 58, Name = "FOP.IGNORE.ACT.ST.FROM.EVAL")]
		public List<string> FopIgnoreActStFromEval { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.DISPLAY.PELL.LEU
		/// </summary>
		[DataMember(Order = 69, Name = "FOP.DISPLAY.PELL.LEU")]
		public string FopDisplayPellLeu { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SUPPRESS.AVG.PKG.DISPLAY
		/// </summary>
		[DataMember(Order = 70, Name = "FOP.SUPPRESS.AVG.PKG.DISPLAY")]
		public string FopSuppressAvgPkgDisplay { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SUPPRESS.ACT.SUM.DISPLAY
		/// </summary>
		[DataMember(Order = 71, Name = "FOP.SUPPRESS.ACT.SUM.DISPLAY")]
		public string FopSuppressActSumDisplay { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.IGNORE.AWD.CAT.FROM.EVAL
		/// </summary>
		[DataMember(Order = 72, Name = "FOP.IGNORE.AWD.CAT.FROM.EVAL")]
		public List<string> FopIgnoreAwdCatFromEval { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.IGNORE.AWARDS.FROM.EVAL
		/// </summary>
		[DataMember(Order = 73, Name = "FOP.IGNORE.AWARDS.FROM.EVAL")]
		public List<string> FopIgnoreAwardsFromEval { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWD.PDS.FROM.AWDLTR
		/// </summary>
		[DataMember(Order = 74, Name = "FOP.EXCL.AWD.PDS.FROM.AWDLTR")]
		public List<string> FopExclAwdPdsFromAwdltr { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWARDS.FROM.AWDLTR
		/// </summary>
		[DataMember(Order = 75, Name = "FOP.EXCL.AWARDS.FROM.AWDLTR")]
		public List<string> FopExclAwardsFromAwdltr { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.EXCL.AWD.CAT.FROM.AWDLTR
		/// </summary>
		[DataMember(Order = 76, Name = "FOP.EXCL.AWD.CAT.FROM.AWDLTR")]
		public List<string> FopExclAwdCatFromAwdltr { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.DECLINE.ZERO.ACC.LOANS
		/// </summary>
		[DataMember(Order = 77, Name = "FOP.DECLINE.ZERO.ACC.LOANS")]
		public string FopDeclineZeroAccLoans { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SUPPRESS.AWD.LTR.ACCEPT
		/// </summary>
		[DataMember(Order = 80, Name = "FOP.SUPPRESS.AWD.LTR.ACCEPT")]
		public string FopSuppressAwdLtrAccept { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.SUPPRESS.DISB.INFO.DISPL
		/// </summary>
		[DataMember(Order = 81, Name = "FOP.SUPPRESS.DISB.INFO.DISPL")]
		public string FopSuppressDisbInfoDispl { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.IGNORE.AWARDS.ON.CHKLST
		/// </summary>
		[DataMember(Order = 82, Name = "FOP.IGNORE.AWARDS.ON.CHKLST")]
		public List<string> FopIgnoreAwardsOnChklst { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.IGNORE.AWD.CAT.ON.CHKLST
		/// </summary>
		[DataMember(Order = 83, Name = "FOP.IGNORE.AWD.CAT.ON.CHKLST")]
		public List<string> FopIgnoreAwdCatOnChklst { get; set; }
		
		/// <summary>
		/// CDD Name: FOP.IGNORE.ACT.ST.ON.CHKLST
		/// </summary>
		[DataMember(Order = 84, Name = "FOP.IGNORE.ACT.ST.ON.CHKLST")]
		public List<string> FopIgnoreActStOnChklst { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<FaOfficeParametersFopChklstItems> FopChklstItemsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: FOP.CHKLST.ITEMS
			
			FopChklstItemsEntityAssociation = new List<FaOfficeParametersFopChklstItems>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(FopChecklistItems != null)
			{
				int numFopChklstItems = FopChecklistItems.Count;
				if (FopChecklistRuleTableId !=null && FopChecklistRuleTableId.Count > numFopChklstItems) numFopChklstItems = FopChecklistRuleTableId.Count;
				if (FopChecklistDisplayAction !=null && FopChecklistDisplayAction.Count > numFopChklstItems) numFopChklstItems = FopChecklistDisplayAction.Count;
				if (FopChecklistAssignByDflt !=null && FopChecklistAssignByDflt.Count > numFopChklstItems) numFopChklstItems = FopChecklistAssignByDflt.Count;

				for (int i = 0; i < numFopChklstItems; i++)
				{

					string value0 = "";
					if (FopChecklistItems != null && i < FopChecklistItems.Count)
					{
						value0 = FopChecklistItems[i];
					}


					string value1 = "";
					if (FopChecklistRuleTableId != null && i < FopChecklistRuleTableId.Count)
					{
						value1 = FopChecklistRuleTableId[i];
					}


					string value2 = "";
					if (FopChecklistDisplayAction != null && i < FopChecklistDisplayAction.Count)
					{
						value2 = FopChecklistDisplayAction[i];
					}


					string value3 = "";
					if (FopChecklistAssignByDflt != null && i < FopChecklistAssignByDflt.Count)
					{
						value3 = FopChecklistAssignByDflt[i];
					}

					FopChklstItemsEntityAssociation.Add(new FaOfficeParametersFopChklstItems( value0, value1, value2, value3));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class FaOfficeParametersFopChklstItems
	{
		public string FopChecklistItemsAssocMember;	
		public string FopChecklistRuleTableIdAssocMember;	
		public string FopChecklistDisplayActionAssocMember;	
		public string FopChecklistAssignByDfltAssocMember;	
		public FaOfficeParametersFopChklstItems() {}
		public FaOfficeParametersFopChklstItems(
			string inFopChecklistItems,
			string inFopChecklistRuleTableId,
			string inFopChecklistDisplayAction,
			string inFopChecklistAssignByDflt)
		{
			FopChecklistItemsAssocMember = inFopChecklistItems;
			FopChecklistRuleTableIdAssocMember = inFopChecklistRuleTableId;
			FopChecklistDisplayActionAssocMember = inFopChecklistDisplayAction;
			FopChecklistAssignByDfltAssocMember = inFopChecklistAssignByDflt;
		}
	}
}