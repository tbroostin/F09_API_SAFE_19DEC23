//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 2:29:54 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: PAYMSTR
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Paymstr")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 2:29:54 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "PAYMSTR", EntityType = "PERM")]
	public class Paymstr : IColleagueEntity
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
		/// CDD Name: PM.INSTITUTION.NAME
		/// </summary>
		[DataMember(Order = 0, Name = "PM.INSTITUTION.NAME")]
		public string PmInstitutionName { get; set; }
		
		/// <summary>
		/// CDD Name: PM.INSTITUTION.ADDRESS
		/// </summary>
		[DataMember(Order = 1, Name = "PM.INSTITUTION.ADDRESS")]
		public List<string> PmInstitutionAddress { get; set; }
		
		/// <summary>
		/// CDD Name: PM.INSTITUTION.CITY
		/// </summary>
		[DataMember(Order = 2, Name = "PM.INSTITUTION.CITY")]
		public string PmInstitutionCity { get; set; }
		
		/// <summary>
		/// CDD Name: PM.INSTITUTION.STATE
		/// </summary>
		[DataMember(Order = 3, Name = "PM.INSTITUTION.STATE")]
		public string PmInstitutionState { get; set; }
		
		/// <summary>
		/// CDD Name: PM.INSTITUTION.ZIPCODE
		/// </summary>
		[DataMember(Order = 4, Name = "PM.INSTITUTION.ZIPCODE")]
		public string PmInstitutionZipcode { get; set; }
		
		/// <summary>
		/// CDD Name: PM.INSTITUTION.EIN
		/// </summary>
		[DataMember(Order = 5, Name = "PM.INSTITUTION.EIN")]
		public string PmInstitutionEin { get; set; }
		
		/// <summary>
		/// CDD Name: PM.INSTITUTION.SIN
		/// </summary>
		[DataMember(Order = 6, Name = "PM.INSTITUTION.SIN")]
		public string PmInstitutionSin { get; set; }
		
		/// <summary>
		/// CDD Name: PM.ZERO.BENDED.ON.STUB
		/// </summary>
		[DataMember(Order = 30, Name = "PM.ZERO.BENDED.ON.STUB")]
		public string PmZeroBendedOnStub { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}