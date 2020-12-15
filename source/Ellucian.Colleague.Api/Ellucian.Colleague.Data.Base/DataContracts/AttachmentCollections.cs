//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/25/2019 3:39:31 PM by user sxs
//
//     Type: ENTITY
//     Entity: ATTACHMENT.COLLECTIONS
//     Application: UT
//     Environment: dvetk_wstst01
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
	[DataContract(Name = "AttachmentCollections")]
	[ColleagueDataContract(GeneratedDateTime = "4/25/2019 3:39:31 PM", User = "sxs")]
	[EntityDataContract(EntityName = "ATTACHMENT.COLLECTIONS", EntityType = "PHYS")]
	public class AttachmentCollections : IColleagueEntity
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
		/// CDD Name: ATCOL.NAME
		/// </summary>
		[DataMember(Order = 0, Name = "ATCOL.NAME")]
		public string AtcolName { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.DESCRIPTION
		/// </summary>
		[DataMember(Order = 1, Name = "ATCOL.DESCRIPTION")]
		public string AtcolDescription { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.OWNER
		/// </summary>
		[DataMember(Order = 2, Name = "ATCOL.OWNER")]
		public string AtcolOwner { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.STATUS
		/// </summary>
		[DataMember(Order = 3, Name = "ATCOL.STATUS")]
		public string AtcolStatus { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.OWNER.ACTIONS
		/// </summary>
		[DataMember(Order = 4, Name = "ATCOL.OWNER.ACTIONS")]
		public List<string> AtcolOwnerActions { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.RETAIN.DURATION
		/// </summary>
		[DataMember(Order = 5, Name = "ATCOL.RETAIN.DURATION")]
		public string AtcolRetainDuration { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.ALLOWED.CONTENT.TYPES
		/// </summary>
		[DataMember(Order = 6, Name = "ATCOL.ALLOWED.CONTENT.TYPES")]
		public List<string> AtcolAllowedContentTypes { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.MAX.ATTACHMENT.SIZE
		/// </summary>
		[DataMember(Order = 7, Name = "ATCOL.MAX.ATTACHMENT.SIZE")]
		public long? AtcolMaxAttachmentSize { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.USERS.ID
		/// </summary>
		[DataMember(Order = 8, Name = "ATCOL.USERS.ID")]
		public List<string> AtcolUsersId { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.USERS.ACTIONS
		/// </summary>
		[DataMember(Order = 9, Name = "ATCOL.USERS.ACTIONS")]
		public List<string> AtcolUsersActions { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.ROLES.ID
		/// </summary>
		[DataMember(Order = 10, Name = "ATCOL.ROLES.ID")]
		public List<string> AtcolRolesId { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.ROLES.ACTIONS
		/// </summary>
		[DataMember(Order = 11, Name = "ATCOL.ROLES.ACTIONS")]
		public List<string> AtcolRolesActions { get; set; }
		
		/// <summary>
		/// CDD Name: ATCOL.ENCR.KEY.ID
		/// </summary>
		[DataMember(Order = 12, Name = "ATCOL.ENCR.KEY.ID")]
		public string AtcolEncrKeyId { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<AttachmentCollectionsAtcolUsers> AtcolUsersEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<AttachmentCollectionsAtcolRoles> AtcolRolesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: ATCOL.USERS
			
			AtcolUsersEntityAssociation = new List<AttachmentCollectionsAtcolUsers>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(AtcolUsersId != null)
			{
				int numAtcolUsers = AtcolUsersId.Count;
				if (AtcolUsersActions !=null && AtcolUsersActions.Count > numAtcolUsers) numAtcolUsers = AtcolUsersActions.Count;

				for (int i = 0; i < numAtcolUsers; i++)
				{

					string value0 = "";
					if (AtcolUsersId != null && i < AtcolUsersId.Count)
					{
						value0 = AtcolUsersId[i];
					}


					string value1 = "";
					if (AtcolUsersActions != null && i < AtcolUsersActions.Count)
					{
						value1 = AtcolUsersActions[i];
					}

					AtcolUsersEntityAssociation.Add(new AttachmentCollectionsAtcolUsers( value0, value1));
				}
			}
			// EntityAssociation Name: ATCOL.ROLES
			
			AtcolRolesEntityAssociation = new List<AttachmentCollectionsAtcolRoles>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(AtcolRolesId != null)
			{
				int numAtcolRoles = AtcolRolesId.Count;
				if (AtcolRolesActions !=null && AtcolRolesActions.Count > numAtcolRoles) numAtcolRoles = AtcolRolesActions.Count;

				for (int i = 0; i < numAtcolRoles; i++)
				{

					string value0 = "";
					if (AtcolRolesId != null && i < AtcolRolesId.Count)
					{
						value0 = AtcolRolesId[i];
					}


					string value1 = "";
					if (AtcolRolesActions != null && i < AtcolRolesActions.Count)
					{
						value1 = AtcolRolesActions[i];
					}

					AtcolRolesEntityAssociation.Add(new AttachmentCollectionsAtcolRoles( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class AttachmentCollectionsAtcolUsers
	{
		public string AtcolUsersIdAssocMember;	
		public string AtcolUsersActionsAssocMember;	
		public AttachmentCollectionsAtcolUsers() {}
		public AttachmentCollectionsAtcolUsers(
			string inAtcolUsersId,
			string inAtcolUsersActions)
		{
			AtcolUsersIdAssocMember = inAtcolUsersId;
			AtcolUsersActionsAssocMember = inAtcolUsersActions;
		}
	}
	
	[Serializable]
	public class AttachmentCollectionsAtcolRoles
	{
		public string AtcolRolesIdAssocMember;	
		public string AtcolRolesActionsAssocMember;	
		public AttachmentCollectionsAtcolRoles() {}
		public AttachmentCollectionsAtcolRoles(
			string inAtcolRolesId,
			string inAtcolRolesActions)
		{
			AtcolRolesIdAssocMember = inAtcolRolesId;
			AtcolRolesActionsAssocMember = inAtcolRolesActions;
		}
	}
}