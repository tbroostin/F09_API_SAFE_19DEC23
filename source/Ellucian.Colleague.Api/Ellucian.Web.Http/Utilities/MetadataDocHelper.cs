// Copyright 2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Ellucian.Web.Http.Utilities
{
	/// <summary>Constains static analysis methods of the code (reflection).</summary>
	public static class MetadataDocHelper
	{
		#region GetXmlName

		/// <summary>Gets the XML name of an <see cref="Type"/> as it appears in the XML docs.</summary>
		/// <param name="type">The field to get the XML name of.</param>
		/// <returns>The XML name of <paramref name="type"/> as it appears in the XML docs.</returns>
		public static string GetXmlName(this Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			LoadXmlDocumentation(type.Assembly);
			return "T:" + GetXmlNameTypeSegment(type.FullName);
		}

		/// <summary>Gets the XML name of an <see cref="MethodInfo"/> as it appears in the XML docs.</summary>
		/// <param name="methodInfo">The field to get the XML name of.</param>
		/// <returns>The XML name of <paramref name="methodInfo"/> as it appears in the XML docs.</returns>
		public static string GetXmlName(this MethodInfo methodInfo)
		{
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
			return GetXmlNameMethodBase(methodInfo: methodInfo);
		}

		/// <summary>Gets the XML name of an <see cref="ConstructorInfo"/> as it appears in the XML docs.</summary>
		/// <param name="constructorInfo">The field to get the XML name of.</param>
		/// <returns>The XML name of <paramref name="constructorInfo"/> as it appears in the XML docs.</returns>
		public static string GetXmlName(this ConstructorInfo constructorInfo)
		{
			if (constructorInfo == null) throw new ArgumentNullException(nameof(constructorInfo));
			return GetXmlNameMethodBase(constructorInfo: constructorInfo);
		}

		/// <summary>Gets the XML name of an <see cref="PropertyInfo"/> as it appears in the XML docs.</summary>
		/// <param name="propertyInfo">The field to get the XML name of.</param>
		/// <returns>The XML name of <paramref name="propertyInfo"/> as it appears in the XML docs.</returns>
		public static string GetXmlName(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
			return "P:" + GetXmlNameTypeSegment(propertyInfo.DeclaringType.FullName) + "." + propertyInfo.Name;
		}

		/// <summary>Gets the XML name of an <see cref="FieldInfo"/> as it appears in the XML docs.</summary>
		/// <param name="fieldInfo">The field to get the XML name of.</param>
		/// <returns>The XML name of <paramref name="fieldInfo"/> as it appears in the XML docs.</returns>
		public static string GetXmlName(this FieldInfo fieldInfo)
		{
			if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
			return "F:" + GetXmlNameTypeSegment(fieldInfo.DeclaringType.FullName) + "." + fieldInfo.Name;
		}

		/// <summary>Gets the XML name of an <see cref="EventInfo"/> as it appears in the XML docs.</summary>
		/// <param name="eventInfo">The event to get the XML name of.</param>
		/// <returns>The XML name of <paramref name="eventInfo"/> as it appears in the XML docs.</returns>
		public static string GetXmlName(this EventInfo eventInfo)
		{
			if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));
			return "E:" + GetXmlNameTypeSegment(eventInfo.DeclaringType.FullName) + "." + eventInfo.Name;
		}

		internal static string GetXmlNameMethodBase(MethodInfo methodInfo = null, ConstructorInfo constructorInfo = null)
		{
			if (methodInfo != null && constructorInfo != null)
			{
				throw new ArgumentException($"{nameof(GetDocumentation)} {nameof(methodInfo)} != null && {nameof(constructorInfo)} != null");
			}

			if (methodInfo != null)
			{
				if (methodInfo.DeclaringType == null)
				{
					throw new ArgumentException($"{nameof(methodInfo)}.{nameof(Type.DeclaringType)} == null");
				}
				else if (methodInfo.DeclaringType.IsGenericType)
				{
					var methodInfos = methodInfo.DeclaringType.GetGenericTypeDefinition().GetMethods(
						BindingFlags.Static |
						BindingFlags.Public |
						BindingFlags.Instance |
						BindingFlags.NonPublic);
					
					foreach (var mi in methodInfos)
						if (mi.MetadataToken == methodInfo.MetadataToken) methodInfo = mi;
				}
			}

			MethodBase methodBase = methodInfo ?? (MethodBase)constructorInfo;
			//if (sourceof(methodBase == null, out string c1)) throw new TowelBugException(c1);
			//if (sourceof(methodBase!.DeclaringType == null, out string c2)) throw new ArgumentException(c2);

			LoadXmlDocumentation(methodBase.DeclaringType.Assembly);

			Dictionary<string, int> typeGenericMap = new Dictionary<string, int>();
			Type[] typeGenericArguments = methodBase.DeclaringType.GetGenericArguments();
			for (int i = 0; i < typeGenericArguments.Length; i++)
			{
				Type typeGeneric = typeGenericArguments[i];
				typeGenericMap[typeGeneric.Name] = i;
			}

			Dictionary<string, int> methodGenericMap = new Dictionary<string, int>();
			if (constructorInfo == null)
			{
				Type[] methodGenericArguments = methodBase.GetGenericArguments();
				for (int i = 0; i < methodGenericArguments.Length; i++)
				{
					Type methodGeneric = methodGenericArguments[i];
					methodGenericMap[methodGeneric.Name] = i;
				}
			}

			ParameterInfo[] parameterInfos = methodBase.GetParameters();

			string memberTypePrefix = "M:";
			string declarationTypeString = GetXmlDocumenationFormattedString(methodBase.DeclaringType, false, typeGenericMap, methodGenericMap);
			string memberNameString =
				constructorInfo != null ? "#ctor" :
				methodBase.Name;
			string methodGenericArgumentsString =
				methodGenericMap.Count > 0 ?
				"``" + methodGenericMap.Count :
				string.Empty;
			string parametersString =
				parameterInfos.Length > 0 ?
				"(" + string.Join(",", methodBase.GetParameters().Select(x => GetXmlDocumenationFormattedString(x.ParameterType, true, typeGenericMap, methodGenericMap))) + ")" :
				string.Empty;

			string key =
				memberTypePrefix +
				declarationTypeString +
				"." +
				memberNameString +
				methodGenericArgumentsString +
				parametersString;

			if (methodInfo != null &&
				(methodBase.Name is "op_Implicit" ||
				methodBase.Name is "op_Explicit"))
			{
				key += "~" + GetXmlDocumenationFormattedString(methodInfo.ReturnType, true, typeGenericMap, methodGenericMap);
			}
			return key;
		}

		internal static string GetXmlDocumenationFormattedString(
			Type type,
			bool isMethodParameter,
			Dictionary<string, int> typeGenericMap,
			Dictionary<string, int> methodGenericMap)
		{
			if (type.IsGenericParameter)
			{
				int methodIndex;
				bool success = false;
				success = methodGenericMap.TryGetValue(type.Name, out methodIndex);
				return success
					? "``" + methodIndex
					: "`" + typeGenericMap[type.Name];
			}
			else if (type.HasElementType)
			{
				string elementTypeString = GetXmlDocumenationFormattedString(
					type.GetElementType() ?? throw new ArgumentException($"{nameof(type)}.{nameof(Type.HasElementType)} && {nameof(type)}.{nameof(Type.GetElementType)}() == null", nameof(type)),
					isMethodParameter,
					typeGenericMap,
					methodGenericMap);

				if (type.IsPointer)
				{
					return elementTypeString + "*";
				}

				else if (type.IsByRef)
				{ 
					return elementTypeString + "@";
				}

				else if (type.IsArray)
				{
					int rank = type.GetArrayRank();
					string arrayDimensionsString = rank > 1
						? "[" + string.Join(",", Enumerable.Repeat("0:", rank)) + "]"
						: "[]";
					return elementTypeString + arrayDimensionsString;
				}
				else
                {
					throw new ArgumentException();
                }
			}
			else
			{
				string prefaceString = type.IsNested
					? GetXmlDocumenationFormattedString(
						type.DeclaringType ?? throw new ArgumentException($"{nameof(type)}.{nameof(Type.IsNested)} && {nameof(type)}.{nameof(Type.DeclaringType)} == null", nameof(type)),
						isMethodParameter,
						typeGenericMap,
						methodGenericMap) + "."
					: type.Namespace + ".";

				string typeNameString = isMethodParameter
					? typeNameString = Regex.Replace(type.Name, @"`\d+", string.Empty)
					: typeNameString = type.Name;

				string genericArgumentsString = type.IsGenericType && isMethodParameter
					? "{" + string.Join(",",
						type.GetGenericArguments().Select(argument =>
							GetXmlDocumenationFormattedString(
								argument,
								isMethodParameter,
								typeGenericMap,
								methodGenericMap))
						) + "}"
					: string.Empty;

				return prefaceString + typeNameString + genericArgumentsString;
			}
		}

		internal static string GetXmlNameTypeSegment(string typeFullNameString) =>
			Regex.Replace(typeFullNameString, @"\[.*\]", string.Empty).Replace('+', '.');

		#endregion

		#region GetXmlDocumentation

		internal static object xmlCacheLock = new object();
		internal static ISet<Assembly> loadedAssemblies = new HashSet<Assembly>();
		internal static Dictionary<string, string> loadedXmlDocumentation = new Dictionary<string, string>();

		internal static bool LoadXmlDocumentation(Assembly assembly)
		{
			if (loadedAssemblies.Contains(assembly))
			{
				return false;
			}
			bool newContent = false;
			string directoryPath = assembly.GetDirectoryPath();
			if (!string.IsNullOrEmpty(directoryPath))
			{
				string xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
				if (File.Exists(xmlFilePath))
				{
					StreamReader streamReader = new StreamReader(xmlFilePath);
					LoadXmlDocumentationNoLock(streamReader);
					newContent = true;
				}
			}
			loadedAssemblies.Add(assembly);
			return newContent;
		}

		public static string GetDirectoryPath(this Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}

		/// <summary>Loads the XML code documentation into memory so it can be accessed by extension methods on reflection types.</summary>
		/// <param name="xmlDocumentation">The content of the XML code documentation.</param>
		public static void LoadXmlDocumentation(string xmlDocumentation)
		{
			StringReader stringReader = new StringReader(xmlDocumentation);
			LoadXmlDocumentation(stringReader);
		}

		/// <summary>Loads the XML code documentation into memory so it can be accessed by extension methods on reflection types.</summary>
		/// <param name="textReader">The text reader to process in an XmlReader.</param>
		public static void LoadXmlDocumentation(TextReader textReader)
		{
			lock (xmlCacheLock)
			{
				LoadXmlDocumentationNoLock(textReader);
			}
		}

		internal static void LoadXmlDocumentationNoLock(TextReader textReader)
		{
			XmlReader xmlReader = XmlReader.Create(textReader);
			while (xmlReader.Read())
			{
				if (xmlReader.NodeType is XmlNodeType.Element && xmlReader.Name is "member")
				{
					string rawName = xmlReader["name"];
					if (!string.IsNullOrWhiteSpace(rawName))
					{
						loadedXmlDocumentation[rawName] = xmlReader.ReadInnerXml();
					}
				}
			}
		}

		/// <summary>Clears the currently loaded XML documentation.</summary>
		public static void ClearXmlDocumentation()
		{
			lock (xmlCacheLock)
			{
				loadedAssemblies.Clear();
				loadedXmlDocumentation.Clear();
			}
		}

		internal static string GetDocumentation(string key, Assembly assembly)
		{
			lock (xmlCacheLock)
			{
				string value = "";
				bool success = false;
				success = loadedXmlDocumentation.TryGetValue(key, out value);
				if (!success)
				{
					LoadXmlDocumentation(assembly); 
					success = loadedXmlDocumentation.TryGetValue(key, out value);
				}

				if (success && !string.IsNullOrEmpty(value))
                {
					value = "<xmlConverter>" + value + "</xmlConverter>";
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(value);

					string json = JsonConvert.SerializeXmlNode(doc);
					object convertedValue = JsonConvert.DeserializeObject(json);
					foreach (JToken property in ((JObject)convertedValue).Children())
					{
						var name = ((JProperty)property).Name;
						var val = ((JProperty)property).Value;
						if (name == "xmlConverter")
						{
							foreach (JToken prop in ((JObject)val).Children())
							{
								var fldName = ((JProperty)prop).Name;
								var fldVal = ((JProperty)prop).Value;
								if (fldName == "summary")
								{
									string newValue = string.Empty;
									var allValues = fldVal.ToString().Split('\n');
									foreach (var tempValue in allValues)
                                    {
										if (!string.IsNullOrWhiteSpace(tempValue))
											newValue = string.Concat(newValue, " ", tempValue.Trim());
                                    }
									value = newValue.Replace("\n", "").Replace("\r", "").Trim();
								}
							}
						}
					}
				}
				return value;
			}
		}

		/// <summary>Gets the XML documentation on a type.</summary>
		/// <param name="type">The type to get the XML documentation of.</param>
		/// <returns>MetadataAttribute with documentation properties.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static MetadataAttribute GetDocumentation(this Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			var metadataAttribute = type.GetMetadataAttribute();
				
			if (string.IsNullOrEmpty(metadataAttribute.ApiDescription))
			{
				var typeDescription = GetDocumentation(type.GetXmlName(), type.Assembly);
				metadataAttribute.ApiDescription = typeDescription;
			}

			return metadataAttribute;
		}

		/// <summary>Gets the XML documentation on a method.</summary>
		/// <param name="methodInfo">The method to get the XML documentation of.</param>
		/// <returns>MetadataAttribute with documentation properties.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static MetadataAttribute GetDocumentation(this MethodInfo methodInfo)
		{
			if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
			var metadataAttribute = methodInfo.GetMetadataAttribute();

			if (string.IsNullOrEmpty(metadataAttribute.HttpMethodDescription) || string.IsNullOrEmpty(metadataAttribute.HttpMethodSummary))
			{
				var typeDescription = GetDocumentation(methodInfo.GetXmlName(), methodInfo.DeclaringType.Assembly);
				if (string.IsNullOrEmpty(metadataAttribute.HttpMethodDescription)) metadataAttribute.HttpMethodDescription = typeDescription;
				if (string.IsNullOrEmpty(metadataAttribute.HttpMethodSummary)) metadataAttribute.HttpMethodSummary = typeDescription;
			}

			return metadataAttribute;
		}

		/// <summary>Gets the XML documentation on a constructor.</summary>
		/// <param name="constructorInfo">The constructor to get the XML documentation of.</param>
		/// <returns>MetadataAttribute with documentation properties.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static MetadataAttribute GetDocumentation(this ConstructorInfo constructorInfo)
		{
			if (constructorInfo == null) throw new ArgumentNullException(nameof(constructorInfo));
			var metadataAttribute = constructorInfo.GetMetadataAttribute();

			if (string.IsNullOrEmpty(metadataAttribute.DataDescription))
			{
				var typeDescription = GetDocumentation(constructorInfo.GetXmlName(), constructorInfo.DeclaringType.Assembly);
				metadataAttribute.DataDescription = typeDescription;
			}

			return metadataAttribute;
		}

		/// <summary>Gets the XML documentation on a property.</summary>
		/// <param name="propertyInfo">The property to get the XML documentation of.</param>
		/// <returns>MetadataAttribute with documentation properties.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static MetadataAttribute GetDocumentation(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
			var metadataAttribute = propertyInfo.GetMetadataAttribute();

			if (string.IsNullOrEmpty(metadataAttribute.DataDescription))
			{
				var typeDescription = GetDocumentation(propertyInfo.GetXmlName(), propertyInfo.DeclaringType.Assembly);
				metadataAttribute.DataDescription = typeDescription;
			}

			return metadataAttribute;
		}

		/// <summary>Gets the XML documentation on a field.</summary>
		/// <param name="fieldInfo">The field to get the XML documentation of.</param>
		/// <returns>MetadataAttribute with documentation properties.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static MetadataAttribute GetDocumentation(this FieldInfo fieldInfo)
		{
			if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
			var metadataAttribute = fieldInfo.GetMetadataAttribute();

			if (string.IsNullOrEmpty(metadataAttribute.DataDescription))
			{
				var typeDescription = GetDocumentation(fieldInfo.GetXmlName(), fieldInfo.DeclaringType.Assembly);
				metadataAttribute.DataDescription = typeDescription;
			}

			return metadataAttribute;
		}

		/// <summary>Gets the XML documentation on an event.</summary>
		/// <param name="eventInfo">The event to get the XML documentation of.</param>
		/// <returns>MetadataAttribute with documentation properties.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static MetadataAttribute GetDocumentation(this EventInfo eventInfo)
		{
			if (eventInfo == null) throw new ArgumentNullException(nameof(eventInfo));
			var metadataAttribute = eventInfo.GetMetadataAttribute();

			if (string.IsNullOrEmpty(metadataAttribute.DataDescription))
			{
				var typeDescription = GetDocumentation(eventInfo.GetXmlName(), eventInfo.DeclaringType.Assembly);
				metadataAttribute.DataDescription = typeDescription;
			}

			return metadataAttribute;
		}

		/// <summary>Gets the XML documentation on a member.</summary>
		/// <param name="memberInfo">The member to get the XML documentation of.</param>
		/// <returns>MetadataAttribute with documentation properties.</returns>
		/// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
		public static MetadataAttribute GetDocumentation(this MemberInfo memberInfo)
		{
			switch (memberInfo)
			{
				case FieldInfo fieldInfo:
					return fieldInfo.GetDocumentation();
				case PropertyInfo propertyInfo:
					return propertyInfo.GetDocumentation();
				case EventInfo eventInfo:
					return eventInfo.GetDocumentation();
				case ConstructorInfo constructorInfo:
					return constructorInfo.GetDocumentation();
				case MethodInfo methodInfo:
					return methodInfo.GetDocumentation();
				case Type type:
					return type.GetDocumentation();
				case null:
					throw new ArgumentNullException(nameof(memberInfo));
				default:
					throw new NotImplementedException($"{nameof(GetDocumentation)} encountered an unhandled {nameof(MemberInfo)} type: {memberInfo}");
			}
		}

		/// <summary>Gets the XML documentation for a parameter.</summary>
		/// <param name="parameterInfo">The parameter to get the XML documentation for.</param>
		/// <returns>The XML documenation of the parameter.</returns>
		public static string GetDocumentation(this ParameterInfo parameterInfo)
		{
			if (parameterInfo == null) throw new ArgumentNullException(nameof(parameterInfo));
			MetadataAttribute metadataAttribute = parameterInfo.Member.GetDocumentation();
			string memberDocumentation = string.Empty;
			if (!string.IsNullOrEmpty(metadataAttribute.ApiDescription)) memberDocumentation = metadataAttribute.ApiDescription;
			if (!string.IsNullOrEmpty(metadataAttribute.DataDescription)) memberDocumentation = metadataAttribute.DataDescription;
			if (memberDocumentation != null)
			{
				string regexPattern =
					Regex.Escape($@"<param name=""{parameterInfo.Name}"">") +
					".*?" +
					Regex.Escape($@"</param>");

				Match match = Regex.Match(memberDocumentation, regexPattern);
				if (match.Success)
				{
					return match.Value;
				}
			}
			return null;
		}

		#endregion

		#region MetadataAttribute

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="type">Type of object to retrieve MetaData for.</param>
		/// <returns>SchemasAttribute</returns>
		private static MetadataAttribute GetMetadataAttribute(this Type type)
		{
			if (type != null)
			{
				var dataMemberAttributes = (MetadataAttribute[])type.GetCustomAttributes(typeof(MetadataAttribute), false);
				if (dataMemberAttributes != null && dataMemberAttributes.Any())
				{
					return MergeMetadataAttributes(dataMemberAttributes);
				}
			}

			return new MetadataAttribute();
		}

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="methodInfo">Type of object to retrieve MetaData for.</param>
		/// <returns>SchemasAttribute</returns>
		private static MetadataAttribute GetMetadataAttribute(this MethodInfo methodInfo)
		{
			if (methodInfo != null)
			{
				var dataMemberAttributes = (MetadataAttribute[])methodInfo.GetCustomAttributes(typeof(MetadataAttribute), false);
				if (dataMemberAttributes != null && dataMemberAttributes.Any())
				{
					return MergeMetadataAttributes(dataMemberAttributes);
				}
			}

			return new MetadataAttribute();
		}

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="constructorInfo">Type of object to retrieve MetaData for.</param>
		/// <returns>SchemasAttribute</returns>
		private static MetadataAttribute GetMetadataAttribute(this ConstructorInfo constructorInfo)
		{
			if (constructorInfo != null)
			{
				var dataMemberAttributes = (MetadataAttribute[])constructorInfo.GetCustomAttributes(typeof(MetadataAttribute), false);
				if (dataMemberAttributes != null && dataMemberAttributes.Any())
				{
					return MergeMetadataAttributes(dataMemberAttributes);
				}
			}

			return new MetadataAttribute();
		}

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="property">Type of object to retrieve MetaData for.</param>
		/// <returns>SchemasAttribute</returns>
		private static MetadataAttribute GetMetadataAttribute(this PropertyInfo property)
		{
			if (property != null)
			{
				var dataMemberAttributes = (MetadataAttribute[])property.GetCustomAttributes(typeof(MetadataAttribute), false);
				if (dataMemberAttributes != null && dataMemberAttributes.Any())
				{
					return MergeMetadataAttributes(dataMemberAttributes);
				}
			}

			return new MetadataAttribute();
		}

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="fieldInfo">Type of object to retrieve MetaData for.</param>
		/// <returns>SchemasAttribute</returns>
		private static MetadataAttribute GetMetadataAttribute(this FieldInfo fieldInfo)
		{
			if (fieldInfo != null)
			{
				var dataMemberAttributes = (MetadataAttribute[])fieldInfo.GetCustomAttributes(typeof(MetadataAttribute), false);
				if (dataMemberAttributes != null && dataMemberAttributes.Any())
				{
					return MergeMetadataAttributes(dataMemberAttributes);
				}
			}

			return new MetadataAttribute();
		}

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="eventInfo">Type of object to retrieve MetaData for.</param>
		/// <returns>SchemasAttribute</returns>
		private static MetadataAttribute GetMetadataAttribute(this EventInfo eventInfo)
		{
			if (eventInfo != null)
			{
				var dataMemberAttributes = (MetadataAttribute[])eventInfo.GetCustomAttributes(typeof(MetadataAttribute), false);
				if (dataMemberAttributes != null && dataMemberAttributes.Any())
				{
					return MergeMetadataAttributes(dataMemberAttributes);
				}
			}

			return new MetadataAttribute();
		}

		/// <summary>
		/// Get the name to be displayed
		/// </summary>
		/// <param name="memberInfo">Type of object to retrieve MetaData for.</param>
		/// <returns>SchemasAttribute</returns>
		private static MetadataAttribute GetMetadataAttribute(this MemberInfo memberInfo)
		{
			switch (memberInfo)
			{
				case FieldInfo fieldInfo:
					return fieldInfo.GetMetadataAttribute();
				case PropertyInfo propertyInfo:
					return propertyInfo.GetMetadataAttribute();
				case EventInfo eventInfo:
					return eventInfo.GetMetadataAttribute();
				case ConstructorInfo constructorInfo:
					return constructorInfo.GetMetadataAttribute();
				case MethodInfo methodInfo:
					return methodInfo.GetMetadataAttribute();
				case Type type:
					return type.GetMetadataAttribute();
				case null:
					throw new ArgumentNullException(nameof(memberInfo));
				default:
					throw new NotImplementedException($"{nameof(GetDocumentation)} encountered an unhandled {nameof(MemberInfo)} type: {memberInfo}");
			}
		}

		private static MetadataAttribute MergeMetadataAttributes(MetadataAttribute[] metadataAttributes)
        {
			MetadataAttribute returnMetadata = new MetadataAttribute();
			// Merge multiple MetadataAttributes into one sigle MetadataAttribute 
			foreach (var metaData in metadataAttributes)
			{
				if (string.IsNullOrEmpty(returnMetadata.ApiDomain)) returnMetadata.ApiDomain = metaData.ApiDomain;
				if (string.IsNullOrEmpty(returnMetadata.ApiDescription)) returnMetadata.ApiDescription = metaData.ApiDescription;
				if (string.IsNullOrEmpty(returnMetadata.ApiVersionStatus)) returnMetadata.ApiVersionStatus = metaData.ApiVersionStatus;
				if (string.IsNullOrEmpty(returnMetadata.HttpMethodPermission)) returnMetadata.HttpMethodPermission = metaData.HttpMethodPermission;
				if (string.IsNullOrEmpty(returnMetadata.HttpMethodSummary)) returnMetadata.HttpMethodSummary = metaData.HttpMethodSummary;
				if (string.IsNullOrEmpty(returnMetadata.HttpMethodDescription)) returnMetadata.HttpMethodDescription = metaData.HttpMethodDescription;
				if (string.IsNullOrEmpty(returnMetadata.DataFileName)) returnMetadata.DataFileName = metaData.DataFileName;
				if (string.IsNullOrEmpty(returnMetadata.DataElementName)) returnMetadata.DataElementName = metaData.DataElementName;
				if (returnMetadata.DataMaxLength <= 0 && metaData.DataMaxLength > 0) returnMetadata.DataMaxLength = metaData.DataMaxLength;
				if (string.IsNullOrEmpty(returnMetadata.DataDescription)) returnMetadata.DataDescription = metaData.DataDescription;
				if (string.IsNullOrEmpty(returnMetadata.DataReferenceFileName)) returnMetadata.DataReferenceFileName = metaData.DataReferenceFileName;
				if (string.IsNullOrEmpty(returnMetadata.DataReferenceTableName)) returnMetadata.DataReferenceTableName = metaData.DataReferenceTableName;
				if (string.IsNullOrEmpty(returnMetadata.DataReferenceColumnName)) returnMetadata.DataReferenceColumnName = metaData.DataReferenceColumnName;
				if (!returnMetadata.DataRequired && metaData.DataRequired) returnMetadata.DataRequired = metaData.DataRequired;
				if (!returnMetadata.DataIsInquiryOnly && metaData.DataIsInquiryOnly) returnMetadata.DataIsInquiryOnly = metaData.DataIsInquiryOnly;
			}
			return returnMetadata;
		}

		#endregion
	}
}
