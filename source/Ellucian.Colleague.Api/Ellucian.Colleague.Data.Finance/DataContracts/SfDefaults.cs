//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/27/2021 12:28:31 PM by user tylerchristiansen
//
//     Type: ENTITY
//     Entity: SF.DEFAULTS
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

namespace Ellucian.Colleague.Data.Finance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "SfDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "4/27/2021 12:28:31 PM", User = "tylerchristiansen")]
	[EntityDataContract(EntityName = "SF.DEFAULTS", EntityType = "PERM")]
	public class SfDefaults : IColleagueEntity
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
		/// CDD Name: SF.PMT.INTVL
		/// </summary>
		[DataMember(Order = 0, Name = "SF.PMT.INTVL")]
		public string SfPmtIntvl { get; set; }
		
		/// <summary>
		/// CDD Name: SF.FIN.ACT.INTVL
		/// </summary>
		[DataMember(Order = 1, Name = "SF.FIN.ACT.INTVL")]
		public string SfFinActIntvl { get; set; }
		
		/// <summary>
		/// CDD Name: SF.INTVL.BEGIN.BASIS
		/// </summary>
		[DataMember(Order = 2, Name = "SF.INTVL.BEGIN.BASIS")]
		public string SfIntvlBeginBasis { get; set; }
		
		/// <summary>
		/// CDD Name: SF.INTVL.BEGIN.OFFSET
		/// </summary>
		[DataMember(Order = 3, Name = "SF.INTVL.BEGIN.OFFSET")]
		public int? SfIntvlBeginOffset { get; set; }
		
		/// <summary>
		/// CDD Name: SF.INTVL.END.BASIS
		/// </summary>
		[DataMember(Order = 4, Name = "SF.INTVL.END.BASIS")]
		public string SfIntvlEndBasis { get; set; }
		
		/// <summary>
		/// CDD Name: SF.INTVL.END.OFFSET
		/// </summary>
		[DataMember(Order = 5, Name = "SF.INTVL.END.OFFSET")]
		public int? SfIntvlEndOffset { get; set; }
		
		/// <summary>
		/// CDD Name: SF.AR.DEPOSIT.TYPES
		/// </summary>
		[DataMember(Order = 6, Name = "SF.AR.DEPOSIT.TYPES")]
		public List<string> SfArDepositTypes { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ARD.AR.TYPES
		/// </summary>
		[DataMember(Order = 7, Name = "SF.ARD.AR.TYPES")]
		public List<string> SfArdArTypes { get; set; }
		
		/// <summary>
		/// CDD Name: SF.SHOW.CREDIT.AMTS
		/// </summary>
		[DataMember(Order = 8, Name = "SF.SHOW.CREDIT.AMTS")]
		public string SfShowCreditAmts { get; set; }
		
		/// <summary>
		/// CDD Name: SF.PMTS.ENABLED
		/// </summary>
		[DataMember(Order = 9, Name = "SF.PMTS.ENABLED")]
		public string SfPmtsEnabled { get; set; }
		
		/// <summary>
		/// CDD Name: SF.CONFIRM.TEXT
		/// </summary>
		[DataMember(Order = 10, Name = "SF.CONFIRM.TEXT")]
		public string SfConfirmText { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ACKNOWLEDGE.PARA
		/// </summary>
		[DataMember(Order = 11, Name = "SF.ACKNOWLEDGE.PARA")]
		public string SfAcknowledgePara { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ACKNOWLEDGE.FROM.EMAIL
		/// </summary>
		[DataMember(Order = 12, Name = "SF.ACKNOWLEDGE.FROM.EMAIL")]
		public string SfAcknowledgeFromEmail { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ACKNOWLEDGE.REPLY.EMAIL
		/// </summary>
		[DataMember(Order = 13, Name = "SF.ACKNOWLEDGE.REPLY.EMAIL")]
		public string SfAcknowledgeReplyEmail { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ACKNOWLEDGE.COPY.EMAIL
		/// </summary>
		[DataMember(Order = 14, Name = "SF.ACKNOWLEDGE.COPY.EMAIL")]
		public string SfAcknowledgeCopyEmail { get; set; }
		
		/// <summary>
		/// CDD Name: SF.MERCHANT.PERSON.ID
		/// </summary>
		[DataMember(Order = 15, Name = "SF.MERCHANT.PERSON.ID")]
		public string SfMerchantPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: SF.MERCHANT.CONTACT.PHONE
		/// </summary>
		[DataMember(Order = 16, Name = "SF.MERCHANT.CONTACT.PHONE")]
		public string SfMerchantContactPhone { get; set; }
		
		/// <summary>
		/// CDD Name: SF.MERCHANT.CONTACT.NAME
		/// </summary>
		[DataMember(Order = 17, Name = "SF.MERCHANT.CONTACT.NAME")]
		public string SfMerchantContactName { get; set; }
		
		/// <summary>
		/// CDD Name: SF.MERCHANT.CONTACT.EMAIL
		/// </summary>
		[DataMember(Order = 18, Name = "SF.MERCHANT.CONTACT.EMAIL")]
		public string SfMerchantContactEmail { get; set; }
		
		/// <summary>
		/// CDD Name: SF.OFFICE.CODE
		/// </summary>
		[DataMember(Order = 19, Name = "SF.OFFICE.CODE")]
		public string SfOfficeCode { get; set; }
		
		/// <summary>
		/// CDD Name: SF.SUPPORT.EMAIL
		/// </summary>
		[DataMember(Order = 20, Name = "SF.SUPPORT.EMAIL")]
		public string SfSupportEmail { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ACKNOWLEDGE.FOOTER.TEXT
		/// </summary>
		[DataMember(Order = 21, Name = "SF.ACKNOWLEDGE.FOOTER.TEXT")]
		public string SfAcknowledgeFooterText { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ACKNOWLEDGE.FOOTER.IMAGE
		/// </summary>
		[DataMember(Order = 22, Name = "SF.ACKNOWLEDGE.FOOTER.IMAGE")]
		public string SfAcknowledgeFooterImage { get; set; }
		
		/// <summary>
		/// CDD Name: SF.NOTIFICATION.TEXT
		/// </summary>
		[DataMember(Order = 23, Name = "SF.NOTIFICATION.TEXT")]
		public string SfNotificationText { get; set; }
		
		/// <summary>
		/// CDD Name: SF.CUR.PRD.BEGIN.DATE
		/// </summary>
		[DataMember(Order = 24, Name = "SF.CUR.PRD.BEGIN.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SfCurPrdBeginDate { get; set; }
		
		/// <summary>
		/// CDD Name: SF.CUR.PRD.END.DATE
		/// </summary>
		[DataMember(Order = 25, Name = "SF.CUR.PRD.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SfCurPrdEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: SF.CUR.PRD.LAST.UPDATED.DATE
		/// </summary>
		[DataMember(Order = 26, Name = "SF.CUR.PRD.LAST.UPDATED.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SfCurPrdLastUpdatedDate { get; set; }
		
		/// <summary>
		/// CDD Name: SF.STMT.INCL.SCHEDULE
		/// </summary>
		[DataMember(Order = 27, Name = "SF.STMT.INCL.SCHEDULE")]
		public string SfStmtInclSchedule { get; set; }
		
		/// <summary>
		/// CDD Name: SF.STMT.INCL.DETAIL
		/// </summary>
		[DataMember(Order = 28, Name = "SF.STMT.INCL.DETAIL")]
		public string SfStmtInclDetail { get; set; }
		
		/// <summary>
		/// CDD Name: SF.STMT.INCL.HISTORY
		/// </summary>
		[DataMember(Order = 29, Name = "SF.STMT.INCL.HISTORY")]
		public string SfStmtInclHistory { get; set; }
		
		/// <summary>
		/// CDD Name: SF.STMT.INST.NAME
		/// </summary>
		[DataMember(Order = 30, Name = "SF.STMT.INST.NAME")]
		public string SfStmtInstName { get; set; }
		
		/// <summary>
		/// CDD Name: SF.STMT.TITLE
		/// </summary>
		[DataMember(Order = 31, Name = "SF.STMT.TITLE")]
		public string SfStmtTitle { get; set; }
		
		/// <summary>
		/// CDD Name: SF.STMT.INST.ADDR.LINES
		/// </summary>
		[DataMember(Order = 32, Name = "SF.STMT.INST.ADDR.LINES")]
		public List<string> SfStmtInstAddrLines { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ALLOW.DEPOSIT.PARTIAL.PMT
		/// </summary>
		[DataMember(Order = 33, Name = "SF.ALLOW.DEPOSIT.PARTIAL.PMT")]
		public string SfAllowDepositPartialPmt { get; set; }
		
		/// <summary>
		/// CDD Name: SF.DEPOSIT.PMT.DISTR
		/// </summary>
		[DataMember(Order = 34, Name = "SF.DEPOSIT.PMT.DISTR")]
		public string SfDepositPmtDistr { get; set; }
		
		/// <summary>
		/// CDD Name: SF.DUE.DATE.OVR.TERMS
		/// </summary>
		[DataMember(Order = 35, Name = "SF.DUE.DATE.OVR.TERMS")]
		public List<string> SfDueDateOvrTerms { get; set; }
		
		/// <summary>
		/// CDD Name: SF.DUE.DATE.OVR.DATES
		/// </summary>
		[DataMember(Order = 36, Name = "SF.DUE.DATE.OVR.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SfDueDateOvrDates { get; set; }
		
		/// <summary>
		/// CDD Name: SF.NON.TERM.DUE.DATE.OVR
		/// </summary>
		[DataMember(Order = 37, Name = "SF.NON.TERM.DUE.DATE.OVR")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SfNonTermDueDateOvr { get; set; }
		
		/// <summary>
		/// CDD Name: SF.PAST.PERIOD.DUE.DATE.OVR
		/// </summary>
		[DataMember(Order = 38, Name = "SF.PAST.PERIOD.DUE.DATE.OVR")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SfPastPeriodDueDateOvr { get; set; }
		
		/// <summary>
		/// CDD Name: SF.CUR.PERIOD.DUE.DATE.OVR
		/// </summary>
		[DataMember(Order = 39, Name = "SF.CUR.PERIOD.DUE.DATE.OVR")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SfCurPeriodDueDateOvr { get; set; }
		
		/// <summary>
		/// CDD Name: SF.FTR.PERIOD.DUE.DATE.OVR
		/// </summary>
		[DataMember(Order = 40, Name = "SF.FTR.PERIOD.DUE.DATE.OVR")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SfFtrPeriodDueDateOvr { get; set; }
		
		/// <summary>
		/// CDD Name: SF.STMT.MESSAGE
		/// </summary>
		[DataMember(Order = 42, Name = "SF.STMT.MESSAGE")]
		public List<string> SfStmtMessage { get; set; }
		
		/// <summary>
		/// CDD Name: SF.AR.TYPES.IDS
		/// </summary>
		[DataMember(Order = 43, Name = "SF.AR.TYPES.IDS")]
		public List<string> SfArTypesIds { get; set; }
		
		/// <summary>
		/// CDD Name: SF.AR.TYPES.PAYABLE.FLAGS
		/// </summary>
		[DataMember(Order = 44, Name = "SF.AR.TYPES.PAYABLE.FLAGS")]
		public List<string> SfArTypesPayableFlags { get; set; }
		
		/// <summary>
		/// CDD Name: SF.PROVIDER.LINK
		/// </summary>
		[DataMember(Order = 45, Name = "SF.PROVIDER.LINK")]
		public string SfProviderLink { get; set; }
		
		/// <summary>
		/// CDD Name: SF.ENABLE.D7.CALC.FLAG
		/// </summary>
		[DataMember(Order = 47, Name = "SF.ENABLE.D7.CALC.FLAG")]
		public string SfEnableD7CalcFlag { get; set; }
		
		/// <summary>
		/// CDD Name: SF.DISPLAY.DUE.DATES
		/// </summary>
		[DataMember(Order = 48, Name = "SF.DISPLAY.DUE.DATES")]
		public string SfDisplayDueDates { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SfDefaultsSfArDepositTypes> SfArDepositTypesEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SfDefaultsSfTermDueDateOvrs> SfTermDueDateOvrsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SfDefaultsSfArTypes> SfArTypesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: SF.AR.DEPOSIT.TYPES
			
			SfArDepositTypesEntityAssociation = new List<SfDefaultsSfArDepositTypes>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SfArDepositTypes != null)
			{
				int numSfArDepositTypes = SfArDepositTypes.Count;
				if (SfArdArTypes !=null && SfArdArTypes.Count > numSfArDepositTypes) numSfArDepositTypes = SfArdArTypes.Count;

				for (int i = 0; i < numSfArDepositTypes; i++)
				{

					string value0 = "";
					if (SfArDepositTypes != null && i < SfArDepositTypes.Count)
					{
						value0 = SfArDepositTypes[i];
					}


					string value1 = "";
					if (SfArdArTypes != null && i < SfArdArTypes.Count)
					{
						value1 = SfArdArTypes[i];
					}

					SfArDepositTypesEntityAssociation.Add(new SfDefaultsSfArDepositTypes( value0, value1));
				}
			}
			// EntityAssociation Name: SF.TERM.DUE.DATE.OVRS
			
			SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SfDueDateOvrTerms != null)
			{
				int numSfTermDueDateOvrs = SfDueDateOvrTerms.Count;
				if (SfDueDateOvrDates !=null && SfDueDateOvrDates.Count > numSfTermDueDateOvrs) numSfTermDueDateOvrs = SfDueDateOvrDates.Count;

				for (int i = 0; i < numSfTermDueDateOvrs; i++)
				{

					string value0 = "";
					if (SfDueDateOvrTerms != null && i < SfDueDateOvrTerms.Count)
					{
						value0 = SfDueDateOvrTerms[i];
					}


					DateTime? value1 = null;
					if (SfDueDateOvrDates != null && i < SfDueDateOvrDates.Count)
					{
						value1 = SfDueDateOvrDates[i];
					}

					SfTermDueDateOvrsEntityAssociation.Add(new SfDefaultsSfTermDueDateOvrs( value0, value1));
				}
			}
			// EntityAssociation Name: SF.AR.TYPES
			
			SfArTypesEntityAssociation = new List<SfDefaultsSfArTypes>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SfArTypesIds != null)
			{
				int numSfArTypes = SfArTypesIds.Count;
				if (SfArTypesPayableFlags !=null && SfArTypesPayableFlags.Count > numSfArTypes) numSfArTypes = SfArTypesPayableFlags.Count;

				for (int i = 0; i < numSfArTypes; i++)
				{

					string value0 = "";
					if (SfArTypesIds != null && i < SfArTypesIds.Count)
					{
						value0 = SfArTypesIds[i];
					}


					string value1 = "";
					if (SfArTypesPayableFlags != null && i < SfArTypesPayableFlags.Count)
					{
						value1 = SfArTypesPayableFlags[i];
					}

					SfArTypesEntityAssociation.Add(new SfDefaultsSfArTypes( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class SfDefaultsSfArDepositTypes
	{
		public string SfArDepositTypesAssocMember;	
		public string SfArdArTypesAssocMember;	
		public SfDefaultsSfArDepositTypes() {}
		public SfDefaultsSfArDepositTypes(
			string inSfArDepositTypes,
			string inSfArdArTypes)
		{
			SfArDepositTypesAssocMember = inSfArDepositTypes;
			SfArdArTypesAssocMember = inSfArdArTypes;
		}
	}
	
	[Serializable]
	public class SfDefaultsSfTermDueDateOvrs
	{
		public string SfDueDateOvrTermsAssocMember;	
		public DateTime? SfDueDateOvrDatesAssocMember;	
		public SfDefaultsSfTermDueDateOvrs() {}
		public SfDefaultsSfTermDueDateOvrs(
			string inSfDueDateOvrTerms,
			DateTime? inSfDueDateOvrDates)
		{
			SfDueDateOvrTermsAssocMember = inSfDueDateOvrTerms;
			SfDueDateOvrDatesAssocMember = inSfDueDateOvrDates;
		}
	}
	
	[Serializable]
	public class SfDefaultsSfArTypes
	{
		public string SfArTypesIdsAssocMember;	
		public string SfArTypesPayableFlagsAssocMember;	
		public SfDefaultsSfArTypes() {}
		public SfDefaultsSfArTypes(
			string inSfArTypesIds,
			string inSfArTypesPayableFlags)
		{
			SfArTypesIdsAssocMember = inSfArTypesIds;
			SfArTypesPayableFlagsAssocMember = inSfArTypesPayableFlags;
		}
	}
}