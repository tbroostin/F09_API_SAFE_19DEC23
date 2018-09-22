//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/29/2017 8:09:28 AM by user bsf1
//
//     Type: ENTITY
//     Entity: AR.PAYMENTS
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "ArPayments")]
	[ColleagueDataContract(GeneratedDateTime = "11/29/2017 8:09:28 AM", User = "bsf1")]
	[EntityDataContract(EntityName = "AR.PAYMENTS", EntityType = "PHYS")]
	public class ArPayments : IColleagueEntity
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
		/// CDD Name: ARP.PERSON.ID
		/// </summary>
		[DataMember(Order = 0, Name = "ARP.PERSON.ID")]
		public string ArpPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.AR.TYPE
		/// </summary>
		[DataMember(Order = 1, Name = "ARP.AR.TYPE")]
		public string ArpArType { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "ARP.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ArpDate { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.CASH.RCPT
		/// </summary>
		[DataMember(Order = 3, Name = "ARP.CASH.RCPT")]
		public string ArpCashRcpt { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.TERM
		/// </summary>
		[DataMember(Order = 9, Name = "ARP.TERM")]
		public string ArpTerm { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.LOCATION
		/// </summary>
		[DataMember(Order = 10, Name = "ARP.LOCATION")]
		public string ArpLocation { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.AMT
		/// </summary>
		[DataMember(Order = 16, Name = "ARP.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArpAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.REVERSAL.AMT
		/// </summary>
		[DataMember(Order = 17, Name = "ARP.REVERSAL.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArpReversalAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.REVERSED.BY.PAYMENT
		/// </summary>
		[DataMember(Order = 40, Name = "ARP.REVERSED.BY.PAYMENT")]
		public string ArpReversedByPayment { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.ORIG.PAY.METHOD
		/// </summary>
		[DataMember(Order = 46, Name = "ARP.ORIG.PAY.METHOD")]
		public string ArpOrigPayMethod { get; set; }
		
		/// <summary>
		/// CDD Name: ARP.ARCHIVE
		/// </summary>
		[DataMember(Order = 50, Name = "ARP.ARCHIVE")]
		public string ArpArchive { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}