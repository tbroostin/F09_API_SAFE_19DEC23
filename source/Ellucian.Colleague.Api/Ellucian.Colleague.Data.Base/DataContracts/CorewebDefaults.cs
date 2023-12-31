//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/4/2021 11:29:05 AM by user cindystair
//
//     Type: ENTITY
//     Entity: COREWEB.DEFAULTS
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CorewebDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "11/4/2021 11:29:05 AM", User = "cindystair")]
	[EntityDataContract(EntityName = "COREWEB.DEFAULTS", EntityType = "PERM")]
	public class CorewebDefaults : IColleagueEntity
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
		/// CDD Name: COREWEB.CC.CODES
		/// </summary>
		[DataMember(Order = 0, Name = "COREWEB.CC.CODES")]
		public List<string> CorewebCcCodes { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.SUPPRESS.INSTANCE
		/// </summary>
		[DataMember(Order = 1, Name = "COREWEB.SUPPRESS.INSTANCE")]
		public string CorewebSuppressInstance { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.DOCUMENTS.SORT1
		/// </summary>
		[DataMember(Order = 2, Name = "COREWEB.DOCUMENTS.SORT1")]
		public string CorewebDocumentsSort1 { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.DOCUMENTS.SORT2
		/// </summary>
		[DataMember(Order = 3, Name = "COREWEB.DOCUMENTS.SORT2")]
		public string CorewebDocumentsSort2 { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.BLANK.STATUS.TEXT
		/// </summary>
		[DataMember(Order = 4, Name = "COREWEB.BLANK.STATUS.TEXT")]
		public string CorewebBlankStatusText { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.BLANK.DUE.DATE.TEXT
		/// </summary>
		[DataMember(Order = 5, Name = "COREWEB.BLANK.DUE.DATE.TEXT")]
		public string CorewebBlankDueDateText { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.PHONE.VIEW.TYPES
		/// </summary>
		[DataMember(Order = 6, Name = "COREWEB.PHONE.VIEW.TYPES")]
		public List<string> CorewebPhoneViewTypes { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.EMAIL.VIEW.TYPES
		/// </summary>
		[DataMember(Order = 7, Name = "COREWEB.EMAIL.VIEW.TYPES")]
		public List<string> CorewebEmailViewTypes { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ADDRESS.VIEW.TYPES
		/// </summary>
		[DataMember(Order = 8, Name = "COREWEB.ADDRESS.VIEW.TYPES")]
		public List<string> CorewebAddressViewTypes { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.USER.PROFILE.TEXT
		/// </summary>
		[DataMember(Order = 9, Name = "COREWEB.USER.PROFILE.TEXT")]
		public string CorewebUserProfileText { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.EMAIL.UPDT.TYPES
		/// </summary>
		[DataMember(Order = 19, Name = "COREWEB.EMAIL.UPDT.TYPES")]
		public List<string> CorewebEmailUpdtTypes { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ALL.EMAIL.VIEWABLE
		/// </summary>
		[DataMember(Order = 21, Name = "COREWEB.ALL.EMAIL.VIEWABLE")]
		public string CorewebAllEmailViewable { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ALL.EMAIL.UPDATABLE
		/// </summary>
		[DataMember(Order = 22, Name = "COREWEB.ALL.EMAIL.UPDATABLE")]
		public string CorewebAllEmailUpdatable { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.EMAIL.UPDT.NO.PERM
		/// </summary>
		[DataMember(Order = 23, Name = "COREWEB.EMAIL.UPDT.NO.PERM")]
		public string CorewebEmailUpdtNoPerm { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ALL.ADDR.VIEWABLE
		/// </summary>
		[DataMember(Order = 24, Name = "COREWEB.ALL.ADDR.VIEWABLE")]
		public string CorewebAllAddrViewable { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ALL.PHONE.VIEWABLE
		/// </summary>
		[DataMember(Order = 25, Name = "COREWEB.ALL.PHONE.VIEWABLE")]
		public string CorewebAllPhoneViewable { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.PHONE.UPDT.TYPES
		/// </summary>
		[DataMember(Order = 26, Name = "COREWEB.PHONE.UPDT.TYPES")]
		public List<string> CorewebPhoneUpdtTypes { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.PHONE.UPDT.NO.PERM
		/// </summary>
		[DataMember(Order = 27, Name = "COREWEB.PHONE.UPDT.NO.PERM")]
		public string CorewebPhoneUpdtNoPerm { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ADDR.UPDATABLE
		/// </summary>
		[DataMember(Order = 28, Name = "COREWEB.ADDR.UPDATABLE")]
		public string CorewebAddrUpdatable { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ADDR.UPDT.NO.PERM
		/// </summary>
		[DataMember(Order = 29, Name = "COREWEB.ADDR.UPDT.NO.PERM")]
		public string CorewebAddrUpdtNoPerm { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.SEVERITY.START
		/// </summary>
		[DataMember(Order = 30, Name = "COREWEB.SEVERITY.START")]
		public List<int?> CorewebSeverityStart { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.SEVERITY.END
		/// </summary>
		[DataMember(Order = 31, Name = "COREWEB.SEVERITY.END")]
		public List<int?> CorewebSeverityEnd { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.STYLE
		/// </summary>
		[DataMember(Order = 32, Name = "COREWEB.STYLE")]
		public List<string> CorewebStyle { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.EMER.HIDE.HLTH.COND
		/// </summary>
		[DataMember(Order = 33, Name = "COREWEB.EMER.HIDE.HLTH.COND")]
		public string CorewebEmerHideHlthCond { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.EMER.ALLOW.OPTOUT
		/// </summary>
		[DataMember(Order = 34, Name = "COREWEB.EMER.ALLOW.OPTOUT")]
		public string CorewebEmerAllowOptout { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.EMER.REQUIRE.CONTACT
		/// </summary>
		[DataMember(Order = 35, Name = "COREWEB.EMER.REQUIRE.CONTACT")]
		public string CorewebEmerRequireContact { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.EMER.HIDE.OTHER.INFO
		/// </summary>
		[DataMember(Order = 36, Name = "COREWEB.EMER.HIDE.OTHER.INFO")]
		public string CorewebEmerHideOtherInfo { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.ADDRESS.UPDT.TYPES
		/// </summary>
		[DataMember(Order = 37, Name = "COREWEB.ADDRESS.UPDT.TYPES")]
		public List<string> CorewebAddressUpdtTypes { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.NULL.PHN.TYPE.VIEWBL
		/// </summary>
		[DataMember(Order = 38, Name = "COREWEB.NULL.PHN.TYPE.VIEWBL")]
		public string CorewebNullPhnTypeViewbl { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.CHO.NAME.OPTION
		/// </summary>
		[DataMember(Order = 39, Name = "COREWEB.CHO.NAME.OPTION")]
		public string CorewebChoNameOption { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.GEN.IDENT.OPTION
		/// </summary>
		[DataMember(Order = 40, Name = "COREWEB.GEN.IDENT.OPTION")]
		public string CorewebGenIdentOption { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.PRONOUN.OPTION
		/// </summary>
		[DataMember(Order = 41, Name = "COREWEB.PRONOUN.OPTION")]
		public string CorewebPronounOption { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.NICKNAME.OPTION
		/// </summary>
		[DataMember(Order = 42, Name = "COREWEB.NICKNAME.OPTION")]
		public string CorewebNicknameOption { get; set; }
		
		/// <summary>
		/// CDD Name: COREWEB.PHONE.TEXT.AUTH
		/// </summary>
		[DataMember(Order = 44, Name = "COREWEB.PHONE.TEXT.AUTH")]
		public string CorewebPhoneTextAuth { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CorewebDefaultsCorewebRestrStyles> CorewebRestrStylesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: COREWEB.RESTR.STYLES
			
			CorewebRestrStylesEntityAssociation = new List<CorewebDefaultsCorewebRestrStyles>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(CorewebSeverityStart != null)
			{
				int numCorewebRestrStyles = CorewebSeverityStart.Count;
				if (CorewebSeverityEnd !=null && CorewebSeverityEnd.Count > numCorewebRestrStyles) numCorewebRestrStyles = CorewebSeverityEnd.Count;
				if (CorewebStyle !=null && CorewebStyle.Count > numCorewebRestrStyles) numCorewebRestrStyles = CorewebStyle.Count;

				for (int i = 0; i < numCorewebRestrStyles; i++)
				{

					int? value0 = null;
					if (CorewebSeverityStart != null && i < CorewebSeverityStart.Count)
					{
						value0 = CorewebSeverityStart[i];
					}


					int? value1 = null;
					if (CorewebSeverityEnd != null && i < CorewebSeverityEnd.Count)
					{
						value1 = CorewebSeverityEnd[i];
					}


					string value2 = "";
					if (CorewebStyle != null && i < CorewebStyle.Count)
					{
						value2 = CorewebStyle[i];
					}

					CorewebRestrStylesEntityAssociation.Add(new CorewebDefaultsCorewebRestrStyles( value0, value1, value2));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class CorewebDefaultsCorewebRestrStyles
	{
		public int? CorewebSeverityStartAssocMember;	
		public int? CorewebSeverityEndAssocMember;	
		public string CorewebStyleAssocMember;	
		public CorewebDefaultsCorewebRestrStyles() {}
		public CorewebDefaultsCorewebRestrStyles(
			int? inCorewebSeverityStart,
			int? inCorewebSeverityEnd,
			string inCorewebStyle)
		{
			CorewebSeverityStartAssocMember = inCorewebSeverityStart;
			CorewebSeverityEndAssocMember = inCorewebSeverityEnd;
			CorewebStyleAssocMember = inCorewebStyle;
		}
	}
}