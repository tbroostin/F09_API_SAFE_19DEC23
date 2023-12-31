//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/12/2020 11:38:59 AM by user vaidyasubrahmanyadv
//
//     Type: ENTITY
//     Entity: TAX.FORM.1099NEC.YEARS
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
	[DataContract(Name = "TaxForm1099necYears")]
	[ColleagueDataContract(GeneratedDateTime = "8/12/2020 11:38:59 AM", User = "vaidyasubrahmanyadv")]
	[EntityDataContract(EntityName = "TAX.FORM.1099NEC.YEARS", EntityType = "PHYS")]
	public class TaxForm1099necYears : IColleagueEntity
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
		/// CDD Name: TFNEY.WEB.FORMAT.TEMPLATE
		/// </summary>
		[DataMember(Order = 0, Name = "TFNEY.WEB.FORMAT.TEMPLATE")]
		public string TfneyWebFormatTemplate { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.BATCH.FORMAT.TEMPLATE
		/// </summary>
		[DataMember(Order = 1, Name = "TFNEY.BATCH.FORMAT.TEMPLATE")]
		public string TfneyBatchFormatTemplate { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.WEB.ENABLED
		/// </summary>
		[DataMember(Order = 2, Name = "TFNEY.WEB.ENABLED")]
		public string TfneyWebEnabled { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.SUBMIT.SEQ.NOS
		/// </summary>
		[DataMember(Order = 3, Name = "TFNEY.SUBMIT.SEQ.NOS")]
		public List<string> TfneySubmitSeqNos { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.SUBMIT.DATES
		/// </summary>
		[DataMember(Order = 4, Name = "TFNEY.SUBMIT.DATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> TfneySubmitDates { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.SUBMIT.TITLES
		/// </summary>
		[DataMember(Order = 5, Name = "TFNEY.SUBMIT.TITLES")]
		public List<string> TfneySubmitTitles { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.SUBMIT.TIMES
		/// </summary>
		[DataMember(Order = 6, Name = "TFNEY.SUBMIT.TIMES")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> TfneySubmitTimes { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.SUBMIT.OPERS
		/// </summary>
		[DataMember(Order = 7, Name = "TFNEY.SUBMIT.OPERS")]
		public List<string> TfneySubmitOpers { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.FORM.A.FORMAT.TEMPLATE
		/// </summary>
		[DataMember(Order = 8, Name = "TFNEY.FORM.A.FORMAT.TEMPLATE")]
		public string TfneyFormAFormatTemplate { get; set; }
		
		/// <summary>
		/// CDD Name: TFNEY.MASK.SSN
		/// </summary>
		[DataMember(Order = 9, Name = "TFNEY.MASK.SSN")]
		public string TfneyMaskSsn { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<TaxForm1099necYearsTfneySubmitted> TfneySubmittedEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: TFNEY.SUBMITTED
			
			TfneySubmittedEntityAssociation = new List<TaxForm1099necYearsTfneySubmitted>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(TfneySubmitSeqNos != null)
			{
				int numTfneySubmitted = TfneySubmitSeqNos.Count;
				if (TfneySubmitDates !=null && TfneySubmitDates.Count > numTfneySubmitted) numTfneySubmitted = TfneySubmitDates.Count;
				if (TfneySubmitTitles !=null && TfneySubmitTitles.Count > numTfneySubmitted) numTfneySubmitted = TfneySubmitTitles.Count;
				if (TfneySubmitTimes !=null && TfneySubmitTimes.Count > numTfneySubmitted) numTfneySubmitted = TfneySubmitTimes.Count;
				if (TfneySubmitOpers !=null && TfneySubmitOpers.Count > numTfneySubmitted) numTfneySubmitted = TfneySubmitOpers.Count;

				for (int i = 0; i < numTfneySubmitted; i++)
				{

					string value0 = "";
					if (TfneySubmitSeqNos != null && i < TfneySubmitSeqNos.Count)
					{
						value0 = TfneySubmitSeqNos[i];
					}


					DateTime? value1 = null;
					if (TfneySubmitDates != null && i < TfneySubmitDates.Count)
					{
						value1 = TfneySubmitDates[i];
					}


					string value2 = "";
					if (TfneySubmitTitles != null && i < TfneySubmitTitles.Count)
					{
						value2 = TfneySubmitTitles[i];
					}


					DateTime? value3 = null;
					if (TfneySubmitTimes != null && i < TfneySubmitTimes.Count)
					{
						value3 = TfneySubmitTimes[i];
					}


					string value4 = "";
					if (TfneySubmitOpers != null && i < TfneySubmitOpers.Count)
					{
						value4 = TfneySubmitOpers[i];
					}

					TfneySubmittedEntityAssociation.Add(new TaxForm1099necYearsTfneySubmitted( value0, value1, value2, value3, value4));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class TaxForm1099necYearsTfneySubmitted
	{
		public string TfneySubmitSeqNosAssocMember;	
		public DateTime? TfneySubmitDatesAssocMember;	
		public string TfneySubmitTitlesAssocMember;	
		public DateTime? TfneySubmitTimesAssocMember;	
		public string TfneySubmitOpersAssocMember;	
		public TaxForm1099necYearsTfneySubmitted() {}
		public TaxForm1099necYearsTfneySubmitted(
			string inTfneySubmitSeqNos,
			DateTime? inTfneySubmitDates,
			string inTfneySubmitTitles,
			DateTime? inTfneySubmitTimes,
			string inTfneySubmitOpers)
		{
			TfneySubmitSeqNosAssocMember = inTfneySubmitSeqNos;
			TfneySubmitDatesAssocMember = inTfneySubmitDates;
			TfneySubmitTitlesAssocMember = inTfneySubmitTitles;
			TfneySubmitTimesAssocMember = inTfneySubmitTimes;
			TfneySubmitOpersAssocMember = inTfneySubmitOpers;
		}
	}
}