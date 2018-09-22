// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Converters;

namespace Ellucian.Colleague.Dtos.Tests.Converters
{
    [TestClass]
    public class ArrayGuidObject2FilterConverterTests
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        string guid1 = Guid.NewGuid().ToString();
        string guid2 = Guid.NewGuid().ToString();

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
        public void ArrayGuidObject2FilterConverter_MultivalueArray_Valid()
        {
            var criteria = string.Concat("[{ \"id\":\"", guid1, "\" }, { \"id\":\"", guid2, "\" }]");

            var filterConverter = new ArrayGuidObject2FilterConverter();

            var rawFilterData = JsonConvert.DeserializeObject<List<GuidObject2>>(criteria,
                filterConverter);

            Assert.IsNotNull(rawFilterData);
            Assert.IsTrue(rawFilterData.GetType() == typeof(List<GuidObject2>));
            Assert.IsTrue(!string.IsNullOrEmpty(rawFilterData[0].Id));
            Assert.IsTrue(!string.IsNullOrEmpty(rawFilterData[1].Id));
            Assert.AreEqual(guid1, rawFilterData[0].Id);
            Assert.AreEqual(guid2, rawFilterData[1].Id);
        }

        [TestMethod]
        public void ArrayGuidObject2FilterConverter_SingleValue_Valid()
        {
            var criteria = string.Concat("\"", guid1, "\"");

            var filterConverter = new ArrayGuidObject2FilterConverter();

            var rawFilterData = JsonConvert.DeserializeObject<List<GuidObject2>>(criteria,
                filterConverter);

            Assert.IsNotNull(rawFilterData);
            Assert.IsTrue(rawFilterData.GetType() == typeof(List<GuidObject2>));
            Assert.IsTrue(!string.IsNullOrEmpty(rawFilterData[0].Id));
            Assert.AreEqual(guid1, rawFilterData[0].Id);
           
        }
    }
}