//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:34:56 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: CASH.RCPTS
//     Application: ST
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

namespace Ellucian.Colleague.Data.Finance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CashRcpts")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:34:56 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "CASH.RCPTS", EntityType = "PHYS")]
	public class CashRcpts : IColleagueEntity
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
		/// CDD Name: RCPT.NO
		/// </summary>
		[DataMember(Order = 0, Name = "RCPT.NO")]
		public string RcptNo { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "RCPT.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RcptDate { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.SESSION
		/// </summary>
		[DataMember(Order = 4, Name = "RCPT.SESSION")]
		public string RcptSession { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.PAYER.ID
		/// </summary>
		[DataMember(Order = 6, Name = "RCPT.PAYER.ID")]
		public string RcptPayerId { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.PAYER.DESC
		/// </summary>
		[DataMember(Order = 7, Name = "RCPT.PAYER.DESC")]
		public string RcptPayerDesc { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.DEPOSITS
		/// </summary>
		[DataMember(Order = 9, Name = "RCPT.DEPOSITS")]
		public List<string> RcptDeposits { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.TENDER.GL.DISTR.CODE
		/// </summary>
		[DataMember(Order = 15, Name = "RCPT.TENDER.GL.DISTR.CODE")]
		public string RcptTenderGlDistrCode { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.CONTROL.NOS
		/// </summary>
		[DataMember(Order = 16, Name = "RCPT.CONTROL.NOS")]
		public List<string> RcptControlNos { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.EXPIRE.DATES
		/// </summary>
		[DataMember(Order = 17, Name = "RCPT.EXPIRE.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> RcptExpireDates { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.CONFIRM.NOS
		/// </summary>
		[DataMember(Order = 18, Name = "RCPT.CONFIRM.NOS")]
		public List<string> RcptConfirmNos { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.NON.CASH.AMTS
		/// </summary>
		[DataMember(Order = 19, Name = "RCPT.NON.CASH.AMTS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> RcptNonCashAmts { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.PAY.METHODS
		/// </summary>
		[DataMember(Order = 20, Name = "RCPT.PAY.METHODS")]
		public List<string> RcptPayMethods { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.NON.CASH.GL.NOS
		/// </summary>
		[DataMember(Order = 21, Name = "RCPT.NON.CASH.GL.NOS")]
		public List<string> RcptNonCashGlNos { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.NSFS
		/// </summary>
		[DataMember(Order = 28, Name = "RCPT.NSFS")]
		public List<string> RcptNsfs { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.NON.CASH.REVERSAL.AMTS
		/// </summary>
		[DataMember(Order = 32, Name = "RCPT.NON.CASH.REVERSAL.AMTS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> RcptNonCashReversalAmts { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.NON.CASH.BANK.DEPOSITS
		/// </summary>
		[DataMember(Order = 33, Name = "RCPT.NON.CASH.BANK.DEPOSITS")]
		public List<string> RcptNonCashBankDeposits { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.FOREIGN.NON.CASH.AMTS
		/// </summary>
		[DataMember(Order = 37, Name = "RCPT.FOREIGN.NON.CASH.AMTS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> RcptForeignNonCashAmts { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.NON.CASH.TRANS.NOS
		/// </summary>
		[DataMember(Order = 50, Name = "RCPT.NON.CASH.TRANS.NOS")]
		public List<string> RcptNonCashTransNos { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.ENCRYPTED.CONTROL.NOS
		/// </summary>
		[DataMember(Order = 51, Name = "RCPT.ENCRYPTED.CONTROL.NOS")]
		public List<string> RcptEncryptedControlNos { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.PROVIDER.ACCTS
		/// </summary>
		[DataMember(Order = 52, Name = "RCPT.PROVIDER.ACCTS")]
		public List<string> RcptProviderAccts { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.NON.CASH.CONV.FEES
		/// </summary>
		[DataMember(Order = 53, Name = "RCPT.NON.CASH.CONV.FEES")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> RcptNonCashConvFees { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.ENCRYPTED.EXPIRE.DATES
		/// </summary>
		[DataMember(Order = 57, Name = "RCPT.ENCRYPTED.EXPIRE.DATES")]
		public List<string> RcptEncryptedExpireDates { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.ECOMM.PAY.METHODS
		/// </summary>
		[DataMember(Order = 58, Name = "RCPT.ECOMM.PAY.METHODS")]
		public List<string> RcptEcommPayMethods { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.USED.PAY.METHODS
		/// </summary>
		[DataMember(Order = 61, Name = "RCPT.USED.PAY.METHODS")]
		public List<string> RcptUsedPayMethods { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.EC.PAY.TRANS.IDS
		/// </summary>
		[DataMember(Order = 62, Name = "RCPT.EC.PAY.TRANS.IDS")]
		public List<string> RcptEcPayTransIds { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.EXTERNAL.SYSTEM
		/// </summary>
		[DataMember(Order = 63, Name = "RCPT.EXTERNAL.SYSTEM")]
		public string RcptExternalSystem { get; set; }
		
		/// <summary>
		/// CDD Name: RCPT.EXTERNAL.ID
		/// </summary>
		[DataMember(Order = 64, Name = "RCPT.EXTERNAL.ID")]
		public string RcptExternalId { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CashRcptsRcptNonCash> RcptNonCashEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: RCPT.NON.CASH
			
			RcptNonCashEntityAssociation = new List<CashRcptsRcptNonCash>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(RcptPayMethods != null)
			{
				int numRcptNonCash = RcptPayMethods.Count;
				if (RcptControlNos !=null && RcptControlNos.Count > numRcptNonCash) numRcptNonCash = RcptControlNos.Count;
				if (RcptExpireDates !=null && RcptExpireDates.Count > numRcptNonCash) numRcptNonCash = RcptExpireDates.Count;
				if (RcptConfirmNos !=null && RcptConfirmNos.Count > numRcptNonCash) numRcptNonCash = RcptConfirmNos.Count;
				if (RcptNonCashAmts !=null && RcptNonCashAmts.Count > numRcptNonCash) numRcptNonCash = RcptNonCashAmts.Count;
				if (RcptNonCashGlNos !=null && RcptNonCashGlNos.Count > numRcptNonCash) numRcptNonCash = RcptNonCashGlNos.Count;
				if (RcptNsfs !=null && RcptNsfs.Count > numRcptNonCash) numRcptNonCash = RcptNsfs.Count;
				if (RcptNonCashReversalAmts !=null && RcptNonCashReversalAmts.Count > numRcptNonCash) numRcptNonCash = RcptNonCashReversalAmts.Count;
				if (RcptNonCashBankDeposits !=null && RcptNonCashBankDeposits.Count > numRcptNonCash) numRcptNonCash = RcptNonCashBankDeposits.Count;
				if (RcptForeignNonCashAmts !=null && RcptForeignNonCashAmts.Count > numRcptNonCash) numRcptNonCash = RcptForeignNonCashAmts.Count;
				if (RcptNonCashTransNos !=null && RcptNonCashTransNos.Count > numRcptNonCash) numRcptNonCash = RcptNonCashTransNos.Count;
				if (RcptEncryptedControlNos !=null && RcptEncryptedControlNos.Count > numRcptNonCash) numRcptNonCash = RcptEncryptedControlNos.Count;
				if (RcptProviderAccts !=null && RcptProviderAccts.Count > numRcptNonCash) numRcptNonCash = RcptProviderAccts.Count;
				if (RcptNonCashConvFees !=null && RcptNonCashConvFees.Count > numRcptNonCash) numRcptNonCash = RcptNonCashConvFees.Count;
				if (RcptEncryptedExpireDates !=null && RcptEncryptedExpireDates.Count > numRcptNonCash) numRcptNonCash = RcptEncryptedExpireDates.Count;
				if (RcptEcommPayMethods !=null && RcptEcommPayMethods.Count > numRcptNonCash) numRcptNonCash = RcptEcommPayMethods.Count;
				if (RcptUsedPayMethods !=null && RcptUsedPayMethods.Count > numRcptNonCash) numRcptNonCash = RcptUsedPayMethods.Count;
				if (RcptEcPayTransIds !=null && RcptEcPayTransIds.Count > numRcptNonCash) numRcptNonCash = RcptEcPayTransIds.Count;

				for (int i = 0; i < numRcptNonCash; i++)
				{

					string value0 = "";
					if (RcptControlNos != null && i < RcptControlNos.Count)
					{
						value0 = RcptControlNos[i];
					}


					DateTime? value1 = null;
					if (RcptExpireDates != null && i < RcptExpireDates.Count)
					{
						value1 = RcptExpireDates[i];
					}


					string value2 = "";
					if (RcptConfirmNos != null && i < RcptConfirmNos.Count)
					{
						value2 = RcptConfirmNos[i];
					}


					Decimal? value3 = null;
					if (RcptNonCashAmts != null && i < RcptNonCashAmts.Count)
					{
						value3 = RcptNonCashAmts[i];
					}


					string value4 = "";
					if (RcptPayMethods != null && i < RcptPayMethods.Count)
					{
						value4 = RcptPayMethods[i];
					}


					string value5 = "";
					if (RcptNonCashGlNos != null && i < RcptNonCashGlNos.Count)
					{
						value5 = RcptNonCashGlNos[i];
					}


					string value6 = "";
					if (RcptNsfs != null && i < RcptNsfs.Count)
					{
						value6 = RcptNsfs[i];
					}


					Decimal? value7 = null;
					if (RcptNonCashReversalAmts != null && i < RcptNonCashReversalAmts.Count)
					{
						value7 = RcptNonCashReversalAmts[i];
					}


					string value8 = "";
					if (RcptNonCashBankDeposits != null && i < RcptNonCashBankDeposits.Count)
					{
						value8 = RcptNonCashBankDeposits[i];
					}


					Decimal? value9 = null;
					if (RcptForeignNonCashAmts != null && i < RcptForeignNonCashAmts.Count)
					{
						value9 = RcptForeignNonCashAmts[i];
					}


					string value10 = "";
					if (RcptNonCashTransNos != null && i < RcptNonCashTransNos.Count)
					{
						value10 = RcptNonCashTransNos[i];
					}


					string value11 = "";
					if (RcptEncryptedControlNos != null && i < RcptEncryptedControlNos.Count)
					{
						value11 = RcptEncryptedControlNos[i];
					}


					string value12 = "";
					if (RcptProviderAccts != null && i < RcptProviderAccts.Count)
					{
						value12 = RcptProviderAccts[i];
					}


					Decimal? value13 = null;
					if (RcptNonCashConvFees != null && i < RcptNonCashConvFees.Count)
					{
						value13 = RcptNonCashConvFees[i];
					}


					string value14 = "";
					if (RcptEncryptedExpireDates != null && i < RcptEncryptedExpireDates.Count)
					{
						value14 = RcptEncryptedExpireDates[i];
					}


					string value15 = "";
					if (RcptEcommPayMethods != null && i < RcptEcommPayMethods.Count)
					{
						value15 = RcptEcommPayMethods[i];
					}


					string value16 = "";
					if (RcptUsedPayMethods != null && i < RcptUsedPayMethods.Count)
					{
						value16 = RcptUsedPayMethods[i];
					}


					string value17 = "";
					if (RcptEcPayTransIds != null && i < RcptEcPayTransIds.Count)
					{
						value17 = RcptEcPayTransIds[i];
					}

					RcptNonCashEntityAssociation.Add(new CashRcptsRcptNonCash( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15, value16, value17));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class CashRcptsRcptNonCash
	{
		public string RcptControlNosAssocMember;	
		public DateTime? RcptExpireDatesAssocMember;	
		public string RcptConfirmNosAssocMember;	
		public Decimal? RcptNonCashAmtsAssocMember;	
		public string RcptPayMethodsAssocMember;	
		public string RcptNonCashGlNosAssocMember;	
		public string RcptNsfsAssocMember;	
		public Decimal? RcptNonCashReversalAmtsAssocMember;	
		public string RcptNonCashBankDepositsAssocMember;	
		public Decimal? RcptForeignNonCashAmtsAssocMember;	
		public string RcptNonCashTransNosAssocMember;	
		public string RcptEncryptedControlNosAssocMember;	
		public string RcptProviderAcctsAssocMember;	
		public Decimal? RcptNonCashConvFeesAssocMember;	
		public string RcptEncryptedExpireDatesAssocMember;	
		public string RcptEcommPayMethodsAssocMember;	
		public string RcptUsedPayMethodsAssocMember;	
		public string RcptEcPayTransIdsAssocMember;	
		public CashRcptsRcptNonCash() {}
		public CashRcptsRcptNonCash(
			string inRcptControlNos,
			DateTime? inRcptExpireDates,
			string inRcptConfirmNos,
			Decimal? inRcptNonCashAmts,
			string inRcptPayMethods,
			string inRcptNonCashGlNos,
			string inRcptNsfs,
			Decimal? inRcptNonCashReversalAmts,
			string inRcptNonCashBankDeposits,
			Decimal? inRcptForeignNonCashAmts,
			string inRcptNonCashTransNos,
			string inRcptEncryptedControlNos,
			string inRcptProviderAccts,
			Decimal? inRcptNonCashConvFees,
			string inRcptEncryptedExpireDates,
			string inRcptEcommPayMethods,
			string inRcptUsedPayMethods,
			string inRcptEcPayTransIds)
		{
			RcptControlNosAssocMember = inRcptControlNos;
			RcptExpireDatesAssocMember = inRcptExpireDates;
			RcptConfirmNosAssocMember = inRcptConfirmNos;
			RcptNonCashAmtsAssocMember = inRcptNonCashAmts;
			RcptPayMethodsAssocMember = inRcptPayMethods;
			RcptNonCashGlNosAssocMember = inRcptNonCashGlNos;
			RcptNsfsAssocMember = inRcptNsfs;
			RcptNonCashReversalAmtsAssocMember = inRcptNonCashReversalAmts;
			RcptNonCashBankDepositsAssocMember = inRcptNonCashBankDeposits;
			RcptForeignNonCashAmtsAssocMember = inRcptForeignNonCashAmts;
			RcptNonCashTransNosAssocMember = inRcptNonCashTransNos;
			RcptEncryptedControlNosAssocMember = inRcptEncryptedControlNos;
			RcptProviderAcctsAssocMember = inRcptProviderAccts;
			RcptNonCashConvFeesAssocMember = inRcptNonCashConvFees;
			RcptEncryptedExpireDatesAssocMember = inRcptEncryptedExpireDates;
			RcptEcommPayMethodsAssocMember = inRcptEcommPayMethods;
			RcptUsedPayMethodsAssocMember = inRcptUsedPayMethods;
			RcptEcPayTransIdsAssocMember = inRcptEcPayTransIds;
		}
	}
}