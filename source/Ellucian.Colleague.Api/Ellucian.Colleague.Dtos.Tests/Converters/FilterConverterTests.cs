// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Converters;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json.Converters;


namespace Ellucian.Colleague.Dtos.Tests.Converters
{
    [TestClass]
    public class FilterConverterTests
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        [TestInitialize]
        public void SetupCustomConverter()
        {

            jsonSettings.TypeNameHandling = TypeNameHandling.All;
            jsonSettings.DefaultValueHandling = DefaultValueHandling.Include;
            jsonSettings.Formatting = Formatting.None;
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonSettings.NullValueHandling = NullValueHandling.Include;
            jsonSettings.ObjectCreationHandling = ObjectCreationHandling.Auto;
            jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
            jsonSettings.CheckAdditionalContent = false;
        }

        [TestMethod]
        public void ReadJsonTest_ValidString()
        {
            var criteria = "{\"title\":\"anyvalue\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_InvalidFilter()
        {
            var criteria = "{\"invalid\":\"inactive\"}";

            var filterConverter = new FilterConverter("invalid");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_ValidProperty_NotInFilter()
        {
            var criteria = "{\"noFilter\":\"noFilter\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_ValidProperty_NotInFilter_ContainsConverter()
        {
            var criteria = "{\"dateOfBirth\":\"2017-01-01\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "Property");

            var retval = filterConverter.GetInvalidFilterErrorMessage();
            Assert.AreEqual("'dateOfBirth' is an invalid property on the schema. ", retval);
        }

        [TestMethod]
        public void ReadJsonTest_InvalidProperty()
        {
            var criteria = "{\"invalidProperty\":\"inactive\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used          
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "invalidProperty");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "Property");

            var retval = filterConverter.GetInvalidFilterErrorMessage();
            Assert.AreEqual("'invalidProperty' is an invalid property on the schema. ", retval);
        }

        [TestMethod]
        public void ReadJsonTest_InvalidEnumeration()
        {
            var criteria = "{\"status\":\"invalidEnumeration\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "status");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "Enumeration");

            var retval = filterConverter.GetInvalidFilterErrorMessage();
            Assert.AreEqual("'invalidEnumeration' is an invalid enumeration value. ", retval);

        }

        [TestMethod]
        public void ReadJsonTest_InvalidEnumeration_NonNullable()
        {
            var criteria = "{\"statusNonNullable\":\"invalidEnumeration\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "statusNonNullable");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "Enumeration");

            var retval = filterConverter.GetInvalidFilterErrorMessage();
            Assert.AreEqual("'invalidEnumeration' is an invalid enumeration value. ", retval);

        }

        [TestMethod]
        public void ReadJsonTest_ValidEnumeration()
        {
            var criteria = "{\"status\":\"closed\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);

        }

        [TestMethod]
        public void ReadJsonTest_ValidEnumeration_NonNullable()
        {
            var criteria = "{\"statusNonNullable\":\"closed\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);

        }


        [TestMethod]
        public void ReadJsonTest_ValidEnumerationCaseInsensitive()
        {
            var criteria = "{\"status\":\"Closed\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);

        }


        [TestMethod]
        public void ReadJsonTest_ValidEnumeration_NoEnumMember()
        {
            var criteria = "{\"status\":\"Cancelled\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_ValidEnumeration_NonNulbale_NoEnumMember()
        {
            var criteria = "{\"statusNonNullable\":\"Cancelled\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);

        }

        [TestMethod]
        public void ReadJsonTest_ValidGuidObject2()
        {
            var criteria = string.Format("{{\"course\": {{\"id\":\"{0}\"}}}}", Guid.NewGuid());

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);

        }

        
        [TestMethod]
        public void ReadJsonTest_Invalid_GuidObject2()
        {
            var criteria = string.Format("{{\"course\": {{\"kitten\":\"{0}\"}}}}", Guid.NewGuid());

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "Property");

            var retval = filterConverter.GetInvalidFilterErrorMessage();
            Assert.AreEqual("'course.kitten' is an invalid property on the schema. ", retval);

        }
        

        [TestMethod]
        public void ReadJsonTest_ValidArray()
        {
            var criteria = string.Format("{{\"academicLevels\":  [ {{\"id\":\"{0}\"}} ] }}", Guid.NewGuid());

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);

        }
        [TestMethod]
        public void ReadJsonTest_ValidArrayMultipleValues()
        {
            var criteria = string.Format("{{\"academicLevels\":  [ {{\"id\":\"{0}\"}}, {{\"id\":\"{1}\"}} ] }}",
                Guid.NewGuid(), Guid.NewGuid());

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_ParentArray_MultipleValues_Invalid()
        {
            var criteria = string.Format("{{\"academicLevels\":  [ {{\"invalid\":\"{0}\"}}, {{\"id\":\"{1}\"}} ] }}",
                Guid.NewGuid(), Guid.NewGuid());

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
            
        }

        [TestMethod]
        public void ReadJsonTest_EmptyProperty_Enumeration()
        {
            var criteria = "{\"status\":\"\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_EmptyProperty_NestedEnumeration()
        {
            var criteria = "{\"testDtoProperty\": { \"testEnum\":\"\" }}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsTrue(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_EmptyProperty_GuidObject()
        {
            var criteria = "{\"course\": { \"id\":\"\" }}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsTrue(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_EmptyProperty_Array()
        {
            var criteria = string.Format("{{\"academicLevels\":  [ {{\"id\":\"{0}\"}} ] }}",
                "");

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_EmptyProperty_DateTime()
        {
            var criteria = "{\"startOn\":\"\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsTrue(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_NotEmptyProperty_DateTime()
        {
            var criteria = "{\"startOn\":\"2017-01-01\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_NotEmptyProperty_DateTime_SupportedOperator()
        {

            var criteria = "{\"endOn\":{\"$eq\":\"1989-08-15\"}}";


            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
        }


        [TestMethod]
        public void ReadJsonTest_NotEmptyProperty_DateTime_NotSupportedOperator()
        {
            
            var criteria = "{\"endOn\":{\"$invalid\":\"1989-08-15\"}}";


            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "endOn");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "UnsupportedFilterOperator");
        }

        [TestMethod]
        public void ReadJsonTest_NotEmptyProperty_DateTime_InvalidFilterOperator()
        {

            var criteria = "{\"endOn\":{\"$EQ\":\"1989-08-15\"}}";


            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "endOn");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "InvalidFilterOperator");
        }



        [TestMethod]
        public void ReadJsonTest_Invalid_DateTimeOffset()
        {
            var criteria = "{\"startOn\":\"cat\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_Valid_DateTime()
        {
            var criteria = "{\"endOn\":\"2000-03-01\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_Invalid_DateTime_1()
        {
            var criteria = "{\"endOn\":\"cat\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_Invalid_DateTime_2()
        {
            var criteria = "{\"endOn\":\"2015-01-33\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_EmptyProperty_String()
        {
            var criteria = "{\"title\":\"\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsTrue(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_NotEmptyProperty_String()
        {
            var criteria = "{\"title\":\"\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_EmptyNestedObject()
        {
            var criteria = string.Format("{{\"course\": {{\"detail\": {{\"id\":\"{0}\"}} }} }}", "");

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_ListDtoProperty_Valid()
        {
            var criteria = string.Format("{{\"testListDtoProperty\":  [ {{\"name1\":\"{0}\"}} ] }}", Guid.NewGuid());

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_ListDtoProperty_Invalid()
        {
            var criteria = string.Format("{{\"testListDtoProperty\":  [ {{\"invalid\":\"{0}\"}} ] }}", Guid.NewGuid());

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "testListDtoProperty");

        }


        [TestMethod]
        public void ReadJsonTest_ChildDtoProperty_Valid()
        {
            var criteria = "{\"testChildDtoProperty\": { \"name1\":\"bob\" }}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);


            Assert.IsFalse(filterConverter.ContainsEmptyFilterProperties);
        }

        [TestMethod]
        public void ReadJsonTest_ChildDtoProperty_Invalid()
        {
            var criteria = "{\"testChildDtoProperty\": { \"invalid\":\"bob\" }}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "testChildDtoProperty");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3, FilterConverter.InvalidFilterType.Property);

        }

        [TestMethod]
        public void ReadJsonTest_ChildDtoProperty_ValidPropertyNotInFilter()
        {
            var criteria = "{\"testChildDtoProperty\": { \"name2\":\"bob\" }}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "name2");
        }

        [TestMethod]
        public void ReadJsonTest_TestEnumNoFilter()
        {
            var criteria = "{\"testEnumNoFilter\":\"closed\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        


        [TestMethod]
        public void ReadJsonTest_InvalidDataType()
        {
            var criteria = "{\"title\":78}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "title");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "DataType");

            var retval = filterConverter.GetInvalidFilterErrorMessage();
            Assert.AreEqual("'title' is an invalid data type. ", retval);
        }

        [TestMethod]
        public void ReadJsonTest_InvalidDataType_GuidObject_1()
        {
            var criteria = "{\"course\":\"invalid\"}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            //invalidFilters will be populated if an invalid value is used
            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "course");
            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item3.ToString(), "DataType");

            var retval = filterConverter.GetInvalidFilterErrorMessage();
            Assert.AreEqual("'course' is an invalid data type. ", retval);
        }

   
        [TestMethod]
        public void ReadJsonTest_ListDtoProperty_Multivalue_NotInFilterGroup()
        {
            var criteria = string.Format("{{\"testListDtoProperty\":  [ {{\"name1\":\"{0}\"}} , {{\"name2\":\"{0}\"}}] }}", "bob");

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_ListDtoProperty_Multivalue()
        {
            var criteria = string.Format("{{\"testListDto2Property\":  [ {{\"name1\":\"{0}\"}} , {{\"name2\":\"{0}\"}}] }}", "bob");

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_List_Enum_DtoProperty_Multivalue()
        {
           var criteria = "{\"statuses\":[ \"open\",\"closed\" ]}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_List_Enum_DtoProperty_Multivalue_Invalid()
        {          
            var criteria = "{\"statuses\":[ \"open\",\"kitten\" ]}";

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_CredentialsDtoProperty_Multivalue()
        {
            var criteria = string.Format("{{\"testCredentials\":  [ {{\"type\":\"{0}\"}} , {{\"value\":\"{1}\"}}] }}", "ssn", "123-45-6789");

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsFalse(filterConverter.ContainsInvalidFilterProperties());
        }

        [TestMethod]
        public void ReadJsonTest_CredentialsDtoProperty_Invalid()
        {
            var criteria = string.Format("{{\"testCredentials\":  [ {{\"type\":\"{0}\"}} , {{\"invalid\":{1}}}] }}", "ssn", "123456789");

            var filterConverter = new FilterConverter("criteria");

            var rawFilterData = JsonConvert.DeserializeObject<TestDto>(criteria,
                filterConverter);

            Assert.IsTrue(filterConverter.ContainsInvalidFilterProperties());

            Assert.AreEqual(filterConverter.InvalidFilterProperties.Count, 1);

            Assert.AreEqual(filterConverter.InvalidFilterProperties[0].Item1, "testCredentials");
        }

    }

    [DataContract]
    public class TestDto
    {
        /// <summary>
        /// Date of birth
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("dateOfBirth", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? BirthDate { get; set; }

        [DataMember(Name = "title")]
        [FilterProperty("criteria")]
        public string Title { get; set; }

        [DataMember(Name = "startOn", EmitDefaultValue = false)]
        [JsonProperty("startOn", NullValueHandling = NullValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public DateTimeOffset? StartOn { get; set; }

        [DataMember(Name = "endOn", EmitDefaultValue = false)]
        [JsonProperty("endOn", NullValueHandling = NullValueHandling.Ignore)]       
        [FilterProperty("criteria", new string[] { "$eq", "$gte", "$lte" })]
        public DateTime? EndOn { get; set; }

        [DataMember(Name = "course", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        public TestGuidObject Course { get; set; }

        [DataMember(Name = "academicLevels", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        public List<TestGuidObject> AcademicLevels { get; set; }

        [DataMember(Name = "status", EmitDefaultValue = false)]
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public TestEnum? Status { get; set; }

        [DataMember(Name = "status", EmitDefaultValue = false)]
        [JsonProperty("statusNonNullable", NullValueHandling = NullValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public TestEnum StatusNonNullable { get; set; }


        [DataMember(Name = "statuses", EmitDefaultValue = false)]
        [JsonProperty("statuses", NullValueHandling = NullValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public List<TestEnum?> Statuses { get; set; }

        [DataMember(Name = "testDtoProperty", EmitDefaultValue = false)]
        [JsonProperty("testDtoProperty", NullValueHandling = NullValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public TestDtoProperty TestDtoProperty { get; set; }

        [DataMember(Name = "testDto2Property", EmitDefaultValue = false)]
        [JsonProperty("testDto2Property", NullValueHandling = NullValueHandling.Ignore)]
        [FilterProperty("criteria")]
        public TestDto2Property TestDto2Property { get; set; }


        [DataMember(Name = "testListDtoProperty", EmitDefaultValue = false)]
        [JsonProperty("testListDtoProperty", NullValueHandling = NullValueHandling.Ignore)]
        public List<TestDtoProperty> TestListDtoProperty { get; set; }

        [DataMember(Name = "testListDto2Property", EmitDefaultValue = false)]
        [JsonProperty("testListDto2Property", NullValueHandling = NullValueHandling.Ignore)]
        public List<TestDto2Property> TestListDto2Property { get; set; }


        [DataMember(Name = "testChildDtoProperty", EmitDefaultValue = false)]
        [JsonProperty("testChildDtoProperty", NullValueHandling = NullValueHandling.Ignore)]
        public TestDtoProperty TestChildDtoProperty { get; set; }

        [DataMember(Name = "noFilter")]
        public string PropertyWithNoFilter { get; set; }

        [JsonProperty("testEnumNoFilter", NullValueHandling = NullValueHandling.Ignore)]
        public TestEnum? TestEnumNoFilter { get; set; }
        
        [JsonProperty("testCredentials", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<TestCredentialDtoProperty> TestCredentials { get; set; }
        

        public TestDto() : base()
        {
            AcademicLevels = new List<TestGuidObject>();
        }
    }

    /// <summary>
    /// A GUID container
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TestGuidObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonConstructor]
        public TestGuidObject()
        {
        }

        public TestGuidObject(string id)
        {
            Id = id;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TestDtoProperty
    {
        [JsonProperty("testEnum")]
        public TestEnum? TestEnum { get; set; }

        [JsonProperty("name1")]
        [FilterProperty("criteria")]
        public string Name1 { get; set; }

        [JsonProperty("name2")]
        public string Name2 { get; set; }

        [JsonConstructor]
        public TestDtoProperty() { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TestDto2Property
    {
        [JsonProperty("testEnum")]
        public TestEnum? TestEnum { get; set; }

        [JsonProperty("name1")]
        [FilterProperty("criteria")]
        public string Name1 { get; set; }

        [JsonProperty("name2")]
        [FilterProperty("criteria")]
        public string Name2 { get; set; }

        [JsonConstructor]
        public TestDto2Property() { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TestCredentialDtoProperty
    {       
        [JsonProperty("type")]
        [FilterProperty("criteria")]
        public Dtos.EnumProperties.CredentialType2? Type { get; set; }
      
        [FilterProperty("criteria")]
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("startOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? StartOn { get; set; }

        [JsonProperty("endOn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset? EndOn { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TestEnum
    {
        NotSet,
        [EnumMember(Value = "closed")]
        Closed,
        [EnumMember(Value = "open")]
        Open,
        [EnumMember(Value = "pending")]
        Pending,
        Cancelled
    }
}
