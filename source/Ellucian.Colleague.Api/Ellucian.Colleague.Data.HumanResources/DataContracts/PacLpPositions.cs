//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/3/2017 2:22:33 PM by user nickkirschke
//
//     Type: ENTITY
//     Entity: PAC.LP.POSITIONS
//     Application: HR
//     Environment: coldevwapp01
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
	[DataContract(Name = "PacLpPositions")]
	[ColleagueDataContract(GeneratedDateTime = "10/3/2017 2:22:33 PM", User = "nickkirschke")]
	[EntityDataContract(EntityName = "PAC.LP.POSITIONS", EntityType = "PHYS")]
	public class PacLpPositions : IColleagueEntity
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
		/// CDD Name: PLPP.PAC.LOAD.PERIODS.ID
		/// </summary>
		[DataMember(Order = 0, Name = "PLPP.PAC.LOAD.PERIODS.ID")]
		public string PlppPacLoadPeriodsId { get; set; }
		
		/// <summary>
		/// CDD Name: PLPP.POSITION.ID
		/// </summary>
		[DataMember(Order = 2, Name = "PLPP.POSITION.ID")]
		public string PlppPositionId { get; set; }
		
		/// <summary>
		/// CDD Name: PLPP.INTENDED.LOAD
		/// </summary>
		[DataMember(Order = 3, Name = "PLPP.INTENDED.LOAD")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PlppIntendedLoad { get; set; }
		
		/// <summary>
		/// CDD Name: PLPP.PAC.LP.ASGMTS.IDS
		/// </summary>
		[DataMember(Order = 29, Name = "PLPP.PAC.LP.ASGMTS.IDS")]
		public List<string> PlppPacLpAsgmtsIds { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}