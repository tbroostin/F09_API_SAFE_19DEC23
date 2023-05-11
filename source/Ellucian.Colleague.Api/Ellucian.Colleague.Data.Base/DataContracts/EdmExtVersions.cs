//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/3/2022 1:33:13 PM by user asainju2
//
//     Type: ENTITY
//     Entity: EDM.EXT.VERSIONS
//     Application: UT
//     Environment: dvetk
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
	[DataContract(Name = "EdmExtVersions")]
	[ColleagueDataContract(GeneratedDateTime = "3/3/2022 1:33:13 PM", User = "asainju2")]
	[EntityDataContract(EntityName = "EDM.EXT.VERSIONS", EntityType = "PHYS")]
	public class EdmExtVersions : IColleagueEntity
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
		/// CDD Name: EDMV.DESCRIPTION
		/// </summary>
		[DataMember(Order = 0, Name = "EDMV.DESCRIPTION")]
		public string EdmvDescription { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.RESOURCE.NAME
		/// </summary>
		[DataMember(Order = 1, Name = "EDMV.RESOURCE.NAME")]
		public string EdmvResourceName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.COLUMN.NAME
		/// </summary>
		[DataMember(Order = 2, Name = "EDMV.COLUMN.NAME")]
		public List<string> EdmvColumnName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.JSON.LABEL
		/// </summary>
		[DataMember(Order = 3, Name = "EDMV.JSON.LABEL")]
		public List<string> EdmvJsonLabel { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.VERSION.NUMBER
		/// </summary>
		[DataMember(Order = 4, Name = "EDMV.VERSION.NUMBER")]
		public string EdmvVersionNumber { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.JSON.PATH
		/// </summary>
		[DataMember(Order = 6, Name = "EDMV.JSON.PATH")]
		public List<string> EdmvJsonPath { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.JSON.PROPERTY.TYPE
		/// </summary>
		[DataMember(Order = 7, Name = "EDMV.JSON.PROPERTY.TYPE")]
		public List<string> EdmvJsonPropertyType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.EXTENDED.SCHEMA.TYPE
		/// </summary>
		[DataMember(Order = 8, Name = "EDMV.EXTENDED.SCHEMA.TYPE")]
		public string EdmvExtendedSchemaType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.FILE.NAME
		/// </summary>
		[DataMember(Order = 9, Name = "EDMV.FILE.NAME")]
		public List<string> EdmvFileName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.LENGTH
		/// </summary>
		[DataMember(Order = 10, Name = "EDMV.LENGTH")]
		public List<int?> EdmvLength { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.DATABASE.USAGE.TYPE
		/// </summary>
		[DataMember(Order = 11, Name = "EDMV.DATABASE.USAGE.TYPE")]
		public List<string> EdmvDatabaseUsageType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.DATE.TIME.LINK
		/// </summary>
		[DataMember(Order = 12, Name = "EDMV.DATE.TIME.LINK")]
		public List<string> EdmvDateTimeLink { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.CONVERSION
		/// </summary>
		[DataMember(Order = 13, Name = "EDMV.CONVERSION")]
		public List<string> EdmvConversion { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.INQUIRY.FIELDS
		/// </summary>
		[DataMember(Order = 19, Name = "EDMV.INQUIRY.FIELDS")]
		public List<string> EdmvInquiryFields { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.FIELD.NUMBER
		/// </summary>
		[DataMember(Order = 20, Name = "EDMV.FIELD.NUMBER")]
		public List<int?> EdmvFieldNumber { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.ASSOCIATION.CONTROLLER
		/// </summary>
		[DataMember(Order = 21, Name = "EDMV.ASSOCIATION.CONTROLLER")]
		public List<string> EdmvAssociationController { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.SECONDARY.FILE.NAME
		/// </summary>
		[DataMember(Order = 22, Name = "EDMV.SECONDARY.FILE.NAME")]
		public List<string> EdmvSecondaryFileName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.SEC.COLUMN.NAME
		/// </summary>
		[DataMember(Order = 23, Name = "EDMV.SEC.COLUMN.NAME")]
		public List<string> EdmvSecColumnName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.SEC.WHEN.COLUMN.NAME
		/// </summary>
		[DataMember(Order = 24, Name = "EDMV.SEC.WHEN.COLUMN.NAME")]
		public List<string> EdmvSecWhenColumnName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.SEC.WHEN.OPER
		/// </summary>
		[DataMember(Order = 25, Name = "EDMV.SEC.WHEN.OPER")]
		public List<string> EdmvSecWhenOper { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.SEC.WHEN.VALUE
		/// </summary>
		[DataMember(Order = 26, Name = "EDMV.SEC.WHEN.VALUE")]
		public List<string> EdmvSecWhenValue { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.SEC.WHEN.FILE.NAME
		/// </summary>
		[DataMember(Order = 28, Name = "EDMV.SEC.WHEN.FILE.NAME")]
		public List<string> EdmvSecWhenFileName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.TRANS.TYPE
		/// </summary>
		[DataMember(Order = 29, Name = "EDMV.TRANS.TYPE")]
		public List<string> EdmvTransType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.TRANS.ENUM.TABLE
		/// </summary>
		[DataMember(Order = 30, Name = "EDMV.TRANS.ENUM.TABLE")]
		public List<string> EdmvTransEnumTable { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.GUID.COLUMN.NAME
		/// </summary>
		[DataMember(Order = 31, Name = "EDMV.GUID.COLUMN.NAME")]
		public List<string> EdmvGuidColumnName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.GUID.FILE.NAME
		/// </summary>
		[DataMember(Order = 32, Name = "EDMV.GUID.FILE.NAME")]
		public List<string> EdmvGuidFileName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.TRANS.COLUMN.NAME
		/// </summary>
		[DataMember(Order = 33, Name = "EDMV.TRANS.COLUMN.NAME")]
		public List<string> EdmvTransColumnName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.TRANS.FILE.NAME
		/// </summary>
		[DataMember(Order = 34, Name = "EDMV.TRANS.FILE.NAME")]
		public List<string> EdmvTransFileName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.TRANS.TABLE.NAME
		/// </summary>
		[DataMember(Order = 35, Name = "EDMV.TRANS.TABLE.NAME")]
		public List<string> EdmvTransTableName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.SEC.DATABASE.USAGE.TYPE
		/// </summary>
		[DataMember(Order = 36, Name = "EDMV.SEC.DATABASE.USAGE.TYPE")]
		public List<string> EdmvSecDatabaseUsageType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.VERSION.STATUS
		/// </summary>
		[DataMember(Order = 37, Name = "EDMV.VERSION.STATUS")]
		public string EdmvVersionStatus { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.FILTER.CRITERIA
		/// </summary>
		[DataMember(Order = 40, Name = "EDMV.FILTER.CRITERIA")]
		public List<string> EdmvFilterCriteria { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.NAMED.QUERIES
		/// </summary>
		[DataMember(Order = 41, Name = "EDMV.NAMED.QUERIES")]
		public List<string> EdmvNamedQueries { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.COLUMN.DESC
		/// </summary>
		[DataMember(Order = 42, Name = "EDMV.COLUMN.DESC")]
		public List<string> EdmvColumnDesc { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.COLUMN.REQUIRED
		/// </summary>
		[DataMember(Order = 43, Name = "EDMV.COLUMN.REQUIRED")]
		public List<string> EdmvColumnRequired { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<EdmExtVersionsEdmvColumns> EdmvColumnsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: EDMV.COLUMNS
			
			EdmvColumnsEntityAssociation = new List<EdmExtVersionsEdmvColumns>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(EdmvColumnName != null)
			{
				int numEdmvColumns = EdmvColumnName.Count;
				if (EdmvJsonLabel !=null && EdmvJsonLabel.Count > numEdmvColumns) numEdmvColumns = EdmvJsonLabel.Count;
				if (EdmvJsonPath !=null && EdmvJsonPath.Count > numEdmvColumns) numEdmvColumns = EdmvJsonPath.Count;
				if (EdmvJsonPropertyType !=null && EdmvJsonPropertyType.Count > numEdmvColumns) numEdmvColumns = EdmvJsonPropertyType.Count;
				if (EdmvFileName !=null && EdmvFileName.Count > numEdmvColumns) numEdmvColumns = EdmvFileName.Count;
				if (EdmvLength !=null && EdmvLength.Count > numEdmvColumns) numEdmvColumns = EdmvLength.Count;
				if (EdmvDatabaseUsageType !=null && EdmvDatabaseUsageType.Count > numEdmvColumns) numEdmvColumns = EdmvDatabaseUsageType.Count;
				if (EdmvDateTimeLink !=null && EdmvDateTimeLink.Count > numEdmvColumns) numEdmvColumns = EdmvDateTimeLink.Count;
				if (EdmvConversion !=null && EdmvConversion.Count > numEdmvColumns) numEdmvColumns = EdmvConversion.Count;
				if (EdmvFieldNumber !=null && EdmvFieldNumber.Count > numEdmvColumns) numEdmvColumns = EdmvFieldNumber.Count;
				if (EdmvAssociationController !=null && EdmvAssociationController.Count > numEdmvColumns) numEdmvColumns = EdmvAssociationController.Count;
				if (EdmvSecondaryFileName !=null && EdmvSecondaryFileName.Count > numEdmvColumns) numEdmvColumns = EdmvSecondaryFileName.Count;
				if (EdmvSecColumnName !=null && EdmvSecColumnName.Count > numEdmvColumns) numEdmvColumns = EdmvSecColumnName.Count;
				if (EdmvSecWhenColumnName !=null && EdmvSecWhenColumnName.Count > numEdmvColumns) numEdmvColumns = EdmvSecWhenColumnName.Count;
				if (EdmvSecWhenOper !=null && EdmvSecWhenOper.Count > numEdmvColumns) numEdmvColumns = EdmvSecWhenOper.Count;
				if (EdmvSecWhenValue !=null && EdmvSecWhenValue.Count > numEdmvColumns) numEdmvColumns = EdmvSecWhenValue.Count;
				if (EdmvSecWhenFileName !=null && EdmvSecWhenFileName.Count > numEdmvColumns) numEdmvColumns = EdmvSecWhenFileName.Count;
				if (EdmvTransType !=null && EdmvTransType.Count > numEdmvColumns) numEdmvColumns = EdmvTransType.Count;
				if (EdmvTransEnumTable !=null && EdmvTransEnumTable.Count > numEdmvColumns) numEdmvColumns = EdmvTransEnumTable.Count;
				if (EdmvGuidColumnName !=null && EdmvGuidColumnName.Count > numEdmvColumns) numEdmvColumns = EdmvGuidColumnName.Count;
				if (EdmvGuidFileName !=null && EdmvGuidFileName.Count > numEdmvColumns) numEdmvColumns = EdmvGuidFileName.Count;
				if (EdmvTransColumnName !=null && EdmvTransColumnName.Count > numEdmvColumns) numEdmvColumns = EdmvTransColumnName.Count;
				if (EdmvTransFileName !=null && EdmvTransFileName.Count > numEdmvColumns) numEdmvColumns = EdmvTransFileName.Count;
				if (EdmvTransTableName !=null && EdmvTransTableName.Count > numEdmvColumns) numEdmvColumns = EdmvTransTableName.Count;
				if (EdmvSecDatabaseUsageType !=null && EdmvSecDatabaseUsageType.Count > numEdmvColumns) numEdmvColumns = EdmvSecDatabaseUsageType.Count;
				if (EdmvFilterCriteria !=null && EdmvFilterCriteria.Count > numEdmvColumns) numEdmvColumns = EdmvFilterCriteria.Count;
				if (EdmvColumnDesc !=null && EdmvColumnDesc.Count > numEdmvColumns) numEdmvColumns = EdmvColumnDesc.Count;
				if (EdmvColumnRequired !=null && EdmvColumnRequired.Count > numEdmvColumns) numEdmvColumns = EdmvColumnRequired.Count;

				for (int i = 0; i < numEdmvColumns; i++)
				{

					string value0 = "";
					if (EdmvColumnName != null && i < EdmvColumnName.Count)
					{
						value0 = EdmvColumnName[i];
					}


					string value1 = "";
					if (EdmvJsonLabel != null && i < EdmvJsonLabel.Count)
					{
						value1 = EdmvJsonLabel[i];
					}


					string value2 = "";
					if (EdmvJsonPath != null && i < EdmvJsonPath.Count)
					{
						value2 = EdmvJsonPath[i];
					}


					string value3 = "";
					if (EdmvJsonPropertyType != null && i < EdmvJsonPropertyType.Count)
					{
						value3 = EdmvJsonPropertyType[i];
					}


					string value4 = "";
					if (EdmvFileName != null && i < EdmvFileName.Count)
					{
						value4 = EdmvFileName[i];
					}


					int? value5 = null;
					if (EdmvLength != null && i < EdmvLength.Count)
					{
						value5 = EdmvLength[i];
					}


					string value6 = "";
					if (EdmvDatabaseUsageType != null && i < EdmvDatabaseUsageType.Count)
					{
						value6 = EdmvDatabaseUsageType[i];
					}


					string value7 = "";
					if (EdmvDateTimeLink != null && i < EdmvDateTimeLink.Count)
					{
						value7 = EdmvDateTimeLink[i];
					}


					string value8 = "";
					if (EdmvConversion != null && i < EdmvConversion.Count)
					{
						value8 = EdmvConversion[i];
					}


					int? value9 = null;
					if (EdmvFieldNumber != null && i < EdmvFieldNumber.Count)
					{
						value9 = EdmvFieldNumber[i];
					}


					string value10 = "";
					if (EdmvAssociationController != null && i < EdmvAssociationController.Count)
					{
						value10 = EdmvAssociationController[i];
					}


					string value11 = "";
					if (EdmvSecondaryFileName != null && i < EdmvSecondaryFileName.Count)
					{
						value11 = EdmvSecondaryFileName[i];
					}


					string value12 = "";
					if (EdmvSecColumnName != null && i < EdmvSecColumnName.Count)
					{
						value12 = EdmvSecColumnName[i];
					}


					string value13 = "";
					if (EdmvSecWhenColumnName != null && i < EdmvSecWhenColumnName.Count)
					{
						value13 = EdmvSecWhenColumnName[i];
					}


					string value14 = "";
					if (EdmvSecWhenOper != null && i < EdmvSecWhenOper.Count)
					{
						value14 = EdmvSecWhenOper[i];
					}


					string value15 = "";
					if (EdmvSecWhenValue != null && i < EdmvSecWhenValue.Count)
					{
						value15 = EdmvSecWhenValue[i];
					}


					string value16 = "";
					if (EdmvSecWhenFileName != null && i < EdmvSecWhenFileName.Count)
					{
						value16 = EdmvSecWhenFileName[i];
					}


					string value17 = "";
					if (EdmvTransType != null && i < EdmvTransType.Count)
					{
						value17 = EdmvTransType[i];
					}


					string value18 = "";
					if (EdmvTransEnumTable != null && i < EdmvTransEnumTable.Count)
					{
						value18 = EdmvTransEnumTable[i];
					}


					string value19 = "";
					if (EdmvGuidColumnName != null && i < EdmvGuidColumnName.Count)
					{
						value19 = EdmvGuidColumnName[i];
					}


					string value20 = "";
					if (EdmvGuidFileName != null && i < EdmvGuidFileName.Count)
					{
						value20 = EdmvGuidFileName[i];
					}


					string value21 = "";
					if (EdmvTransColumnName != null && i < EdmvTransColumnName.Count)
					{
						value21 = EdmvTransColumnName[i];
					}


					string value22 = "";
					if (EdmvTransFileName != null && i < EdmvTransFileName.Count)
					{
						value22 = EdmvTransFileName[i];
					}


					string value23 = "";
					if (EdmvTransTableName != null && i < EdmvTransTableName.Count)
					{
						value23 = EdmvTransTableName[i];
					}


					string value24 = "";
					if (EdmvSecDatabaseUsageType != null && i < EdmvSecDatabaseUsageType.Count)
					{
						value24 = EdmvSecDatabaseUsageType[i];
					}


					string value25 = "";
					if (EdmvFilterCriteria != null && i < EdmvFilterCriteria.Count)
					{
						value25 = EdmvFilterCriteria[i];
					}


					string value26 = "";
					if (EdmvColumnDesc != null && i < EdmvColumnDesc.Count)
					{
						value26 = EdmvColumnDesc[i];
					}


					string value27 = "";
					if (EdmvColumnRequired != null && i < EdmvColumnRequired.Count)
					{
						value27 = EdmvColumnRequired[i];
					}

					EdmvColumnsEntityAssociation.Add(new EdmExtVersionsEdmvColumns( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15, value16, value17, value18, value19, value20, value21, value22, value23, value24, value25, value26, value27));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class EdmExtVersionsEdmvColumns
	{
		public string EdmvColumnNameAssocMember;	
		public string EdmvJsonLabelAssocMember;	
		public string EdmvJsonPathAssocMember;	
		public string EdmvJsonPropertyTypeAssocMember;	
		public string EdmvFileNameAssocMember;	
		public int? EdmvLengthAssocMember;	
		public string EdmvDatabaseUsageTypeAssocMember;	
		public string EdmvDateTimeLinkAssocMember;	
		public string EdmvConversionAssocMember;	
		public int? EdmvFieldNumberAssocMember;	
		public string EdmvAssociationControllerAssocMember;	
		public string EdmvSecondaryFileNameAssocMember;	
		public string EdmvSecColumnNameAssocMember;	
		public string EdmvSecWhenColumnNameAssocMember;	
		public string EdmvSecWhenOperAssocMember;	
		public string EdmvSecWhenValueAssocMember;	
		public string EdmvSecWhenFileNameAssocMember;	
		public string EdmvTransTypeAssocMember;	
		public string EdmvTransEnumTableAssocMember;	
		public string EdmvGuidColumnNameAssocMember;	
		public string EdmvGuidFileNameAssocMember;	
		public string EdmvTransColumnNameAssocMember;	
		public string EdmvTransFileNameAssocMember;	
		public string EdmvTransTableNameAssocMember;	
		public string EdmvSecDatabaseUsageTypeAssocMember;	
		public string EdmvFilterCriteriaAssocMember;	
		public string EdmvColumnDescAssocMember;	
		public string EdmvColumnRequiredAssocMember;	
		public EdmExtVersionsEdmvColumns() {}
		public EdmExtVersionsEdmvColumns(
			string inEdmvColumnName,
			string inEdmvJsonLabel,
			string inEdmvJsonPath,
			string inEdmvJsonPropertyType,
			string inEdmvFileName,
			int? inEdmvLength,
			string inEdmvDatabaseUsageType,
			string inEdmvDateTimeLink,
			string inEdmvConversion,
			int? inEdmvFieldNumber,
			string inEdmvAssociationController,
			string inEdmvSecondaryFileName,
			string inEdmvSecColumnName,
			string inEdmvSecWhenColumnName,
			string inEdmvSecWhenOper,
			string inEdmvSecWhenValue,
			string inEdmvSecWhenFileName,
			string inEdmvTransType,
			string inEdmvTransEnumTable,
			string inEdmvGuidColumnName,
			string inEdmvGuidFileName,
			string inEdmvTransColumnName,
			string inEdmvTransFileName,
			string inEdmvTransTableName,
			string inEdmvSecDatabaseUsageType,
			string inEdmvFilterCriteria,
			string inEdmvColumnDesc,
			string inEdmvColumnRequired)
		{
			EdmvColumnNameAssocMember = inEdmvColumnName;
			EdmvJsonLabelAssocMember = inEdmvJsonLabel;
			EdmvJsonPathAssocMember = inEdmvJsonPath;
			EdmvJsonPropertyTypeAssocMember = inEdmvJsonPropertyType;
			EdmvFileNameAssocMember = inEdmvFileName;
			EdmvLengthAssocMember = inEdmvLength;
			EdmvDatabaseUsageTypeAssocMember = inEdmvDatabaseUsageType;
			EdmvDateTimeLinkAssocMember = inEdmvDateTimeLink;
			EdmvConversionAssocMember = inEdmvConversion;
			EdmvFieldNumberAssocMember = inEdmvFieldNumber;
			EdmvAssociationControllerAssocMember = inEdmvAssociationController;
			EdmvSecondaryFileNameAssocMember = inEdmvSecondaryFileName;
			EdmvSecColumnNameAssocMember = inEdmvSecColumnName;
			EdmvSecWhenColumnNameAssocMember = inEdmvSecWhenColumnName;
			EdmvSecWhenOperAssocMember = inEdmvSecWhenOper;
			EdmvSecWhenValueAssocMember = inEdmvSecWhenValue;
			EdmvSecWhenFileNameAssocMember = inEdmvSecWhenFileName;
			EdmvTransTypeAssocMember = inEdmvTransType;
			EdmvTransEnumTableAssocMember = inEdmvTransEnumTable;
			EdmvGuidColumnNameAssocMember = inEdmvGuidColumnName;
			EdmvGuidFileNameAssocMember = inEdmvGuidFileName;
			EdmvTransColumnNameAssocMember = inEdmvTransColumnName;
			EdmvTransFileNameAssocMember = inEdmvTransFileName;
			EdmvTransTableNameAssocMember = inEdmvTransTableName;
			EdmvSecDatabaseUsageTypeAssocMember = inEdmvSecDatabaseUsageType;
			EdmvFilterCriteriaAssocMember = inEdmvFilterCriteria;
			EdmvColumnDescAssocMember = inEdmvColumnDesc;
			EdmvColumnRequiredAssocMember = inEdmvColumnRequired;
		}
	}
}